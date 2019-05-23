using Met.Core.Proto;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Met.Core
{
    public class PluginManager
    {
        private class FunctionDefinition
        {
            public string ExtName { get; private set; }
            public string Method { get; private set; }
            public bool Blocking { get; private set; }
            public Func<Packet, Packet> Handler { get; private set; }

            public FunctionDefinition(string extName, string method, bool blocking, Func<Packet, Packet> handler)
            {
                this.ExtName = extName;
                this.Method = method;
                this.Blocking = blocking;
                this.Handler = handler;
            }
        }

        private Dictionary<string, FunctionDefinition> handlers = null;
        private Dictionary<string, List<string>> extFunctions = null;

        public PluginManager()
        {
            this.handlers = new Dictionary<string, FunctionDefinition>();
            this.extFunctions = new Dictionary<string, List<string>>();

            // Internal function registrations
            this.RegisterFunction(string.Empty, "core_enumextcmd", false, this.CoreEnumextcmd);
        }

        public void RegisterFunction(string extName, string method, bool blocking, Func<Packet, Packet> handler)
        {
            this.handlers[method] = new FunctionDefinition(extName, method, blocking, handler);
        }

        public void UnregisterFunction(string name)
        {
            this.handlers.Remove(name);
        }

        public Packet InvokeHandler(Packet request)
        {
            var fd = default(FunctionDefinition);

            if (this.handlers.TryGetValue(request.Method, out fd))
            {
                return fd.Handler(request);
            }

            return null;
        }

        private Packet CoreEnumextcmd(Packet request)
        {
            var response = request.CreateResponse();
            var extName = request.Tlvs[TlvType.String].First().ValueAsString();

            foreach (var cmd in GetCommandsForExtension(extName))
            {
                response.Add(TlvType.String, cmd);
            }

            response.Add(TlvType.Result, PacketResult.Success);
            return response;
        }

        private IEnumerable<string> GetCommandsForExtension(string extName)
        {
            return this.handlers.Values.Where(fd => fd.ExtName == extName).Select(fd => fd.Method);
        }
    }
}
