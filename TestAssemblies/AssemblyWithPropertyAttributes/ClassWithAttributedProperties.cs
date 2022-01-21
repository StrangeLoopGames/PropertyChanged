using System.Collections.Generic;
using System.ComponentModel;
using PropertyChanged;

/// <summary>Test class for processors working with property attributes (i.e. NoOwnNotifyProcessing).</summary>
public class ClassWithAttributedProperties : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public List<string> Notifications = new List<string>();

    [DisplayName] public string? DisplayName { get; set; }
    [Description] public string? Description { get; set; }
    [AlsoNotifyFor(nameof(DisplayName))] public string? NoOwnNotify { get; set; }

    public ClassWithAttributedProperties() => PropertyChanged += (_, args) => Notifications.Add($"event:{args.PropertyName}");

    public void OnNoOwnNotifyChanged() => Notifications.Add("method:NoOwnNotify");
}