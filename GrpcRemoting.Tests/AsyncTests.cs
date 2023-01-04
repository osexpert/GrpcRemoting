using GrpcRemoting.Tests.Tools;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GrpcRemoting.Tests
{
    public class AsyncTests
    {
        #region Service with async method

        public interface IAsyncService
        {
            Task<string> ConvertToBase64Async(string text);

            Task NonGenericTask();
        }

        public class AsyncService : IAsyncService
        {
            public async Task<string> ConvertToBase64Async(string text)
            {
                var convertFunc = new Func<string>(() =>
                {
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
                    return Convert.ToBase64String(stream.ToArray());
                });

                var base64String = await Task.Run(convertFunc);

                return base64String;
            }

            public Task NonGenericTask()
            {
                return Task.CompletedTask;
            }
        }

        #endregion

        [Fact]
        public async void AsyncMethods_should_work()
        {
            var serverConfig =
                new ServerConfig()
                {
                    //RegisterServicesAction = container =>
                    //    container.RegisterService<IAsyncService, AsyncService>(
                    //        lifetime: ServiceLifetime.Singleton)
                };

            await using var server = new NativeServer(9196, serverConfig);
			server.RegisterService<IAsyncService, AsyncService>();
			server.Start();

			await using var client = new NativeClient(9196, new ClientConfig());

            var proxy = client.CreateProxy<IAsyncService>();

            var base64String = await proxy.ConvertToBase64Async("Yay");

            Assert.Equal("WWF5", base64String);
        }

        /// <summary>
        /// Awaiting for ordinary non-generic task method should not hangs. 
        /// </summary>
        [Fact(Timeout = 15000)]
        public async Task AwaitingNonGenericTask_should_not_hang_forever()
        {
            var port = 9197;
            
            var serverConfig =
                new ServerConfig()
                {
                    //RegisterServicesAction = container =>
                    //    container.RegisterService<IAsyncService, AsyncService>(
                    //        lifetime: ServiceLifetime.Singleton)
                };

			await using var server = new NativeServer(port, serverConfig);
			server.RegisterService<IAsyncService, AsyncService>();
			server.Start();

            await using var client = new NativeClient(port, new ClientConfig());

            var proxy = client.CreateProxy<IAsyncService>();

            await proxy.NonGenericTask();
        }
    }
}
