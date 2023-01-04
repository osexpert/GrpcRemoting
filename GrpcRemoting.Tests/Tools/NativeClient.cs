using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Channel = Grpc.Core.Channel;

namespace GrpcRemoting.Tests.Tools
{
    public class NativeClient : RemotingClient , IAsyncDisposable
    {
        Channel _channel;

        public NativeClient(int port, ClientConfig config) : base(GetInvoker(port, out var channel), config)
        {
            _channel = channel;
        }

        private static CallInvoker GetInvoker(int port, out Channel channel)
        {
            channel = new Channel("localhost", port, ChannelCredentials.Insecure);
            return channel.CreateCallInvoker();
        }

        public ValueTask DisposeAsync()
        {
            if (_channel != null)
                return new ValueTask(_channel.ShutdownAsync());
            else
                return ValueTask.CompletedTask;
        }
    }
    
}
