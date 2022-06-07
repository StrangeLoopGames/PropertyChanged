#nullable enable

using System.Linq;
using System.Xml;

public partial class ModuleWeaver
{
    string? GetConfigValue(string optionName) => Config?.Attributes(optionName).Select(a => a.Value).SingleOrDefault();

    /// <summary>Returns boolean value of config option if set. Otherwise returns <c>null</c>.</summary>
    bool? GetConfigBoolean(string optionName) => GetConfigValue(optionName) is { } value ? XmlConvert.ToBoolean(value.ToLowerInvariant()) : null;
    /// <summary>Sets <paramref name="option"/> from config if <paramref name="optionName"/> set in the config. Keep untouched otherwise.</summary>
    void SetFromConfigIfAvailable(ref bool option, string optionName)
    {
        if (GetConfigBoolean(optionName) is {} value)
            option = value;
    }

    /// <summary>Tries to parse <paramref name="xmlValue"/> as boolean. Can't use <see cref="XmlConvert.ToBoolean"/> because it throws exception if value can't be parsed.</summary>
    bool TryParseBoolean(string xmlValue, out bool value)
    {
        (var result, value) = xmlValue.ToLowerInvariant() switch { "1" or "true" => (true, true), "0" or "false" => (true, false), _ => (false, false) };
        return result;
    }
}
