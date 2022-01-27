using System;
using System.Collections.Generic;
using System.ComponentModel;
using PropertyChanged;

public class ClassToTestGeneric : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public List<(string PropertyName, bool HasDefaultValues)> Notified = new();

    public string Trigger { get; set; } // trigger property with setter

    #region Primitive types
    [DependsOn(nameof(Trigger))] public byte Byte => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public sbyte SByte => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public short Short => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public ushort UShort => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public int Int => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public uint UInt => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public long Long => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public ulong ULong => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public float Float => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public double Double => throw new NotImplementedException();
    #endregion

    #region Struct types
    [DependsOn(nameof(Trigger))] public Guid Guid => throw new NotImplementedException();
    #endregion

    #region Class types
    [DependsOn(nameof(Trigger))] public string String => throw new NotImplementedException();
    [DependsOn(nameof(Trigger))] public object Object => throw new NotImplementedException();
    #endregion

    #region Forced properties
    [DependsOn(nameof(Trigger)), ForceBeforeAfter] public int RealInt => Trigger?.Length ?? 0;
    [DependsOn(nameof(Trigger)), ForceBeforeAfter] public string RealString => Trigger;
    #endregion

    protected virtual void OnPropertyChanged<T>(string propertyName, T before, T after)
    {
        Notified.Add((propertyName, EqualityComparer<T>.Default.Equals(before, default) && EqualityComparer<T>.Default.Equals(after, default)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}