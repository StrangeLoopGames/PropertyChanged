namespace PropertyChanged;

using System.ComponentModel;

/// <summary>Allows to invoke PropertyChanged by external code. Auto-implemented by Fody if `AddPropertyChangedInvoker="true"` in XML config.</summary>
public interface INotifyPropertyChangedInvoker
{
    /// <summary>Invokes <c>PropertyChanged?.Invoke(args)</c>.</summary>
    void InvokePropertyChanged(PropertyChangedEventArgs args);
}