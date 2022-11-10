using System.Collections.Generic;
using System.Xml.Linq;
using Fody;
using Xunit;

public class ExplicitNotifyProcessingTests
{
    [Theory]
    [InlineData("ClassWithAttributedProperties",                         "System.ComponentModel.DisplayNameAttribute",                                            new[] { "event:DisplayNameAuto", "event:DisplayNameAutoReadOnly", "event:DisplayName", "event:DisplayName", "method:NoOwnNotify" })]
    [InlineData("ClassWithAttributedProperties",                         "System.ComponentModel.DescriptionAttribute",                                            new[] { "event:Description", "method:NoOwnNotify" })]
    [InlineData("ClassWithAttributedProperties",                         "System.ComponentModel.DisplayNameAttribute,System.ComponentModel.DescriptionAttribute", new[] { "event:DisplayNameAuto", "event:DisplayNameAutoReadOnly", "event:DisplayName", "event:Description", "event:DisplayName", "method:NoOwnNotify" })]
    [InlineData("ClassSenderPropertyChangedArgWithAttributedProperties", "System.ComponentModel.DisplayNameAttribute",                                            new[] { "event:DisplayNameAuto", "event:DisplayNameAutoReadOnly", "event:DisplayName", "event:DisplayName", "method:NoOwnNotify" })]
    [InlineData("ClassSenderPropertyChangedArgWithAttributedProperties", "System.ComponentModel.DescriptionAttribute",                                            new[] { "event:Description", "method:NoOwnNotify" })]
    [InlineData("ClassSenderPropertyChangedArgWithAttributedProperties", "System.ComponentModel.DisplayNameAttribute,System.ComponentModel.DescriptionAttribute", new[] { "event:DisplayNameAuto", "event:DisplayNameAutoReadOnly", "event:DisplayName", "event:Description", "event:DisplayName", "method:NoOwnNotify" })]
    [InlineData("ClassPropertyChangedArgWithAttributedProperties",       "System.ComponentModel.DisplayNameAttribute",                                            new[] { "event:DisplayNameAuto", "event:DisplayNameAutoReadOnly", "event:DisplayName", "event:DisplayName", "method:NoOwnNotify" })]
    [InlineData("ClassPropertyChangedArgWithAttributedProperties",       "System.ComponentModel.DescriptionAttribute",                                            new[] { "event:Description", "method:NoOwnNotify" })]
    [InlineData("ClassPropertyChangedArgWithAttributedProperties",       "System.ComponentModel.DisplayNameAttribute,System.ComponentModel.DescriptionAttribute", new[] { "event:DisplayNameAuto", "event:DisplayNameAutoReadOnly", "event:DisplayName", "event:Description", "event:DisplayName", "method:NoOwnNotify" })]
    [InlineData("ClassBeforeAfterWithAttributedProperties",              "System.ComponentModel.DisplayNameAttribute",                                            new[] { "event:DisplayNameAuto::Auto", "event:DisplayNameAutoReadOnly::42", "event:DisplayName::My Name", "event:DisplayName:My Name:My Name", "method:NoOwnNotify" })]
    [InlineData("ClassBeforeAfterWithAttributedProperties",              "System.ComponentModel.DescriptionAttribute",                                            new[] { "event:Description::My Description", "method:NoOwnNotify" })]
    [InlineData("ClassBeforeAfterWithAttributedProperties",              "System.ComponentModel.DisplayNameAttribute,System.ComponentModel.DescriptionAttribute", new[] { "event:DisplayNameAuto::Auto", "event:DisplayNameAutoReadOnly::42", "event:DisplayName::My Name", "event:Description::My Description", "event:DisplayName:My Name:My Name", "method:NoOwnNotify" })]
    [InlineData("ClassBeforeAfterGenericWithAttributedProperties",       "System.ComponentModel.DisplayNameAttribute",                                            new[] { "event:DisplayNameAuto::Auto", "event:DisplayNameAutoReadOnly:0:42", "event:DisplayName::My Name", "event:DisplayName:My Name:My Name", "method:NoOwnNotify" })]
    [InlineData("ClassBeforeAfterGenericWithAttributedProperties",       "System.ComponentModel.DescriptionAttribute",                                            new[] { "event:Description::My Description", "method:NoOwnNotify" })]
    [InlineData("ClassBeforeAfterGenericWithAttributedProperties",       "System.ComponentModel.DisplayNameAttribute,System.ComponentModel.DescriptionAttribute", new[] { "event:DisplayNameAuto::Auto", "event:DisplayNameAutoReadOnly:0:42", "event:DisplayName::My Name", "event:Description::My Description", "event:DisplayName:My Name:My Name", "method:NoOwnNotify" })]
    public void OnlyNotifiesPropertiesWithAttributes(string typeName, string onlyNotifyWithAttributes, string[] expectedNotifications)
    {
        var xElement = XElement.Parse($"<PropertyChanged OnlyNotifyWithAttributes='{onlyNotifyWithAttributes}'/>");
        var moduleWeaver = new ModuleWeaver { Config = xElement };
        var testResult = moduleWeaver.ExecuteTestRun("AssemblyWithPropertyAttributes.dll");
        var instance = testResult.GetInstance(typeName);
        instance.DisplayName = "My Name";
        instance.Description = "My Description";
        instance.NoOwnNotify = "Value";
        var notifications = (List<string>)instance.Notifications;
        Assert.Equal(expectedNotifications, notifications);
    }
}
