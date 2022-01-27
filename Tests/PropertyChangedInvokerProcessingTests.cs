using System.ComponentModel;
using Fody;
using PropertyChanged;
using Xunit;

[Collection("AssemblyToProcess")]
public class PropertyChangedInvokerProcessingTests
{
    [Fact]
    public void ShouldImplementINotifyPropertyChangedInvokerInterfaceWhenConfigEnabled()
    {
        var weaver = new ModuleWeaver { AddPropertyChangedInvoker = true };
        var testResult = weaver.ExecuteTestRun("AssemblyToProcess.dll");
        var instance = testResult.GetInstance("ClassParentWithProperty");
        Assert.True(instance is INotifyPropertyChangedInvoker);
        Assert.False(testResult.GetInstance("ReactiveUI.ReactiveObject") is INotifyPropertyChangedInvoker, "Shouldn't auto-implement for classes with custom PropertyChanged implementation.");

        string notifiedPropertyName = null;
        object notifiedObject = null;

        void Handler(object source, PropertyChangedEventArgs args)
        {
            notifiedObject = source;
            notifiedPropertyName = args.PropertyName;
        }

        ((INotifyPropertyChanged)instance).PropertyChanged += Handler;
        instance.InvokePropertyChanged(new PropertyChangedEventArgs("Foo"));
        Assert.Equal((object)instance, notifiedObject);
        Assert.Equal("Foo", notifiedPropertyName);
    }

    [Fact]
    public void ShouldNotImplementINotifyPropertyChangedInvokerInterfaceByDefault()
    {
        var weaver = new ModuleWeaver();
        var testResult = weaver.ExecuteTestRun("AssemblyToProcess.dll");
        var instance = testResult.GetInstance("ClassParentWithProperty");
        Assert.False(instance is INotifyPropertyChangedInvoker);
    }
}