﻿using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public class TypeNode
{
    public TypeNode()
    {
        Nodes                 = new List<TypeNode>();
        PropertyDependencies  = new List<PropertyDependency>();
        Mappings              = new List<MemberMapping>();
        PropertyDatas         = new Dictionary<PropertyDefinition, PropertyData>();
    }

    public TypeDefinition                               TypeDefinition;
    public List<TypeNode>                               Nodes;
    public List<PropertyDependency>                     PropertyDependencies;
    public List<MemberMapping>                          Mappings;
    public EventInvokerMethod                           EventInvoker;
    public MethodReference                              IsChangedInvoker;
    public Dictionary<PropertyDefinition, PropertyData> PropertyDatas;
    public List<PropertyDefinition>                     AllProperties;
    public ICollection<OnChangedMethod>                 OnChangedMethods;
    public HashSet<PropertyDefinition>                  ExplicitNotifyProperties; // set of explicit notify properties, may be restricted with only notify property attributes 
    public IEnumerable<PropertyDefinition> DeclaredProperties => AllProperties.Where(prop => prop.DeclaringType == TypeDefinition);
}
