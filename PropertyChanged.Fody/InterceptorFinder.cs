using System.Linq;
using Fody;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public bool FoundInterceptor;
    public MethodDefinition InterceptMethod;
    public InvokerTypes InterceptorType;        // type of interceptor String or BeforeAfter
    public bool InterceptorUsesInvoker;         // true - uses INotifyPropertyChangedInvoker for callback, false - uses Action for callback

    void SearchForMethod(TypeDefinition typeDefinition)
    {
        var methodDefinition = typeDefinition.Methods.FirstOrDefault(x => x.Name == "Intercept");
        if (methodDefinition == null)
        {
            throw new WeavingException($"Found Type '{typeDefinition.FullName}' but could not find a method named 'Intercept'.");
        }
        if (!methodDefinition.IsStatic)
        {
            throw new WeavingException($"Found Type '{typeDefinition.FullName}.Intercept' but it is not static.");
        }
        if (!methodDefinition.IsPublic)
        {
            throw new WeavingException($"Found Type '{typeDefinition.FullName}.Intercept' but it is not public.");
        }

        if (TryGetInterceptorType(methodDefinition, out var interceptorType, out var usesInvoker))
        {
            FoundInterceptor = true;
            InterceptMethod = methodDefinition;
            InterceptorType = interceptorType;
            InterceptorUsesInvoker = usesInvoker;
            return;
        }

        var message = $@"Found '{typeDefinition.FullName}.Intercept' But the signature is not correct. It needs to be either.
Intercept(object target, Action firePropertyChanged, string propertyName)
or
Intercept(object target, Action firePropertyChanged, string propertyName, object before, object after)";
        throw new WeavingException(message);
    }

    /// <summary>Tries to determine interceptor type by <paramref name="methodDefinition"/> signature. Also returns <c>true</c> in <paramref name="usesInvoker"/> if interceptor method should use <c>INotifyPropertyChangedInvoker</c> instead of Action delegate.</summary>
    bool TryGetInterceptorType(MethodDefinition methodDefinition, out InvokerTypes interceptorType, out bool usesInvoker)
    {
        usesInvoker = false;
        if (IsSingleStringInterceptionMethod(methodDefinition)) // Intercept(object, Action, string) ?
        {
            interceptorType = InvokerTypes.String;
            return true;
        }

        if (IsBeforeAfterInterceptionMethod(methodDefinition))  // Intercept(object, Action, string, object, object) ?
        {
            interceptorType = InvokerTypes.BeforeAfter;
            return true;
        }

        if (AddPropertyChangedInvoker)
        {
            if (IsInvokerSingleStringInterceptionMethod(methodDefinition)) // Intercept(INotifyPropertyChangedInvoker, string) ?
            {
                usesInvoker = true;
                interceptorType = InvokerTypes.String;
                return true;
            }

            if (IsInvokerBeforeAfterInterceptionMethod(methodDefinition))  // Intercept(INotifyPropertyChangedInvoker, object, object) ?
            {
                usesInvoker = true;
                interceptorType = InvokerTypes.BeforeAfter;
                return true;
            }
        }

        interceptorType = default;
        return false;
    }

    /// <summary>Check if method has signature <c>Intercept(object, Action, string)</c>.</summary>
    public bool IsSingleStringInterceptionMethod(MethodDefinition method)
    {
        var parameters = method.Parameters;
        return parameters.Count == 3
               && parameters[0].ParameterType.FullName == "System.Object"
               && parameters[1].ParameterType.FullName == "System.Action"
               && parameters[2].ParameterType.FullName == "System.String";
    }

    /// <summary>Check if method has signature <c>Intercept(object, Action, string, object, object)</c>.</summary>
    public bool IsBeforeAfterInterceptionMethod(MethodDefinition method)
    {
        var parameters = method.Parameters;
        return parameters.Count == 5
               && parameters[0].ParameterType.FullName == "System.Object"
               && parameters[1].ParameterType.FullName == "System.Action"
               && parameters[2].ParameterType.FullName == "System.String"
               && parameters[3].ParameterType.FullName == "System.Object"
               && parameters[4].ParameterType.FullName == "System.Object";
    }

    /// <summary>Check if method has signature <c>Intercept(INotifyPropertyChangedInvoker, string)</c>.</summary>
    public bool IsInvokerSingleStringInterceptionMethod(MethodDefinition method)
    {
        var parameters = method.Parameters;
        return parameters.Count == 2
               && parameters[0].ParameterType.FullName == "PropertyChanged.INotifyPropertyChangedInvoker"
               && parameters[1].ParameterType.FullName == "System.String";
    }

    /// <summary>Check if method has signature <c>Intercept(INotifyPropertyChangedInvoker, string, object, object)</c>.</summary>
    public bool IsInvokerBeforeAfterInterceptionMethod(MethodDefinition method)
    {
        var parameters = method.Parameters;
        return parameters.Count == 4
               && parameters[0].ParameterType.FullName == "PropertyChanged.INotifyPropertyChangedInvoker"
               && parameters[1].ParameterType.FullName == "System.String"
               && parameters[2].ParameterType.FullName == "System.Object"
               && parameters[3].ParameterType.FullName == "System.Object";
    }

    public void FindInterceptor()
    {
        var typeDefinition = ModuleDefinition.Types.FirstOrDefault(x => x.Name == "PropertyChangedNotificationInterceptor");
        if (typeDefinition == null)
        {
            FoundInterceptor = false;
            return;
        }
        SearchForMethod(typeDefinition);
    }
}