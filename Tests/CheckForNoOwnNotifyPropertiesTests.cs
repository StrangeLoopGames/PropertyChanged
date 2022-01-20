namespace Tests;

using System.Collections.Generic;
using System.Xml.Linq;
using Fody;
using Xunit;

public class NoOwnNotifyProcessingTests
{
    [Theory]
    [InlineData("System.ComponentModel.DisplayNameAttribute", new[] { "event:DisplayName", "event:DisplayName", "method:NoOwnNotify" })]
    [InlineData("System.ComponentModel.DescriptionAttribute", new[] { "event:Description", "method:NoOwnNotify" })]
    [InlineData("System.ComponentModel.DisplayNameAttribute,System.ComponentModel.DescriptionAttribute", new[] { "event:DisplayName", "event:Description", "event:DisplayName", "method:NoOwnNotify" })]
    public void OnlyNotifiesPropertiesWithDisplayNameAttribute(string onlyNotifyWithAttributes, string[] expectedNotifications)
    {
        var xElement = XElement.Parse($"<PropertyChanged OnlyNotifyWithAttributes='{onlyNotifyWithAttributes}'/>");
        var moduleWeaver = new ModuleWeaver { Config = xElement };
        var testResult = moduleWeaver.ExecuteTestRun("AssemblyWithPropertyAttributes.dll");
        var instance = testResult.GetInstance("ClassWithAttributedProperties");
        instance.DisplayName = "My Name";
        instance.Description = "My Description";
        instance.NoOwnNotify = "Value";
        var notifications = (List<string>)instance.Notifications;
        Assert.Equal(expectedNotifications, notifications);
    }
}