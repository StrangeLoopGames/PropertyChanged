#nullable enable

namespace PropertyChanged.Fody.Extensions;

using System.Collections.Concurrent;
using Mono.Cecil;

/// <summary>Manager for PropertyChanged.Fody extensions. Check <see cref="ModuleExtensions"/> for supported extensions.</summary>
public class ExtensionsManager
{
    public static readonly ExtensionsManager Default = new();

    ConcurrentDictionary<ModuleDefinition, ModuleExtensions> extensions = new();

    /// <summary>Gets (or creates on demand) extensions for <paramref name="module"/>.</summary>
    public ModuleExtensions this[ModuleDefinition module] => extensions.TryGetValue(module, out var moduleExtensions) ? moduleExtensions : extensions[module] = new ModuleExtensions();
}
