using Grpc.Core;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcRemoting.Tests.Tools
{
    public class NativeServer : RemotingServer, IAsyncDisposable
    {

        Grpc.Core.Server _server;

        public NativeServer(int port, ServerConfig config) : base(config)
        {
            var options = new List<ChannelOption>();
            options.Add(new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue));
            options.Add(new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue));

            _server = new Grpc.Core.Server(options)
            {
                Services =
                {
                    ServerServiceDefinition.CreateBuilder()
                        .AddMethod(GrpcRemoting.Descriptors.RpcCallBinaryFormatter, this.RpcCallBinaryFormatter)
                        .Build()
                }
            };

            _server.Ports.Add("0.0.0.0", port, ServerCredentials.Insecure);
        }

        public ValueTask DisposeAsync()
        {
            if (_server != null)
                return new ValueTask(_server.ShutdownAsync());
            else
                return ValueTask.CompletedTask;
        }

        public void Start() => _server.Start();
    }
}
