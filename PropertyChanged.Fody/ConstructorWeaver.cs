namespace PropertyChanged.Fody;

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
    readonly TypeNode typeNode;

    public ConstructorWeaver(TypeNode typeNode) { this.typeNode = typeNode; }

    /// <summary>Executes weaver for pre-configured <see cref="typeNode"/>.</summary>
    public void Execute()
    {
        foreach (var constructor in typeNode.TypeDefinition.GetConstructors())
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
        var               instructions              = constructor.Body.Instructions;
        var               fieldAssignmentStartIndex = 0;
        List<Instruction> movedInstructions         = null; // will contains instructions moved after constructor, for implicit initialization fields initialized before base constructor call, but for property setter calls these should be after base constructor call
        var               currentIndex              = 0;
        // iterate over all instructions
        while (currentIndex < instructions.Count)
        {
            var instruction = instructions[currentIndex];
            // checks if instruction is constructor call, in that case process and break because all inline fields initialized before constructor call
            if (IsConstructorCall(instruction))
            {
                ProcessConstructorCall(instructions, currentIndex, movedInstructions);
                return;
            }
            
            // if it is a field assignment then process it and move to movedInstructions if is one of notified properties fields
            if (instruction.OpCode == OpCodes.Stfld)
            {
                currentIndex              = ProcessFieldAssignment(instructions, fieldAssignmentStartIndex, currentIndex, ref movedInstructions); // will be set to next instruction index to process
                fieldAssignmentStartIndex = currentIndex;
            }
            else // otherwise just go to next instruction
                ++currentIndex;
        }
    }

    /// <summary>Checks if <paramref name="instruction"/> is a constructor call.</summary>
    bool IsConstructorCall(Instruction instruction) => instruction.OpCode == OpCodes.Call && instruction.Operand is MethodDefinition { IsConstructor: true };


    /// <summary>Process constructor call. Basically inserts all <paramref name="movedInstructions"/> after constructor call.</summary>
    void ProcessConstructorCall(Collection<Instruction> instructions, int currentIndex, List<Instruction> movedInstructions)
    {
        if (movedInstructions == null) return;
        var insertIndex = currentIndex + 1;
        foreach (var instruction in movedInstructions)
            instructions.Insert(insertIndex++, instruction);
    }

    /// <summary>Process field assignment. If it is a backing field for one of notified properties then it replaces field assignment with setter call and moves whole assignment to <paramref name="movedInstructions"/>.</summary>
    int ProcessFieldAssignment(Collection<Instruction> instructions, int fieldAssignmentStartIndex, int currentIndex, ref List<Instruction> movedInstructions)
    {
        var field = (FieldReference)instructions[currentIndex].Operand;
        if (!field.IsBackingField()) return currentIndex + 1; // only care about backing fields
        
        var propertyData = typeNode.PropertyDatas.FirstOrDefault(pd => pd.Value.BackingFieldReference == field).Value;
        if (propertyData == null) return currentIndex + 1;    // ignore non-notified properties backing fields
        
        movedInstructions ??= new();
        MoveInstructions(instructions, fieldAssignmentStartIndex, currentIndex, movedInstructions);            // move whole field assignment to movedInstructions before store field
        var setter = propertyData.PropertyDefinition.SetMethod;
        movedInstructions.Add(Instruction.Create(setter.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, setter)); // and then replace store field operation with setter call 
        instructions.RemoveAt(fieldAssignmentStartIndex);
        return fieldAssignmentStartIndex;
    }

    /// <summary>Moves instruction range from one collection to another.</summary>
    void MoveInstructions(Collection<Instruction> instructions, int start, int end, List<Instruction> targetInstructions)
    {
        for (var index = start; index < end; ++index)
        {
            // always work with start index because instructions shifted when removed
            targetInstructions.Add(instructions[start]);
            instructions.RemoveAt(start);
        }
    }
}
