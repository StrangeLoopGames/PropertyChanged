﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public static class CecilExtensions
{
    static readonly Regex BackingFieldRegex = new("^<(\\w+)>k__BackingField$", RegexOptions.Compiled);

    public static string GetName(this PropertyDefinition propertyDefinition)
    {
        return $"{propertyDefinition.DeclaringType.FullName}.{propertyDefinition.Name}";
    }

    public static bool IsCallToBaseMethod(this Instruction instruction, MethodDefinition method)
    {
        return instruction.OpCode == OpCodes.Call && instruction.IsCallToMethod(method);
    }

    public static bool IsCallToMethod(this Instruction instruction, MethodDefinition method)
    {
        if (!instruction.OpCode.IsCall())
        {
            return false;
        }

        if (!(instruction.Operand is MethodReference methodReference))
        {
            return false;
        }

        if (methodReference.Name != method.Name)
        {
            return false;
        }

        if (methodReference.Resolve() != method)
        {
            return false;
        }

        return true;
    }

    public static bool IsCallToMethod(this Instruction instruction, string methodName, out int propertyNameIndex)
    {
        propertyNameIndex = 1;
        if (!instruction.OpCode.IsCall())
        {
            return false;
        }

        if (!(instruction.Operand is MethodReference methodReference))
        {
            return false;
        }

        if (methodReference.Name != methodName)
        {
            return false;
        }

        var parameterDefinition = methodReference.Parameters.FirstOrDefault(x => x.Name == "propertyName");
        if (parameterDefinition != null)
        {
            propertyNameIndex = methodReference.Parameters.Count - parameterDefinition.Index;
        }

        return true;
    }

    public static bool IsCall(this OpCode opCode)
    {
        return opCode.Code == Code.Call ||
               opCode.Code == Code.Callvirt;
    }

    public static FieldReference GetGeneric(this FieldDefinition definition)
    {
        if (!definition.DeclaringType.HasGenericParameters)
        {
            return definition;
        }
        var declaringType = new GenericInstanceType(definition.DeclaringType);
        foreach (var parameter in definition.DeclaringType.GenericParameters)
        {
            declaringType.GenericArguments.Add(parameter);
        }

        return new FieldReference(definition.Name, definition.FieldType, declaringType);
    }

    public static MethodReference GetGeneric(this MethodReference reference)
    {
        if (!reference.DeclaringType.HasGenericParameters)
        {
            return reference;
        }
        var declaringType = new GenericInstanceType(reference.DeclaringType);
        foreach (var parameter in reference.DeclaringType.GenericParameters)
        {
            declaringType.GenericArguments.Add(parameter);
        }

        var methodReference = new MethodReference(reference.Name, reference.MethodReturnType.ReturnType, declaringType);
        foreach (var parameterDefinition in reference.Parameters)
        {
            methodReference.Parameters.Add(parameterDefinition);
        }

        methodReference.HasThis = reference.HasThis;
        return methodReference;
    }

    public static MethodReference MakeGeneric(this MethodReference self, params TypeReference[] arguments)
    {
        var reference = new MethodReference(self.Name, self.ReturnType)
        {
            HasThis = self.HasThis,
            ExplicitThis = self.ExplicitThis,
            DeclaringType = self.DeclaringType.MakeGenericInstanceType(arguments),
            CallingConvention = self.CallingConvention,
        };

        foreach (var parameter in self.Parameters)
        {
            reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
        }

        foreach (var genericParameter in self.GenericParameters)
        {
            reference.GenericParameters.Add(new GenericParameter(genericParameter.Name, reference));
        }

        return reference;
    }

    public static IEnumerable<CustomAttribute> GetAllCustomAttributes(this TypeDefinition typeDefinition)
    {
        foreach (var attribute in typeDefinition.CustomAttributes)
        {
            yield return attribute;
        }

        if (!(typeDefinition.BaseType is TypeDefinition baseDefinition))
        {
            yield break;
        }

        foreach (var attribute in baseDefinition.GetAllCustomAttributes())
        {
            yield return attribute;
        }
    }

    public static IEnumerable<CustomAttribute> GetAttributes(this IEnumerable<CustomAttribute> attributes, string attributeName)
    {
        return attributes.Where(attribute => attribute.Constructor.DeclaringType.FullName == attributeName);
    }

    public static CustomAttribute GetAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
    {
        return attributes.FirstOrDefault(attribute => attribute.Constructor.DeclaringType.FullName == attributeName);
    }

    public static bool ContainsAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
    {
        return attributes.Any(attribute => attribute.Constructor.DeclaringType.FullName == attributeName);
    }

    /// <summary>Returns full type names for custom <paramref name="attributes"/>.</summary>
    public static IEnumerable<string> Names(this IEnumerable<CustomAttribute> attributes) => attributes.Select(a => a.Constructor.DeclaringType.FullName);

    /// <summary>Returns full names for <paramref name="members"/>.</summary>
    public static IEnumerable<string> FullNames(this IEnumerable<MemberReference> members) => members.Select(a => a.FullName);

    /// <summary>Checks if <paramref name="type"/> has the <paramref name="interface"/>.</summary>
    public static bool HasInterface(this TypeDefinition type, TypeReference @interface) => type.GetAllInterfaces().FullNames().Contains(@interface.FullName);

    /// <summary>Returns <see cref="AssemblyNameReference"/> by <paramref name="assemblyName"/> like "PropertyChanged".</summary>
    public static AssemblyNameReference GetAssemblyReference(this ModuleDefinition moduleDefinition, string assemblyName) => moduleDefinition.AssemblyReferences.SingleOrDefault(x => x.Name == assemblyName);

    public static IEnumerable<TypeReference> GetAllInterfaces(this TypeDefinition type)
    {
        while (type != null)
        {
            if (type.HasInterfaces)
            {
                foreach (var iface in type.Interfaces)
                {
                    yield return iface.InterfaceType;
                }
            }

            type = type.BaseType?.Resolve();
        }
    }

    public static bool GetBaseMethod(this MethodDefinition method, out MethodDefinition baseMethod)
    {
        baseMethod = method.GetBaseMethod();
        return baseMethod != null
            && baseMethod != method; // cecil's GetBaseMethod() returns self if the method has no base method...
    }

    public static IEnumerable<MethodDefinition> GetSelfAndBaseMethods(this MethodDefinition method)
    {
        yield return method;

        while (method.GetBaseMethod(out method))
        {
            yield return method;
        }
    }

    /// <summary>Tries to get property name for <paramref name="field"/> if it is a backing field.</summary>
    public static bool TryGetBackingFieldPropertyName(this FieldDefinition field, out string propertyName)
    {
        var match = BackingFieldRegex.Match(field.Name);
        if (!match.Success)
        {
            propertyName = null;
            return false;
        }
        
        propertyName = match.Groups[1].Value;
        return true;
    }

    /// <summary>Checks if <paramref name="field"/> is a backing field.</summary>
    public static bool IsBackingField(this FieldReference field) => BackingFieldRegex.IsMatch(field.Name);
}
