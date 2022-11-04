using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

public static class InstructionListExtensions
{
    public static void Prepend(this Collection<Instruction> collection, params Instruction[] instructions)
    {
        for (var index = 0; index < instructions.Length; index++)
        {
            var instruction = instructions[index];
            collection.Insert(index, instruction);
        }
    }

    public static void Append(this Collection<Instruction> collection, params Instruction[] instructions)
    {
        foreach (var instruction in instructions)
        {
            collection.Add(instruction);
        }
    }

    public static int Insert(this Collection<Instruction> collection, int index, params Instruction[] instructions)
    {
        foreach (var instruction in instructions.Reverse())
        {
            collection.Insert(index, instruction);
        }
        return index + instructions.Length;
    }

    /// <summary>
    /// Inserts default value of <paramref name="valueType"/> at <paramref name="index"/> and returns first index after last inserted instruction.
    /// If <paramref name="valueType"/> is non-primitive value type then <paramref name="variable"/> will be either created and initialized or reused if already exists. If you need to insert multiple values of same type then you can reuse variable.
    /// If <paramref name="variable"/> isn't null then it should be added to method variables.
    /// </summary>
    public static int InsertDefault(this Collection<Instruction> collection, int index, TypeReference valueType, ref VariableDefinition variable)
    {
        if (valueType.IsValueType)
            index = collection.InsertStructTypeDefault(index, valueType, ref variable);
        else
            collection.Insert(index++, Instruction.Create(OpCodes.Ldnull));

        return index;
    }

    /// <summary>Shortcut for Stloc and Ldloc opcodes with new variable creation.</summary>
    public static int InsertStoreAndLoadVariable(this Collection<Instruction> collection, int index, TypeReference variableType, out VariableDefinition variable)
    {
        variable = new VariableDefinition(variableType);
        collection.Insert(index++, Instruction.Create(OpCodes.Stloc, variable));
        collection.Insert(index++, Instruction.Create(OpCodes.Ldloc, variable));
        return index;
    }

    /// <summary>
    /// Inserts default value of <paramref name="valueType"/> at <paramref name="index"/> and returns first index after last inserted instruction (only works with struct types, should be used via <see cref="InsertDefault"/>).
    /// If <paramref name="valueType"/> is non-primitive value type then <paramref name="variable"/> will be either created and initialized or reused if already exists. If you need to insert multiple values of same type then you can reuse variable.
    /// If <paramref name="variable"/> isn't null then it should be added to method variables.
    /// </summary>
    static int InsertStructTypeDefault(this Collection<Instruction> collection, int index, TypeReference valueType, ref VariableDefinition variable)
    {
        switch (valueType.Name)
        {
            case "SByte":
            case "Int16":
            case "Int32":
            case "Byte":
            case "UInt16":
            case "UInt32":
            case "Boolean":
                collection.Insert(index++, Instruction.Create(OpCodes.Ldc_I4_0));
                break;
            case "Int64":
            case "UInt64":
                collection.Insert(index++, Instruction.Create(OpCodes.Ldc_I4_0));
                collection.Insert(index++, Instruction.Create(OpCodes.Conv_I8));
                break;
            case "Single":
                collection.Insert(index++, Instruction.Create(OpCodes.Ldc_R4, 0f));
                break;
            case "Double":
                collection.Insert(index++, Instruction.Create(OpCodes.Ldc_R8, 0.0));
                break;
            default:
                if (variable == null)
                {
                    variable = new VariableDefinition(valueType);
                    collection.Insert(index++, Instruction.Create(OpCodes.Ldloca_S, variable));
                    collection.Insert(index++, Instruction.Create(OpCodes.Initobj, valueType));
                }
                collection.Insert(index++, Instruction.Create(OpCodes.Ldloc, variable));
                break;
        }

        return index;
    }
}
