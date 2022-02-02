using System.Reflection;
using System.Xml.Linq;
using Fody;
using Xunit;

public class AssemblyWithInvokerInterceptorTests
{
    [Fact]
    public void Simple()
    {
        var xElement = XElement.Parse("<PropertyChanged AddPropertyChangedInvoker='true'/>");
        var weavingTask = new ModuleWeaver { Config = xElement };
        var testResult = weavingTask.ExecuteTestRun(
            "AssemblyWithInvokerInterceptor.dll",
            ignoreCodes: new[] {"0x80131869"});

        var assembly = testResult.Assembly;
        var instance = assembly.GetInstance("ClassToTest");
        EventTester.TestProperty(instance, false);
        var type = assembly.GetType("PropertyChangedNotificationInterceptor");
        var propertyInfo = type.GetProperty("InterceptCalled", BindingFlags.Static | BindingFlags.Public)!;
        var value = (bool)propertyInfo.GetValue(null, null);
        Assert.True(value);
    }

    [Fact]
    public void CustomInvokerType()
    {
        var xElement = XElement.Parse("<PropertyChanged AddPropertyChangedInvoker='AssemblyWithCustomInvokerInterceptor.ICustomNotifyPropertyChangedInvoker, AssemblyWithCustomInvokerInterceptor'/>");
        var weavingTask = new ModuleWeaver { Config = xElement };
        var testResult = weavingTask.ExecuteTestRun(
            "AssemblyWithCustomInvokerInterceptor.dll",
            ignoreCodes: new[] {"0x80131869"});

        var assembly = testResult.Assembly;
        var instance = assembly.GetInstance("ClassToTest");
        EventTester.TestProperty(instance, false);
        var type = assembly.GetType("PropertyChangedNotificationInterceptor");
        var propertyInfo = type.GetProperty("InterceptCalled", BindingFlags.Static | BindingFlags.Public)!;
        var value = (bool)propertyInfo.GetValue(null, null);
        Assert.True(value);
    }

    [Fact]
    public void BeforeAfter()
    {
        var xElement = XElement.Parse("<PropertyChanged AddPropertyChangedInvoker='true'/>");
        var weavingTask = new ModuleWeaver { Config = xElement };
        var testResult = weavingTask.ExecuteTestRun(
            "AssemblyWithInvokerBeforeAfterInterceptor.dll",
            ignoreCodes: new[] {"0x80131869"});
        var assembly = testResult.Assembly;
        var instance = assembly.GetInstance("ClassToTest");
        EventTester.TestProperty(instance, false);
        var type = assembly.GetType("PropertyChangedNotificationInterceptor");
        var propertyInfo = type.GetProperty("InterceptCalled", BindingFlags.Static | BindingFlags.Public)!;
        var value = (bool)propertyInfo.GetValue(null, null);
        Assert.True(value);
    }
}