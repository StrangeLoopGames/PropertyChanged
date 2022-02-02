#nullable enable

using Fody;
using Mono.Cecil;

public static class CecilUtils
{
    /// <summary>Splits assembly qualified name like "System.Int32, System.Runtime" to type name ("System.Int32") and assembly name ("System.Runtime").</summary>
    public static (string, string) SplitTypeAndAssemblyName(string assemblyQualifiedName)
    {
        var assemblySplitIndex = assemblyQualifiedName.IndexOf(',');
        return (assemblyQualifiedName.Substring(0, assemblySplitIndex), assemblyQualifiedName.Substring(assemblySplitIndex + 1).TrimStart());
    }

    /// <summary>Makes <see cref="TypeReference"/> from <paramref name="assemblyQualifiedName"/> like "System.Int32, System.Runtime".</summary>
    public static TypeReference MakeTypeReference(string assemblyQualifiedName)
    {
        var (typeName, assemblyName) = SplitTypeAndAssemblyName(assemblyQualifiedName);
        return MakeTypeReference(typeName, AssemblyNameReference.Parse(assemblyName));
    }

    /// <summary>Makes <see cref="TypeReference"/> from <paramref name="assemblyQualifiedName"/> like "System.Int32, System.Runtime". Assembly reference will be resolved from <paramref name="module"/>.</summary>
    public static TypeReference MakeTypeReference(string assemblyQualifiedName, ModuleDefinition module)
    {
        var (typeName, assemblyName) = SplitTypeAndAssemblyName(assemblyQualifiedName);
        var assemblyReference = module.Assembly.Name.Name == assemblyName ? (IMetadataScope)module : module.GetAssemblyReference(assemblyName) ?? throw new WeavingException($"Can't resolve reference to assembly {assemblyName} for {assemblyQualifiedName}");
        return MakeTypeReference(typeName, assemblyReference);
    }

    /// <summary>Makes <see cref="TypeReference"/> from <paramref name="fullTypeName"/> like "System.Int32" declared in <paramref name="assemblyReference"/>.</summary>
    public static TypeReference MakeTypeReference(string fullTypeName, IMetadataScope assemblyReference)
    {
        var namespaceSplitIndex = fullTypeName.LastIndexOf('.');
        return new TypeReference(fullTypeName.Substring(0, namespaceSplitIndex), fullTypeName.Substring(namespaceSplitIndex + 1), null, assemblyReference);
    }
}