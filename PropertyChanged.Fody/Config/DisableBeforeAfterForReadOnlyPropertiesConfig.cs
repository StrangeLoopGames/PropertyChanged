using System.Linq;
using System.Xml;

public partial class ModuleWeaver
{
    /// <summary>
    /// When value set then before/after values for read-only properties won't be calculated, but instead callback will be invoked with default values.
    /// It is useful when calculated properties may fail when object not fully initialized during deserialization or creation and you really don't care about these calculated properties changes, but aware about settable properties only.
    /// Readonly properties may restore default behavior with help of [ForceBeforeAfter] attribute. In that case real before/after values always submitted no matter of setting value.
    /// </summary>
    public bool DisableBeforeAfterForReadOnlyProperties;

    /// <summary>Resolves value for <see cref="DisableBeforeAfterForReadOnlyProperties"/> from XML config.</summary>
    public void ResolveDisableBeforeAfterForReadOnlyPropertiesConfig()
    {
        var value = Config?.Attributes("DisableBeforeAfterForReadOnlyProperties")
            .Select(a => a.Value)
            .SingleOrDefault();
        if (value != null)
        {
            DisableBeforeAfterForReadOnlyProperties = XmlConvert.ToBoolean(value.ToLowerInvariant());
        }
    }
}