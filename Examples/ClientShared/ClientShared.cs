

using System.Reflection;
using System.Threading.Tasks;
using System;
using GrpcRemoting;
using System.IO;

namespace ClientShared
{
    public class ClientTest
    {
        public void Test(ITestService testServ)
        {
            Task.Run(() =>
            {
                testServ.GetMessages(m =>
                {
                    Console.WriteLine("Message from server: " + m);
                });

                Console.WriteLine("Returned from GetMessages");
            });

            testServ.TestProgress(pro =>
            {
                Console.WriteLine("Progress: " + pro);
            });




            Console.WriteLine("Enter 'CGM' to stop recieving messages");

            int i = 0;

		    while (i++ < 300)
	        {
				var res = testServ.Echo("lol42");
				Console.WriteLine("I: " + i++);
			}

            while (true)
            {
                Console.WriteLine("Write a line");
                var line = Console.ReadLine();
                var res = testServ.Echo(line);

                if (line == "CGM")
                {
                    testServ.CompleteGetMessages();
                    Console.WriteLine("Will not recieve any more messages");
                }

                Console.WriteLine($"Echo {i++} res: " + res);

                testServ.SendMessage("Send mess: " + line);
            }

            // Currently unreachable, change the while(true) -> while(false) to enable the test (and maybe use your own file:-)

            Console.WriteLine("Send file to server test");
            var now = DateTime.Now;
            using (var f = File.OpenRead(@"e:\ubuntu-20.04.2.0-desktop-amd64.iso"))
            {
                byte[] bytes = null;
                testServ.SendFile(@"e:\ubuntu-20.04.2.0-desktop-amd64 WRITTEN BY SERVER.iso", (len) =>
                {
                    if (bytes == null || bytes.Length < len)
                        bytes = new byte[len];
                    var r = f.Read(bytes, 0, len);
                    return (bytes, 0, r);

                }, p => Console.WriteLine("Progress:" + p));
            }
            Console.WriteLine("Time used: " + (DateTime.Now - now));
            now = DateTime.Now;
            Console.WriteLine("Get file from server test");
            using (var f = File.OpenWrite(@"e:\ubuntu-20.04.2.0-desktop-amd64 WRITTEN BY CLIENT.iso"))
            {
                testServ.GetFile(@"e:\ubuntu-20.04.2.0-desktop-amd64.iso", (bytes, off, len) =>
                {
                    f.Write(bytes, off, len);

                }, p => Console.WriteLine("Progress:" + p));
            }
            Console.WriteLine("Time used: " + (DateTime.Now - now));

            Console.WriteLine("done. press a key");
            Console.ReadKey();
        }

       



    }

    public interface ITestService
    {
        void SendMessage(string mess);
        string Echo(string s);
        void TestProgress(Action<string> progress);
        Task GetMessages(Action<string> message);
        void CompleteGetMessages();
        void GetFile(string file, Action<byte[], int, int> write, Action<string> progress);
        void SendFile(string file, Func<int, (byte[], int, int)> read, Action<string> progress);
    }
}
