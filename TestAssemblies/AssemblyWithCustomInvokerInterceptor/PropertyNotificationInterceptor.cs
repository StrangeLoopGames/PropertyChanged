using System.ComponentModel;
using AssemblyWithCustomInvokerInterceptor;

public static class PropertyChangedNotificationInterceptor
{
    public static void Intercept(ICustomNotifyPropertyChangedInvoker invoker, string propertyName)
    {
        invoker.InvokePropertyChanged(new PropertyChangedEventArgs(propertyName));
        InterceptCalled = true;
    }

    public static bool InterceptCalled { get; set; }
}