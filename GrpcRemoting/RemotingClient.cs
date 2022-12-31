using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcRemoting.RpcMessaging;
using GrpcRemoting.Serialization;
using GrpcRemoting.Serialization.Binary;

namespace GrpcRemoting
{
	public interface IGrpcRemotingClientHandler
	{
		// set CallContext
		void BeforeBuildMethodCallMessage(MethodInfo mi);

        // TODO: choose formatter per method??
    }

    public class GrpcRemotingClient
	{
        public IGrpcRemotingClientHandler pHand;
        CallInvoker pInvoker;

        public GrpcRemotingClient(CallInvoker invoker, IGrpcRemotingClientHandler hand)
		{
			pHand = hand;
            pInvoker = invoker;
        }

        private static readonly Castle.DynamicProxy.ProxyGenerator ProxyGenerator = new Castle.DynamicProxy.ProxyGenerator();

        public T CreateServiceProxy<T>()
        {
            var serviceProxyType = typeof(GrpcRemotingClientProxy<>).MakeGenericType(typeof(T));
            var serviceProxy = Activator.CreateInstance(serviceProxyType, this /* GrpcClient */);

            var proxy = ProxyGenerator.CreateInterfaceProxyWithoutTarget(
                interfaceToProxy: typeof(T),
                interceptor: (Castle.DynamicProxy.IInterceptor)serviceProxy);

            return (T)proxy;
        }

        internal void CallbackToSetCallContext(MethodInfo mi) => pHand.BeforeBuildMethodCallMessage(mi);

		public MethodCallMessageBuilder pMessBuild = new();
		public ISerializerAdapter pSerializer = new BinarySerializerAdapter();

        internal async Task InvokeAsync(byte[] req, Func<byte[], Func<byte[], Task>, Task> reponse)
        {
            using (var call = pInvoker.AsyncDuplexStreamingCall(GrpcRemoting.Descriptors.RpcCallBinaryFormatter, null, new CallOptions { }))
            {
                await call.RequestStream.WriteAsync(req).ConfigureAwait(false);
                var responseCompleted = call.ResponseStream.ForEachAsync(b => reponse(b, d => call.RequestStream.WriteAsync(d)));
                await responseCompleted.ConfigureAwait(false);
            }
        }

        internal void Invoke(byte[] req, Func<byte[], Func<byte[], Task>, Task> reponse)
        {
            using (var call = pInvoker.AsyncDuplexStreamingCall(GrpcRemoting.Descriptors.RpcCallBinaryFormatter, null, new CallOptions { }))
            {
                call.RequestStream.WriteAsync(req).GetAwaiter().GetResult();
                var responseCompleted = call.ResponseStream.ForEachAsync(b => reponse(b, d => call.RequestStream.WriteAsync(d)));
                responseCompleted.GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Extension methods that simplify work with gRPC streaming calls.
    /// https://chromium.googlesource.com/external/github.com/grpc/grpc/+/chromium-deps/2016-07-19/src/csharp/Grpc.Core/Utils/AsyncStreamExtensions.cs
    /// </summary>
    static class AsyncStreamExtensions2
    {
        /// <summary>
        /// Reads the entire stream and executes an async action for each element.
        /// </summary>
        public static async Task ForEachAsync<T>(this IAsyncStreamReader<T> streamReader, Func<T, Task> asyncAction)
            where T : class
        {
            while (await streamReader.MoveNext().ConfigureAwait(false))
            {
                await asyncAction(streamReader.Current).ConfigureAwait(false);
            }
        }
    }
}
