using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    /// <summary>
    /// Processes all type nodes and fills <see cref="TypeNode.NoOwnNotifyProperties"/> with properties which shouldn't be notified using configuration rules.
    /// If `OnlyNotifyWithAttributes="attr_type_name1,attr_type_name2"` set in the config then all properties without one of listed attributes will be added to NoOwnNotifyProperties and won't emit PropertyChanged event.
    /// </summary>
    void ProcessExplicitNotify()
    {
        HashSet<string> onlyNotifyWithAttributes = null;                                                   // set of full attribute names for which PropertyChanged event should be emitted, if empty then no filtering applied
        var value = Config?.Attributes("OnlyNotifyWithAttributes").Select(x => x.Value).SingleOrDefault(); // check XML config for OnlyNotifyWithAttributes attribute in form of `attr_type_name1,attr_type_name2` (type name with namespace)
        if (value != null)
            onlyNotifyWithAttributes = new HashSet<string>(value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));

        if (onlyNotifyWithAttributes == null || onlyNotifyWithAttributes.Count == 0) return;               // skip processing if there no attributes specified
        ProcessExplicitNotify(NotifyNodes, onlyNotifyWithAttributes);                                      // go recursively through all type nodes and fill NoOwnNotifyProperties
    }

    /// <summary>Processes all type nodes and fills <see cref="TypeNode.NoOwnNotifyProperties"/> with properties which doesn't have one of attributes from <paramref name="onlyNotifyWithAttributes"/> set.</summary>
    void ProcessExplicitNotify(IEnumerable<TypeNode> nodes, HashSet<string> onlyNotifyWithAttributes)
    {
        foreach (var typeNode in nodes)
        {
            var explicitNotifyProperties = new HashSet<PropertyDefinition>();
            foreach (var property in typeNode.TypeDefinition.Properties)                                   // go through type properties
            {
                if (onlyNotifyWithAttributes.Overlaps(property.CustomAttributes.Names()))                  // check if at least one of custom attributes is in the list
                    explicitNotifyProperties.Add(property);                                                // if not so then add property to NoOwnNotifyProperties
            }

            typeNode.ExplicitNotifyProperties = explicitNotifyProperties;
            ProcessExplicitNotify(typeNode.Nodes, onlyNotifyWithAttributes);                               // call recursively for nested type nodes
        }
    }
}
