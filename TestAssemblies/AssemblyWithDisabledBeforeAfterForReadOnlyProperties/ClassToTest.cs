using System;
using System.Collections.Generic;
using System.ComponentModel;
using PropertyChanged;

public class ClassToTest : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public List<(string PropertyName, bool HasDefaultValues)> Notified = new();

    public string Trigger { get; set; } // trigger property with setter

    [DependsOn(nameof(Trigger))] public int Int => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public Guid Guid => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public string String => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public object Object => throw new NotImplementedException();

    [DependsOn(nameof(Trigger)), ForceBeforeAfter] public int RealInt => Trigger?.Length ?? 0;
    [DependsOn(nameof(Trigger)), ForceBeforeAfter] public string RealString => Trigger;
    protected virtual void OnPropertyChanged(string propertyName, object before, object after)
    {
        Notified.Add((propertyName, before == null && after == null));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}