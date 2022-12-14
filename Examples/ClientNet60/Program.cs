using ClientShared;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcRemoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClientNet60
{
    internal class Program
    {
        static void Main(string[] args)
        {


            var p = new Program();
            p.Go();

        }


        public void Go()
        {

            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            
            var c = new RemotingClient(channel.CreateCallInvoker(), new ClientConfig 
            { 
                BeforeMethodCall = BeforeBuildMethodCallMessage
            });

            var testServ = c.CreateProxy<ITestService>();

            var cs = new ClientTest();
            cs.Test(testServ);
        }

        Guid pSessID = Guid.NewGuid();

        public void BeforeBuildMethodCallMessage(Type t, MethodInfo mi)
        {
            CallContext.SetData("SessionId", pSessID);
        }

    }



}
