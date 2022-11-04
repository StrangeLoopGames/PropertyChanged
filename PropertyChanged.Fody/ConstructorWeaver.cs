namespace PropertyChanged.Fody;

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

/// <summary>
/// Weaves constructor for Fody notified type.
/// Replaces auto-properties initialization for notified properties with calls to property setter to ensure notifications works same way as when properties explicitly initialized in constructor.
/// </summary>
public readonly struct ConstructorWeaver
{
    readonly ModuleWeaver weaver;
    readonly TypeNode     typeNode;

    public ConstructorWeaver(ModuleWeaver weaver, TypeNode typeNode)
    {
        this.weaver   = weaver;
        this.typeNode = typeNode;
    }

    /// <summary>Executes weaver for pre-configured <see cref="typeNode"/>.</summary>
    public void Execute()
    {
        foreach (var constructor in typeNode.TypeDefinition.GetConstructors().Where(ctr => !ctr.IsStatic))
            WeaveConstructor(constructor);
    }

    /// <summary>
    /// Weaves a constructor for auto-properties inline initialization.
    /// <example><code><![CDATA[
    /// public class ABC
    /// {
    ///     public int A { get; set; } = 42;
    /// }
    /// ]]></code>
    /// weaved as
    /// <code><![CDATA[
    /// public class ABC
    /// {
    ///     public int A { get; set; }
    /// 
    ///     public ABC()
    ///     {
    ///        this.A = 42;
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    /// </summary>
    void WeaveConstructor(MethodDefinition constructor)
    {
        var constructorBody      = constructor.Body;
        var instructions         = constructorBody.Instructions;
        var constructorCallIndex = FindConstructorCallIndex(instructions);
        if (constructorCallIndex < 0)
            return;

        var constructorCall = instructions[constructorCallIndex];
        var insertIndex     = constructorCallIndex + 1; // set insert index right after constructor, because all setters should be invoked after base constructor call
        
        var fieldAssignmentStartIndex = 0;
        var currentIndex              = 0;

        // iterate over all instructions
        while (true)
        {
            var instruction = instructions[currentIndex];
            if (instruction == constructorCall)
                break;

            // if it is a field assignment then process it and move to movedInstructions if is one of notified properties fields
            if (instruction.OpCode == OpCodes.Stfld && TryGetNotifyProperty((FieldReference)instruction.Operand, out var property))
            {
                insertIndex  = ProcessFieldAssignment(constructorBody, instructions, insertIndex, fieldAssignmentStartIndex, currentIndex, property); // moves property notification to insert index and returns new insert index
                currentIndex = fieldAssignmentStartIndex;                                                                                             // ProcessFieldAssignment moves all field assignment block after constructor so need to reset current index
            }
            else                                                                                                                                      // otherwise just go to next instruction
                ++currentIndex;
        }
    }

    /// <summary>Returns index of constructor call.</summary>
    int FindConstructorCallIndex(Collection<Instruction> instructions)
    {
        var index = 0;
        foreach (var instruction in instructions)
        {
            if (IsConstructorCall(instruction))
                return index;
            ++index;
        }

        return -1;
    }

    /// <summary>Checks if <paramref name="instruction"/> is a constructor call.</summary>
    static bool IsConstructorCall(Instruction instruction) => instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference { Name: ".ctor" };
    
    /// <summary>Process constructor call. Basically inserts all <paramref name="movedInstructions"/> after constructor call.</summary>
    void ProcessConstructorCall(Collection<Instruction> instructions, int currentIndex, List<Instruction> movedInstructions)
    {
        if (movedInstructions == null) return;
        var insertIndex = currentIndex + 1;
        foreach (var instruction in movedInstructions)
            instructions.Insert(insertIndex++, instruction);
    }
    
    /// <summary>Process field assignment. If it is a backing field for one of notified properties then it replaces field assignment with setter call and moves whole assignment tp <paramref name="insertIndex"/>, because implicit fields assigned before constructor, but all properties setters should be invoked after base constructor.</summary>
    int ProcessFieldAssignment(MethodBody constructorBody, Collection<Instruction> instructions, int insertIndex, int fieldAssignmentStartIndex, int storeFieldIndex, PropertyDefinition property)
    {
        MoveFieldAssignment(instructions, insertIndex, fieldAssignmentStartIndex, storeFieldIndex); // move whole field assignment to insert index, insert index won't change because it should be after field and number of removed instructions same as number of inserted insteructions

        storeFieldIndex = insertIndex - 1; // one step back to point store field instruction
        return property.SetMethod is { } setter ? InsertSetterCall(instructions, storeFieldIndex, setter) : InsertEventInvocation(constructorBody, instructions, storeFieldIndex, property);
    }

    /// <summary>Inserts setter call instead of field assignment.</summary>
    int InsertSetterCall(Collection<Instruction> instructions, int index, MethodDefinition setter)
    {
        instructions.RemoveAt(index);
        instructions.Insert(index, Instruction.Create(setter.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, setter));
        return index + 1;
    }

    /// <summary>Inserts event invocation for <paramref name="propertyDefinition"/> if it was changed.</summary>
    int InsertEventInvocation(MethodBody body, Collection<Instruction> instructions, int index, PropertyDefinition propertyDefinition)
    {
        var invokerType = typeNode.EventInvoker.InvokerType;
        return invokerType switch
        {
            InvokerTypes.String                   => InvokeSimple(instructions, index, propertyDefinition),
            InvokerTypes.BeforeAfter              => InvokeBeforeAfter(body, instructions, index, propertyDefinition, weaver.TypeSystem.ObjectReference),
            InvokerTypes.BeforeAfterGeneric       => InvokeBeforeAfter(body, instructions, index, propertyDefinition, propertyDefinition.PropertyType),
            InvokerTypes.PropertyChangedArg       => InvokePropertyChangedArg(instructions, index, propertyDefinition),
            InvokerTypes.SenderPropertyChangedArg => InvokeSenderPropertyChangedArg(instructions, index, propertyDefinition),
            _                                     => throw new ArgumentOutOfRangeException($"Unsupported invoker type: {invokerType} for {typeNode.TypeDefinition.FullName}")
        };
    }

    int InvokeSimple(Collection<Instruction> instructions, int index, PropertyDefinition property)
    {
        ++index; // just skip stfld because we don't need the value
        index = instructions.Insert(index,
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldstr, property.Name));

        return instructions.Insert(index, PropertyWeaver.CallEventInvoker(typeNode, property.PropertyType).ToArray());
    }

    int InvokeBeforeAfter(MethodBody body, Collection<Instruction> instructions, int index, PropertyDefinition property, TypeReference argType)
    {
        body.InitLocals = true;
        
        // store value to temp variable
        var valueType = property.PropertyType;
        index = instructions.InsertStoreAndLoadVariable(index, valueType, out var valueVariable);
        body.Variables.Add(valueVariable);
        
        ++index; // skip store field operation which will store temp value to the field
        
        VariableDefinition defaultValue = null;                                                                                   // OnPropertyChanged(
        instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));                                                        //   this
        instructions.Insert(index++, Instruction.Create(OpCodes.Ldstr, property.Name));                                           //  ,propertyName
        index = instructions.InsertDefault(index, argType, ref defaultValue);                                                     //  ,default
        instructions.Insert(index++, Instruction.Create(OpCodes.Ldloc, valueVariable));                                           //  ,value
        // ensure value type boxed if needed
        if (!argType.IsValueType && valueType.IsValueType)
            instructions.Insert(index++, Instruction.Create(OpCodes.Box, valueType));
        // in some cases default value may require intermediate local variable
        if (defaultValue != null)
            body.Variables.Add(defaultValue);

        return instructions.Insert(index, PropertyWeaver.CallEventInvoker(typeNode, argType).ToArray());                        // )
    }

    int InvokePropertyChangedArg(Collection<Instruction> instructions, int index, PropertyDefinition property)
    {
        ++index; // just skip stfld because we don't need the value
        index = instructions.Insert(index,
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldsfld, weaver.EventArgsCache.GetEventArgsField(property.Name)));

        return instructions.Insert(index, PropertyWeaver.CallEventInvoker(typeNode, property.PropertyType).ToArray());
    }

    int InvokeSenderPropertyChangedArg(Collection<Instruction> instructions, int index, PropertyDefinition property)
    {
        ++index; // just skip stfld because we don't need the value
        index = instructions.Insert(index,
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldsfld, weaver.EventArgsCache.GetEventArgsField(property.Name)));

        return instructions.Insert(index, PropertyWeaver.CallEventInvoker(typeNode, property.PropertyType).ToArray());
    }

    /// <summary>Returns matching property definition for backing field.</summary>
    bool TryGetNotifyProperty(FieldReference field, out PropertyDefinition property)
    {
        property = null;
        if (!field.IsBackingField()) // only care about backing fields 
            return false;
        
        var mapping = typeNode.Mappings.FirstOrDefault(m => m.FieldDefinition == field);
        if (mapping is not { PropertyDefinition: var propertyDefinition })
            return false;
        
        // it should be either property in PropertyDatas or explicit notify property
        if (!typeNode.PropertyDatas.ContainsKey(propertyDefinition) && !(typeNode.ExplicitNotifyProperties?.Contains(propertyDefinition)).GetValueOrDefault()) 
            return false;
        
        property = propertyDefinition;
        return true;
    }
    
    /// <summary>Moves field assignment to <paramref name="insertIndex"/> which should be greater than <paramref name="fieldAssignmentEnd"/>.</summary>
    void MoveFieldAssignment(Collection<Instruction> instructions, int insertIndex, int fieldAssignmentStart, int fieldAssignmentEnd)
    {
        --insertIndex;                                                            // we remove first, so need to adjust insert index 
        for (var index = fieldAssignmentStart; index <= fieldAssignmentEnd; ++index)
        {
            var instruction = instructions[fieldAssignmentStart]; // always work with start index because instructions shifted when removed
            instructions.RemoveAt(fieldAssignmentStart);                          
            instructions.Insert(insertIndex, instruction);        // always insert at same index because all instructions shifted left when instruction removed
        }
    }
}
