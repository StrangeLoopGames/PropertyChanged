using Fody;

public partial class ModuleWeaver: BaseModuleWeaver
{
    public override void Execute()
    {
        ResolveOnPropertyNameChangedConfig();
        ResolveEnableIsChangedPropertyConfig();
        ResolveTriggerDependentPropertiesConfig();
        ResolveCheckForEqualityConfig();
        ResolveCheckForEqualityUsingBaseEqualsConfig();
        ResolveUseStaticEqualsFromBaseConfig();
        ResolveSuppressWarningsConfig();
        ResolveSuppressOnPropertyNameChangedWarningConfig();
        ResolveEventInvokerName();
        ResolveDisableBeforeAfterForReadOnlyPropertiesConfig();
        FindCoreReferences();
        FindInterceptor();
        ProcessFilterTypeAttributes();
        BuildTypeNodes();
        CleanDoNotNotifyTypes();
        CleanCodeGenedTypes();
        ProcessPropertyChangedInvoker();
        FindMethodsForNodes();
        FindIsChangedMethod();
        FindAllProperties();
        FindMappings();
        DetectIlGeneratedByDependency();
        ProcessDependsOnAttributes();
        WalkPropertyData();
        ProcessNoOwnNotify();
        CheckForWarnings();
        ProcessOnChangedMethods();
        CheckForStackOverflow();
        FindComparisonMethods();
        InitEventArgsCache();
        ProcessTypes();
        InjectEventArgsCache();
        if (ShouldCleanAttributes)
            CleanAttributes();
    }

    public override bool ShouldCleanReference => ShouldCleanAttributes && PropertyChangedInvokerType != PropertyChangedInvokerType.Default;
}
