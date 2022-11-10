using System.Collections.Generic;
using System.ComponentModel;
using PropertyChanged;

/// <summary>Test class for processors working with property attributes (i.e. NoOwnNotifyProcessing).</summary>
public class ClassBeforeAfterGenericWithAttributedProperties : INotifyPropertyChanged
{
    public List<string>                       Notifications = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    [DisplayName]                        public string? DisplayName             { get; set; }
    [Description]                        public string? Description             { get; set; }
    [AlsoNotifyFor(nameof(DisplayName))] public string? NoOwnNotify             { get; set; }
    [DisplayName]                        public string? DisplayNameAuto         { get; set; } = "Auto";
    public                                      string? NoOwnNotifyAuto         { get; set; } = "Auto";
    [DisplayName] public                        int     DisplayNameAutoReadOnly { get; }      = 42;
    public                                      int     NoOwnNotifyAutoReadOnly { get; }      = 42;

    public void OnNoOwnNotifyChanged() => Notifications.Add("method:NoOwnNotify");

    void OnPropertyChanged<T>(string name, T before, T after) => Notifications.Add($"event:{name}:{before}:{after}");
}
