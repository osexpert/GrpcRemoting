using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using GrpcRemoting;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNet60
{
    internal class GrpcRemotingMethodProvider : IServiceMethodProvider<GrpcRemotingService>
    {
        RemotingServer pServ;

        public GrpcRemotingMethodProvider(RemotingServer server)
        {
            pServ = server;
        }

        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<GrpcRemotingService> context)
        {
            context.AddDuplexStreamingMethod(GrpcRemoting.Descriptors.RpcCallBinaryFormatter, new List<object>(), RpcCallBinaryFormatter);
        }

        Task RpcCallBinaryFormatter(GrpcRemotingService service, IAsyncStreamReader<byte[]> input, IServerStreamWriter<byte[]> output, ServerCallContext serverCallContext)
            => pServ.RpcCallBinaryFormatter(input, output, AddGrpcDotnetBidirStreamNotClosedHack(serverCallContext));

        static ServerCallContext AddGrpcDotnetBidirStreamNotClosedHack(ServerCallContext serverCallContext)
        {
            serverCallContext.UserState.TryAdd(RemotingServer.GrpcDotnetBidirStreamNotClosedHackKey, (Action<ServerCallContext>)Hack);
            return serverCallContext;
        }

        static void Hack(ServerCallContext serverCallContext)
        {
			var ctx = serverCallContext.GetHttpContext();
			var http2stream = ctx.Features.Get<IHttp2StreamIdFeature>();
			var meht = http2stream?.GetType().GetMethod("OnEndStreamReceived", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			meht?.Invoke(http2stream, null);
		}
    }
}
