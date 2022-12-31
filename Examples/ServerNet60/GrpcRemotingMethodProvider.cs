using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using GrpcRemoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNet60
{
    internal class GrpcRemotingMethodProvider : IServiceMethodProvider<GrpcRemotingService>
    {
        GrpcRemotingServer pServ;

        public GrpcRemotingMethodProvider(GrpcRemotingServer server)
        {
            pServ = server;
        }

        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<GrpcRemotingService> context)
        {
            context.AddDuplexStreamingMethod(GrpcRemoting.Descriptors.RpcCallBinaryFormatter, new List<object>(), RpcCallBinaryFormatter);
        }

        Task RpcCallBinaryFormatter(GrpcRemotingService service, IAsyncStreamReader<byte[]> input, IServerStreamWriter<byte[]> output, ServerCallContext serverCallContext)
            => pServ.RpcCallBinaryFormatter(input, output, serverCallContext);

    }
}
