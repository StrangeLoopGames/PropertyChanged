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
        ResolveAddPropertyChangedInvokerConfig();
        FindCoreReferences();
        FindInterceptor();
        ProcessFilterTypeAttributes();
        BuildTypeNodes();
        CleanDoNotNotifyTypes();
        CleanCodeGenedTypes();
        FindMethodsForNodes();
        ProcessPropertyChangedInvoker();
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
        CleanAttributes();
    }

    public override bool ShouldCleanReference => true;
}
