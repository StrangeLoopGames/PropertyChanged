using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    /// <summary>Injects OnPropertyChanged with interceptor. Depending on <see cref="InterceptorUsesInvoker"/> value will use either <see cref="InjectInterceptedMethodWithInvoker"/> or <see cref="InjectInterceptedMethodWithDelegate"/>.</summary>
    MethodDefinition InjectInterceptedMethod(TypeDefinition targetType) => InterceptorUsesInvoker ? InjectInterceptedMethodWithInvoker(targetType) : InjectInterceptedMethodWithDelegate(targetType);

    /// <summary>
    /// Standard Fody.PropertyChanged approach where invocation of inner property changed wrapped in Action and passed to Interceptor.
    /// Depending on interceptor type generates either <c>Intercept(this, () => InnerOnPropertyChanged(propertyName), propertyName)</c> or <c>Intercept(this, () => InnerOnPropertyChanged(propertyName), propertyName, before, after)</c>.
    /// </summary>
    MethodDefinition InjectInterceptedMethodWithDelegate(TypeDefinition targetType)
    {
        var innerOnPropertyChanged = GetMethodDefinition(targetType, out _);
        var delegateHolderInjector = new DelegateHolderInjector
                                     {
                                         TargetTypeDefinition = targetType,
                                         OnPropertyChangedMethodReference = innerOnPropertyChanged,
                                         ModuleWeaver = this,
                                     };
        delegateHolderInjector.InjectDelegateHolder();
        var method = new MethodDefinition(EventInvokerNames.First(), GetMethodAttributes(targetType), TypeSystem.VoidReference);

        var propertyName = new ParameterDefinition("propertyName", ParameterAttributes.None, TypeSystem.StringReference);
        var parameters = method.Parameters;
        parameters.Add(propertyName);
        if (InterceptorType == InvokerTypes.BeforeAfter)
        {
            var before = new ParameterDefinition("before", ParameterAttributes.None, TypeSystem.ObjectReference);
            parameters.Add(before);
            var after = new ParameterDefinition("after", ParameterAttributes.None, TypeSystem.ObjectReference);
            parameters.Add(after);
        }

        var action = new VariableDefinition(ActionTypeReference);
        method.Body.Variables.Add(action);

        var variableDefinition = new VariableDefinition(delegateHolderInjector.TypeDefinition);
        method.Body.Variables.Add(variableDefinition);


        var instructions = method.Body.Instructions;

        var last = Instruction.Create(OpCodes.Ret);
        instructions.Add(Instruction.Create(OpCodes.Newobj, delegateHolderInjector.ConstructorDefinition));
        instructions.Add(Instruction.Create(OpCodes.Stloc_1));
        instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
        instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        instructions.Add(Instruction.Create(OpCodes.Stfld, delegateHolderInjector.PropertyNameField));
        instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        instructions.Add(Instruction.Create(OpCodes.Stfld, delegateHolderInjector.TargetField));
        instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
        instructions.Add(Instruction.Create(OpCodes.Ldftn, delegateHolderInjector.MethodDefinition));
        instructions.Add(Instruction.Create(OpCodes.Newobj, ActionConstructorReference));
        instructions.Add(Instruction.Create(OpCodes.Stloc_0));
        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
        instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
        instructions.Add(Instruction.Create(OpCodes.Ldfld, delegateHolderInjector.PropertyNameField));
        if (InterceptorType == InvokerTypes.BeforeAfter)
        {
            instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_3));
            instructions.Add(Instruction.Create(OpCodes.Call, InterceptMethod));
        }
        else
        {
            instructions.Add(Instruction.Create(OpCodes.Call, InterceptMethod));
        }

        instructions.Add(last);
        method.Body.InitLocals = true;

        targetType.Methods.Add(method);
        return method;
    }

    /// <summary>
    /// Call to interceptor with INotifyPropertyChangedInvoker argument instead of a source object and callback Action.
    /// Depending on interceptor type generates either <c>Intercept((INotifyPropertyChangedInvoker)this, propertyName)</c> or <c>Intercept((INotifyPropertyChangedInvoker)this, propertyName, before, after)</c>.
    /// </summary>
    MethodDefinition InjectInterceptedMethodWithInvoker(TypeDefinition targetType)
    {
        var method = new MethodDefinition(EventInvokerNames.First(), GetMethodAttributes(targetType), TypeSystem.VoidReference);

        var propertyName = new ParameterDefinition("propertyName", ParameterAttributes.None, TypeSystem.StringReference);
        var parameters = method.Parameters;
        parameters.Add(propertyName);
        if (InterceptorType == InvokerTypes.BeforeAfter)
        {
            var before = new ParameterDefinition("before", ParameterAttributes.None, TypeSystem.ObjectReference);
            parameters.Add(before);
            var after = new ParameterDefinition("after", ParameterAttributes.None, TypeSystem.ObjectReference);
            parameters.Add(after);
        }

        var instructions = method.Body.Instructions;                              // PropertyChangedNotificationInterceptor.Intercept(
        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));                    //   this,
        instructions.Add(Instruction.Create(OpCodes.Ldarg_1));                    //   propertyName
        if (InterceptorType == InvokerTypes.BeforeAfter)
        {
            instructions.Add(Instruction.Create(OpCodes.Ldarg_2));                //   ,before
            instructions.Add(Instruction.Create(OpCodes.Ldarg_3));                //   ,after
        }
        instructions.Add(Instruction.Create(OpCodes.Call, InterceptMethod));      // )
        instructions.Add(Instruction.Create(OpCodes.Ret));

        targetType.Methods.Add(method);
        return method;
    }
}