using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public class TypeNode
{
    public TypeNode()
    {
        Nodes = new List<TypeNode>();
        PropertyDependencies = new List<PropertyDependency>();
        Mappings = new List<MemberMapping>();
        PropertyDatas = new Dictionary<PropertyDefinition, PropertyData>();
        NoOwnNotifyProperties = new HashSet<PropertyDefinition>();
    }

    public TypeDefinition TypeDefinition;
    public List<TypeNode> Nodes;
    public List<PropertyDependency> PropertyDependencies;
    public List<MemberMapping> Mappings;
    public EventInvokerMethod EventInvoker;
    public MethodReference IsChangedInvoker;
    public Dictionary<PropertyDefinition, PropertyData> PropertyDatas;
    public List<PropertyDefinition> AllProperties;
    public ICollection<OnChangedMethod> OnChangedMethods;
    public HashSet<PropertyDefinition> NoOwnNotifyProperties; // these properties won't emit PropertyChanged event, but still may notify dependent properties or invoke on change methods
    public IEnumerable<PropertyDefinition> DeclaredProperties => AllProperties.Where(prop => prop.DeclaringType == TypeDefinition);
}
