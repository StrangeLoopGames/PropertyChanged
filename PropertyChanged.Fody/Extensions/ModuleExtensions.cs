#nullable enable

namespace PropertyChanged.Fody.Extensions;

using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

/// <summary>PropertyChanged.Fody extensions for <see cref="ModuleDefinition"/>.</summary>
public class ModuleExtensions
{
    List<Func<Collection<Instruction>, int, PropertyDefinition, List<PropertyDefinition>, int>>? fieldAssignHandlers; // collection of handlers for field assignment

    /// <summary>Checks if <see cref="ModuleExtensions"/> frozen (can't accept new extensions). Usually it frozen after PropertyChanged weaving already applied.</summary>
    public bool Frozen { get; private set; }

    /// <summary>Handles field assignment instruction. Extension may add custom logic which happens before or after field assignment (i.e. reset cached field).</summary>
    public int HandleFieldAssign(Collection<Instruction> instructions, int index, PropertyData propertyData)
    {
        if (fieldAssignHandlers == null) return index;
        foreach (var handler in fieldAssignHandlers)
            index = handler(instructions, index, propertyData.PropertyDefinition, propertyData.AlsoNotifyFor);
        return index;
    }

    /// <summary>Adds new field assignment handler which executed with list of instructions assigning the field and can modify the list for custom logic.</summary>
    public void AddFieldAssignHandler(Func<Collection<Instruction>, int, PropertyDefinition, List<PropertyDefinition>, int> handler)
    {
        ThrowIfFrozen();
        (fieldAssignHandlers ??= new()).Add(handler);
    }

    /// <summary>Freezes <see cref="ModuleExtensions"/> preventing other modifications.</summary>
    public void Freeze() => Frozen = true;

    /// <summary>Throws exception if <see cref="Frozen"/>.</summary>
    void ThrowIfFrozen()
    {
        if (Frozen)
            throw new InvalidOperationException("Can't add extensions to module already weaved with PropertyChanged. Ensure weavers extending PropertyChanged executes before it.");
    }
}
