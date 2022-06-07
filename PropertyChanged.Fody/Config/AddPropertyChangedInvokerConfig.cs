#nullable enable

using Mono.Cecil;

public enum PropertyChangedInvokerType
{
    None,
    Default,
    Custom
}

public partial class ModuleWeaver
{
    const string PropertyChangedAssemblyName = "PropertyChanged";

    bool propertyChangedInvokerResolved;
    TypeReference? propertyChangedInvoker;

    /// <summary>
    /// Returns type of <see cref="PropertyChangedInvoker"/>:
    /// - None    - when not set;
    /// - Default - when PropertyChanged version used;
    /// - Custom  - when custom interface from weaved assembly used.
    /// </summary>
    PropertyChangedInvokerType PropertyChangedInvokerType => PropertyChangedInvoker is { } invoker
        ? (invoker.Scope.Name == PropertyChangedAssemblyName ? PropertyChangedInvokerType.Default : PropertyChangedInvokerType.Custom)
        : PropertyChangedInvokerType.None;

    /// <summary>Should it inject INotifyPropertyChangedInvoker interface with <see cref="ProcessPropertyChangedInvoker"/>. If set to <c>true</c> then reference wouldn't be clean and you shouldn't use PrivateAssets='all' in package reference.</summary>
    public TypeReference? PropertyChangedInvoker
    {
        get
        {
            if (propertyChangedInvokerResolved) return propertyChangedInvoker;
            if (GetConfigValue("AddPropertyChangedInvoker") is { } value)                           // check if value presents in the config
            {
                if (!TryParseBoolean(value, out var addPropertyChangedInvoker))                     
                    propertyChangedInvoker = CecilUtils.MakeTypeReference(value, ModuleDefinition); // if failed to parse as boolean then assume it is an assembly qualified type name
                else if (addPropertyChangedInvoker)                                                 // else if value is boolean and is true then use standard interface
                    propertyChangedInvoker = new TypeReference("PropertyChanged", "INotifyPropertyChangedInvoker", null, ModuleDefinition.GetAssemblyReference(PropertyChangedAssemblyName));
            }
            propertyChangedInvokerResolved = true;
            return propertyChangedInvoker;
        }
        set
        {
            propertyChangedInvoker = value;
            propertyChangedInvokerResolved = true;
        }
    }

}
