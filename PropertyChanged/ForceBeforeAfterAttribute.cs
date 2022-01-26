namespace PropertyChanged;

using System;

/// <summary>Suppresses effect of `DisableBeforeAfterForReadOnlyProperties` config option for read-only properties. If the attribute is set then the property will always be notified with real before/after values instead of default values.</summary>
public class ForceBeforeAfterAttribute : Attribute
{
}