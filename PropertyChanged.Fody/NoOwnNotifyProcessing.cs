using System;
using System.Collections.Generic;
using System.Linq;

public partial class ModuleWeaver
{
    HashSet<string> OnlyNotifyWithAttributes;

    void ProcessNoOwnNotify()
    {
        var value = Config?.Attributes("OnlyNotifyWithAttributes").Select(x => x.Value).SingleOrDefault();
        if (value != null)
            OnlyNotifyWithAttributes = new HashSet<string>(value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));

        ProcessNoOwnNotify(NotifyNodes);
    }

    void ProcessNoOwnNotify(IEnumerable<TypeNode> nodes)
    {
        foreach (var typeNode in nodes)
        {
            foreach (var property in typeNode.TypeDefinition.Properties)
            {
                if (OnlyNotifyWithAttributes != null && !OnlyNotifyWithAttributes.Overlaps(property.CustomAttributes.Names()))
                    typeNode.NoOwnNotifyProperties.Add(property);
            }
            ProcessNoOwnNotify(typeNode.Nodes);
        }
    }
}