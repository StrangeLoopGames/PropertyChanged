public partial class ModuleWeaver
{
    bool? addPropertyChangedInvoker;

    /// <summary>Should it inject INotifyPropertyChangedInvoker interface with <see cref="ProcessPropertyChangedInvoker"/>. If set to <c>true</c> then reference wouldn't be clean and you shouldn't use PrivateAssets='all' in package reference.</summary>
    public bool AddPropertyChangedInvoker
    {
        get => addPropertyChangedInvoker ??= GetConfigBoolean("AddPropertyChangedInvoker") ?? false;
        set => addPropertyChangedInvoker = value;
    }
}