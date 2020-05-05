namespace Met.Core
{
    public interface IPlugin
    {
        string Name { get; }
        void Init(byte[] initBytes);
        void Register(PluginManager pluginManager, ChannelManager channelManager);
        void Unregister(PluginManager pluginManager, ChannelManager channelManager);
    }
}
