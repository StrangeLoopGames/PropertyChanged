using System.Collections.Generic;
using System.ComponentModel;
using PropertyChanged;

public class ClassWithInlineInitializedAutoPropertiesWithoutBase : INotifyPropertyChanged
{
    [DoNotNotify]
    public IList<string> PropertyChangedCalls { get; } = new List<string>();
    public event PropertyChangedEventHandler PropertyChanged;
    
    public string Property1 { get; set; } = "Test";

    public string Property2 { get; set; } = "Test2";

    public bool IsChanged { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChangedCalls.Add(propertyName);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
