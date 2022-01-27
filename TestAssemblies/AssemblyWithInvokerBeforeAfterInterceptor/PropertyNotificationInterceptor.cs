﻿using System.ComponentModel;
using PropertyChanged;

public static class PropertyChangedNotificationInterceptor
{
    public static void Intercept(INotifyPropertyChangedInvoker invoker, string propertyName, object before, object after)
    {
        invoker.InvokePropertyChanged(new PropertyChangedEventArgs(propertyName));
        InterceptCalled = true;
    }

    public static bool InterceptCalled { get; set; }
}