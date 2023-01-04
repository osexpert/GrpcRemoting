using GrpcRemoting;
using System.Threading.Tasks;
using System;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using ServerShared;

namespace ServerNet48
{
    internal class Program
    {

        /// <param name="args"></param>
        /// <returns></returns>
        static void Main(string[] args)
        {
            Console.WriteLine("ServerNet48 example");

            var p = new Program();
            p.Go();
        }

        void Go()
        {
            var remServer = new RemotingServer(new ServerConfig { CreateInstance = CreateInstance });
            remServer.RegisterService<ITestService, TestService>();

            var options = new List<ChannelOption>();
            options.Add(new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue));
            options.Add(new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue));

            var server = new Grpc.Core.Server(options)
            {
                Services =
                {
                    ServerServiceDefinition.CreateBuilder()
                        .AddMethod(GrpcRemoting.Descriptors.RpcCallBinaryFormatter, remServer.RpcCallBinaryFormatter)
                        .Build()
                }
            };

            server.Ports.Add("0.0.0.0", 5000, ServerCredentials.Insecure);

            server.Start();

            Console.WriteLine("running");

            // wait for shutdown
            server.ShutdownTask.GetAwaiter().GetResult();
        }


        public object CreateInstance(Type serviceType)
        {
            Guid sessID = (Guid)CallContext.GetData("SessionId");

            Console.WriteLine("SessID: " + sessID);

            return Activator.CreateInstance(serviceType, sessID);
        }
    }

   
}