public partial class ModuleWeaver
{
    bool? cleanAttributes;

    /// <summary>Should it clean PropertyChanged attributes like [DependsOn], [AlsoNotifyFor] and other. If set to <c>true</c> then reference wouldn't be clean and you shouldn't use PrivateAssets='all' in package reference.</summary>
    public bool ShouldCleanAttributes
    {
        get => cleanAttributes ??= GetConfigBoolean("CleanAttributes") ?? true;
        set => cleanAttributes = value;
    }
}