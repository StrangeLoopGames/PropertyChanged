namespace AssemblyWithCustomInvokerInterceptor;

using System.ComponentModel;

public interface ICustomNotifyPropertyChangedInvoker
{
    void InvokePropertyChanged(PropertyChangedEventArgs args);
}