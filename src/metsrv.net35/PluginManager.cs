using Met.Core.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Met.Core
{
    public enum InlineProcessingResult
    {
        Continue,
        PrevTransport,
        NextTransport,
        Shutdown
    }

    public class PluginManager
    {
        private class FunctionDefinition
        {
            public string ExtName { get; private set; }
            public string Method { get; private set; }
            public bool Blocking { get; private set; }
            public Func<Packet, Packet, InlineProcessingResult> Handler { get; private set; }

            public FunctionDefinition(string extName, string method, bool blocking, Func<Packet, Packet, InlineProcessingResult> handler)
            {
                this.ExtName = extName;
                this.Method = method;
                this.Blocking = blocking;
                this.Handler = handler;
            }
        }

        private readonly Dictionary<string, FunctionDefinition> handlers = null;
        private readonly Dictionary<string, List<string>> extFunctions = null;
        private readonly Action<Packet> packetDispatcher = null;

        public PluginManager(Action<Packet> packetDispatcher)
        {
            this.handlers = new Dictionary<string, FunctionDefinition>();
            this.extFunctions = new Dictionary<string, List<string>>();
            this.packetDispatcher = packetDispatcher;

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            // Internal function registrations
            this.RegisterFunction(string.Empty, "core_enumextcmd", false, this.CoreEnumextcmd);
            this.RegisterFunction(string.Empty, "core_loadlib", false, this.CoreLoadLib);
        }

        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.GetExecutingAssembly();
        }

        public void RegisterFunction(string extName, string method, bool blocking, Func<Packet, Packet, InlineProcessingResult> handler)
        {
            this.handlers[method] = new FunctionDefinition(extName, method, blocking, handler);
        }

        public void UnregisterFunction(string name)
        {
            this.handlers.Remove(name);
        }

        public InlineProcessingResult InvokeHandler(Packet request, Packet response)
        {
            var result = InlineProcessingResult.Continue;
            var fd = default(FunctionDefinition);

            if (this.handlers.TryGetValue(request.Method, out fd))
            {
                if (!fd.Blocking)
                {
                    var threadStart = new ThreadStart(() =>
                    {
                        fd.Handler(request, response);
                        this.packetDispatcher(response);
                    });

                    var thread = new Thread(threadStart);
                    thread.Start();
                    return InlineProcessingResult.Continue;
                }

                result = fd.Handler(request, response);
            }
            else
            {
                response.Result = PacketResult.CallNotImplemented;
            }

            this.packetDispatcher(response);
            return result;
        }

        private InlineProcessingResult CoreLoadLib(Packet request, Packet response)
        {
            var data = request.Tlvs[TlvType.Data].First().ValueAsRaw();
            var assembly = Assembly.Load(data);

            var pluginType = assembly.GetTypes().Where(t => t.IsClass && typeof(IPlugin).IsAssignableFrom(t)).FirstOrDefault();
            if (pluginType != null)
            {
                var pluginInstance = assembly.CreateInstance(pluginType.FullName) as IPlugin;
                pluginInstance.Register(this);

                foreach (var cmd in GetCommandsForExtension(pluginInstance.Name))
                {
                    response.Add(TlvType.Method, cmd);
                }

                response.Result = PacketResult.Success;
            }
            else
            {
                response.Result = PacketResult.InvalidData;
            }

            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult CoreEnumextcmd(Packet request, Packet response)
        {
            var extName = request.Tlvs[TlvType.String].First().ValueAsString();

            foreach (var cmd in GetCommandsForExtension(extName))
            {
                response.Add(TlvType.String, cmd);
            }

            response.Result = PacketResult.Success;

            return InlineProcessingResult.Continue;
        }

        private IEnumerable<string> GetCommandsForExtension(string extName)
        {
            return this.handlers.Values.Where(fd => fd.ExtName == extName).Select(fd => fd.Method);
        }
    }
}
