using System;
using System.Collections.Generic;
using System.IO;
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

    public class RemotingClient
	{
        ClientConfig _config;
        CallInvoker _callInvoker;

        public RemotingClient(CallInvoker callInvoker, ClientConfig config)
		{
			_config = config;
            _callInvoker = callInvoker;
        }

        private static readonly Castle.DynamicProxy.ProxyGenerator ProxyGenerator = new Castle.DynamicProxy.ProxyGenerator();

        public T CreateProxy<T>()
        {
            var serviceProxyType = typeof(ServiceProxy<>).MakeGenericType(typeof(T));
            var serviceProxy = Activator.CreateInstance(serviceProxyType, this /* RemotingClient */);

            var proxy = ProxyGenerator.CreateInterfaceProxyWithoutTarget(
                interfaceToProxy: typeof(T),
                interceptor: (Castle.DynamicProxy.IInterceptor)serviceProxy);

            return (T)proxy;
        }

        internal void BeforeMethodCall(Type serviceType, MethodInfo mi) => _config.BeforeMethodCall?.Invoke(serviceType, mi);

		public MethodCallMessageBuilder MethodCallMessageBuilder = new();
		public ISerializerAdapter Serializer = new BinarySerializerAdapter();

        internal async Task InvokeAsync(byte[] req, Func<byte[], Func<byte[], Task>, Task> reponse)
        {
            using (var call = _callInvoker.AsyncDuplexStreamingCall(GrpcRemoting.Descriptors.RpcCallBinaryFormatter, null, new CallOptions { }))
            {
                await call.RequestStream.WriteAsync(req).ConfigureAwait(false);

				while (await call.ResponseStream.MoveNext().ConfigureAwait(false))
				{
                    if (_config.EnableGrpcDotnetServerBidirStreamNotClosedHacks)
                    {
                        // Client hung up
                        if (call.ResponseStream.Current.Length == 1)
                        {
                            if (call.ResponseStream.Current[0] == ClientHangupByte)
                                break;
                            else
                                throw new Exception($"1 byte but not ClientHangupByte");
                        }
                    }

					await reponse(call.ResponseStream.Current, bytes => call.RequestStream.WriteAsync(bytes)).ConfigureAwait(false);
				}

                await call.RequestStream.CompleteAsync().ConfigureAwait(false);
			}
        }

		internal const byte ClientHangupByte = 0x42;

		internal void Invoke(byte[] req, Func<byte[], Func<byte[], Task>, Task> reponse)
        {
			using (var call = _callInvoker.AsyncDuplexStreamingCall(GrpcRemoting.Descriptors.RpcCallBinaryFormatter, null, new CallOptions { }))
            {
                call.RequestStream.WriteAsync(req).GetAwaiter().GetResult();

				while (call.ResponseStream.MoveNext().GetAwaiter().GetResult())
                {
                    if (_config.EnableGrpcDotnetServerBidirStreamNotClosedHacks)
                    {
                        // Client hung up
                        // Hack for grpd-dotnet bug(?): https://github.com/grpc/grpc-dotnet/issues/2010
                        // Initially checked for Length = 0 bytes (server sent 0 bytes as the signal), but I think it is safer to have an explicit hangup byte.
                        // This assumes that we never send 1 bytes with data, except for the hangup sequence.
                        // PS: only works with the dotnet client...
                        if (call.ResponseStream.Current.Length == 1)
                        {
                            if (call.ResponseStream.Current[0] == ClientHangupByte)
                                break;
                            else
                                throw new Exception($"1 byte but not ClientHangupByte");
                        }
                    }

                    reponse(call.ResponseStream.Current, bytes => call.RequestStream.WriteAsync(bytes)).GetAwaiter().GetResult();
				}

                call.RequestStream.CompleteAsync().GetAwaiter().GetResult();
			}
		}
    }

#if false
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
#endif
}
