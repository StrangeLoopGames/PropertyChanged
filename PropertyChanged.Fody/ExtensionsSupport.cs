#nullable enable

using PropertyChanged.Fody.Extensions;

/// <summary>Adds extensions support (from <see cref="ExtensionsManager"/>) to property weaving.</summary>
public partial class ModuleWeaver
{
    public ExtensionsManager ExtensionsManager { get; set; } = ExtensionsManager.Default;

    internal ModuleExtensions? ModuleExtensions;

    /// <summary>Initializes extensions for <see cref="ModuleWeaver"/>.</summary>
    void InitExtensions()
    {
        if (ModuleDefinition == null!) return;
        ModuleExtensions = ExtensionsManager[ModuleDefinition];
        ModuleExtensions.Freeze();
    }
}
