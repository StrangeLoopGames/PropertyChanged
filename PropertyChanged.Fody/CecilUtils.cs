using Mono.Cecil;

public static class CecilUtils
{
    /// <summary>Makes <see cref="TypeReference"/> from <paramref name="assemblyQualifiedName"/> like "System.Int32, System.Runtime".</summary>
    public static TypeReference MakeTypeReference(string assemblyQualifiedName)
    {
        var assemblySplitIndex = assemblyQualifiedName.IndexOf(',');
        var assemblyReference = AssemblyNameReference.Parse(assemblyQualifiedName.Substring(assemblySplitIndex + 1).TrimStart());
        return MakeTypeReference(assemblyQualifiedName.Substring(0, assemblySplitIndex), assemblyReference);
    }

    /// <summary>Makes <see cref="TypeReference"/> from <paramref name="fullTypeName"/> like "System.Int32" declared in <paramref name="assemblyReference"/>.</summary>
    public static TypeReference MakeTypeReference(string fullTypeName, AssemblyNameReference assemblyReference)
    {
        var namespaceSplitIndex = fullTypeName.LastIndexOf('.');
        return new TypeReference(fullTypeName.Substring(0, namespaceSplitIndex), fullTypeName.Substring(namespaceSplitIndex + 1), null, assemblyReference);
    }
}