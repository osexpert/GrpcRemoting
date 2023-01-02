using ClientShared;
using Grpc.Core;
using GrpcRemoting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace ClientNet48
{
    internal class Program : IGrpcRemotingClientHandler
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.Go();
        }


        public void Go()
        {
            var channel = new Channel("localhost", 5000, ChannelCredentials.Insecure);
            var c = new RemotingClient(channel.CreateCallInvoker(), this);
            var testServ = c.CreateServiceProxy<ITestService>();

            var cs = new ClientTest();
            cs.Test(testServ);
        }

        Guid pSessID = Guid.NewGuid();

        public void BeforeBuildMethodCallMessage(MethodInfo mi)
        {
            CallContext.SetData("SessionId", pSessID);
        }
    }

 

}
