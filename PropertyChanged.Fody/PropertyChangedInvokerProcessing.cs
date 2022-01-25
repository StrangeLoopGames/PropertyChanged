using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    /// <summary>Imports <see cref="MethodReference"/> for <c>PropertyChanged.INotifyPropertyChangedInvoker.InvokePropertyChanged</c> if config option 'AddPropertyChangedInvoker' set to true in Fody.PropertyChanged config.</summary>
    MethodReference ImportInvokePropertyChangedInterfaceMethodReference()
    {
        var manualNotifyInterface = CecilUtils.MakeTypeReference("PropertyChanged.INotifyPropertyChangedInvoker", ModuleDefinition.GetAssemblyReference("PropertyChanged")); // make type reference for interface
        var invokePropertyChangedMethod = new MethodReference("InvokePropertyChanged", TypeSystem.VoidReference, manualNotifyInterface);                                     // make method reference for InvokePropertyChanged
        invokePropertyChangedMethod.Parameters.Add(new ParameterDefinition("eventArgs", ParameterAttributes.None, PropertyChangedEventArgsReference));                       // add PropertyChangedEventArgs parameter
        return ModuleDefinition.ImportReference(invokePropertyChangedMethod);                                                                                                // import for the current module
    }

    /// <summary>Processes <see cref="NotifyNodes"/> for <c>INotifyPropertyChangedInvoker</c> auto-implementation if config option 'AddPropertyChangedInvoker' set to true in Fody.PropertyChanged config..</summary>
    void ProcessPropertyChangedInvoker()
    {
        if (!AddPropertyChangedInvoker) return;
        var invokePropertyChangedMethod = ImportInvokePropertyChangedInterfaceMethodReference();
        foreach (var typeNode in NotifyNodes)                                                             // go through all base nodes implementing IPropertyChanged
        {
            var type = typeNode.TypeDefinition;
            var eventHandlerField = GetEventHandlerField(type);
            if (eventHandlerField == null) continue;                                                      // skip types with custom PropertyChanged implementation

            var interfaceMethod = invokePropertyChangedMethod;
            var interfaceType = interfaceMethod.DeclaringType;
            if (!type.HasInterface(interfaceType))                                                        // skip if the type already implements INotifyPropertyChangedInvoker
            {
                type.Interfaces.Add(new InterfaceImplementation(interfaceType));                          // add the interface to the type
                type.Methods.Add(CreateEventInvokerMethod(interfaceMethod, eventHandlerField));           // add InvokePropertyChanged method implementation to the type
            }
        }
    }

    /// <summary>Creates InvokePropertyChanged method implementation as <c>[GeneratedCode] void InvokePropertyChanged(PropertyChangedEventArgs args) => this.PropertyChanged?.Invoke(this, args);</c>.</summary>
    MethodDefinition CreateEventInvokerMethod(MethodReference interfaceMethod, FieldReference propertyChangedField)
    {
        var method = new MethodDefinition(interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot, interfaceMethod.ReturnType);
        method.Parameters.Add(new ParameterDefinition("eventArgs", ParameterAttributes.None, PropertyChangedEventArgsReference));
        MarkAsGeneratedCode(method.CustomAttributes); // add [GeneratedCode]

        var handlerVariable = new VariableDefinition(PropChangedHandlerReference);                                  // PropertyChangedEventHandler handler;
        method.Body.Variables.Add(handlerVariable);

        var instructions = method.Body.Instructions;

        var last = Instruction.Create(OpCodes.Ret);
        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        instructions.Add(Instruction.Create(OpCodes.Ldfld, propertyChangedField));                                  // handler = this.PropertyChanged;
        instructions.Add(Instruction.Create(OpCodes.Stloc_0));
        instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
        instructions.Add(Instruction.Create(OpCodes.Brfalse_S, last));                                              // if (handler == null) return;
        instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        instructions.Add(Instruction.Create(OpCodes.Tail));
        instructions.Add(Instruction.Create(OpCodes.Callvirt, PropertyChangedEventHandlerInvokeReference));         // handler.Invoke(this, args);

        instructions.Add(last);
        method.Body.InitLocals = true;
        return method;
    }

}