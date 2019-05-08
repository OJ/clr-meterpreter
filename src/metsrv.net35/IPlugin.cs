namespace Met.Core
{
    public interface IPlugin
    {
        string Name { get; }
        void Init(byte[] initBytes);
        void Register(PluginManager manager);
        void Unregister(PluginManager manager);
    }
}
