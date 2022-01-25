using System.Linq;
using System.Xml;

public partial class ModuleWeaver
{
    public bool AddPropertyChangedInvoker;

    public void ResolveAddPropertyChangedInvokerConfig()
    {
        var value = Config?.Attributes("AddPropertyChangedInvoker").Select(a => a.Value).SingleOrDefault();
        if (value != null)
            AddPropertyChangedInvoker = XmlConvert.ToBoolean(value.ToLowerInvariant());
    }
}