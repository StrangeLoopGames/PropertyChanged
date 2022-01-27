using System.Collections.Generic;
using System.Linq;
using Fody;
using Xunit;

public class AssemblyWithDisabledBeforeAfterForReadOnlyPropertiesTests
{
    [Fact]
    public void TestGenericOnPropertyChanged()
    {
        var weavingTask = new ModuleWeaver { DisableBeforeAfterForReadOnlyProperties = true };
        var testResult = weavingTask.ExecuteTestRun(
            "AssemblyWithDisabledBeforeAfterForReadOnlyProperties.dll",
            ignoreCodes: new[] {"0x80131869"});
        var instance = testResult.GetInstance("ClassToTestGeneric");
        instance.Trigger = "Foo";
        var notifies = instance.Notified;
        Assert.Equal(new[]
        {
            ("Trigger", false), ("Byte", true), ("SByte", true), ("Short", true), ("UShort", true), ("Int", true), ("UInt", true), ("Long", true), ("ULong", true), ("Float", true), ("Double", true), ("Guid", true), ("String", true), ("Object", true), ("RealInt", false), ("RealString", false)
        }.OrderBy(x => x.Item1), ((IEnumerable<(string, bool)>)notifies).OrderBy( x => x.Item1));
    }

    [Fact]
    public void TestNonGenericOnPropertyChanged()
    {
        var weavingTask = new ModuleWeaver { DisableBeforeAfterForReadOnlyProperties = true };
        var testResult = weavingTask.ExecuteTestRun(
            "AssemblyWithDisabledBeforeAfterForReadOnlyProperties.dll",
            ignoreCodes: new[] {"0x80131869"});
        var instance = testResult.GetInstance("ClassToTest");
        instance.Trigger = "Foo";
        var notifies = instance.Notified;
        Assert.Equal(new[]
        {
            ("Trigger", false), ("Int", true), ("Guid", true), ("String", true), ("Object", true), ("RealInt", false), ("RealString", false)
        }.OrderBy(x => x.Item1), ((IEnumerable<(string, bool)>)notifies).OrderBy( x => x.Item1));
    }
}