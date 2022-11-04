using System.Collections.Generic;

public class ClassWithInlineInitializedAutoProperties : ObservableTestObject
{
    public string Property1 { get; set; } = "Test";

    public string Property2 { get; set; } = "Test2";

    public List<string> Property3 { get; set; } = new();

    public List<string> Property4 { get; } = new();
    
    public bool IsChanged { get; set; }
}
