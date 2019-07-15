using Met.Core;
using Met.Core.Proto;

namespace Met.Stdapi
{
    public class FileSystem
    {
        public void Register(string name, PluginManager manager)
        {
            manager.RegisterFunction(name, "stdapi_fs_separator", false, this.GetFileSystemSeperator);
        }

        private InlineProcessingResult GetFileSystemSeperator(Packet request, Packet response)
        {
            response.Add(TlvType.String, System.IO.Path.DirectorySeparatorChar.ToString());
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }
    }
}
