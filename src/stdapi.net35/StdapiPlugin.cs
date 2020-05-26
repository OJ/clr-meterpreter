using Met.Core;

namespace Met.Stdapi
{
    public class StdapiPlugin : IPlugin
    {
        private SysConfig sysConfig = null;
        private NetConfig netConfig = null;
        private FileSystem fileSystem = null;
        private SysProcess sysProcess = null;
        private SysPower sysPower = null;

        public string Name => "stdapi";

        public StdapiPlugin()
        {
            this.sysConfig = new SysConfig();
            this.netConfig = new NetConfig();
            this.fileSystem = new FileSystem();
            this.sysProcess = new SysProcess();
            this.sysPower = new SysPower();
        }

        public void Init(byte[] initBytes)
        {
        }

        public void Register(PluginManager pluginManager, ChannelManager channelManager)
        {
            this.sysConfig.Register(this.Name, pluginManager);
            this.netConfig.Register(this.Name, pluginManager);
            this.fileSystem.Register(this.Name, pluginManager);
            this.sysProcess.Register(this.Name, pluginManager, channelManager);
            this.sysPower.Register(this.Name, pluginManager);

            channelManager.RegisterChannelCreator("stdapi_net_tcp_client", Channel.TcpClientChannel.Create);
            channelManager.RegisterChannelCreator("stdapi_net_tcp_server", Channel.TcpServerChannel.Create);
        }

        public void Unregister(PluginManager pluginManager, ChannelManager channelManager)
        {
        }
    }
}
