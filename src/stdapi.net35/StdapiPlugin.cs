using Met.Core;

namespace Met.Stdapi
{
    public class StdapiPlugin : IPlugin
    {
        private SysConfig sysConfig;
        private NetConfig netConfig;
        private FileSystem fileSystem;
        private SysProcess sysProcess;

        public string Name => "stdapi";

        public StdapiPlugin()
        {
            this.sysConfig = new SysConfig();
            this.netConfig = new NetConfig();
            this.fileSystem = new FileSystem();
            this.sysProcess = new SysProcess();
        }

        public void Init(byte[] initBytes)
        {
        }

        public void Register(PluginManager manager)
        {
            this.sysConfig.Register(this.Name, manager);
            this.netConfig.Register(this.Name, manager);
            this.fileSystem.Register(this.Name, manager);
            this.sysProcess.Register(this.Name, manager);
            //manager.RegisterFunction(this.Name, "core_channel_open", false, this.CoreChannelOpen);
        }

        public void Unregister(PluginManager manager)
        {
        }

        // TODO: move this to a channel manager of some kind
        //private InlineProcessingResult CoreChannelOpen(Packet request, Packet response)
        //{
        //    return InlineProcessingResult.Continue;
        //}
    }
}
