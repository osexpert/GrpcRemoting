using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcRemoting.Tests.ExternalTypes;
using GrpcRemoting.Tests.Tools;
using Xunit;
using Xunit.Abstractions;

namespace GrpcRemoting.Tests
{
    public class RpcTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RpcTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Call_on_Proxy_should_be_invoked_on_remote_service()
        {
            bool remoteServiceCalled = false;

            var testService =
                new TestService()
                {
                    TestMethodFake = arg =>
                    {
                        remoteServiceCalled = true;
                        return arg;
                    }
                };
            
            var serverConfig =
                new ServerConfig()
                {
                    //RegisterServicesAction = container =>
                    //    container.RegisterService<ITestService>(
                    //        factoryDelegate: () => testService,
                    //        lifetime: ServiceLifetime.Singleton)
                    CreateInstance = (t) => testService
                };

            await using var server = new NativeServer(9094, serverConfig);
			server.Start();
			server.RegisterService<ITestService, TestService>();
			
            async Task ClientAction()
            {
                try
                {
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    await using var client = new NativeClient(9094, new ClientConfig());

                    stopWatch.Stop();
                    _testOutputHelper.WriteLine($"Creating client took {stopWatch.ElapsedMilliseconds} ms");
                    stopWatch.Reset();
                    stopWatch.Start();

					//client.Connect();
					
                    stopWatch.Stop();
                    _testOutputHelper.WriteLine($"Establishing connection took {stopWatch.ElapsedMilliseconds} ms");
                    stopWatch.Reset();
                    stopWatch.Start();
                    
                    var proxy = client.CreateProxy<ITestService>();
                    
                    stopWatch.Stop();
                    _testOutputHelper.WriteLine($"Creating proxy took {stopWatch.ElapsedMilliseconds} ms");
                    stopWatch.Reset();
                    stopWatch.Start();
                    
                    var result = proxy.TestMethod("test");

                    stopWatch.Stop();
                    _testOutputHelper.WriteLine($"Remote method invocation took {stopWatch.ElapsedMilliseconds} ms");
                    stopWatch.Reset();
                    stopWatch.Start();
                    
                    var result2 = proxy.TestMethod("test");

                    stopWatch.Stop();
                    _testOutputHelper.WriteLine($"Second remote method invocation took {stopWatch.ElapsedMilliseconds} ms");
                    
                    Assert.Equal("test", result);
                    Assert.Equal("test", result2);
                    
                    proxy.MethodWithOutParameter(out int methodCallCount);
                    
                    Assert.Equal(1, methodCallCount);
                }
                catch (Exception e)
                {
                    _testOutputHelper.WriteLine(e.ToString());
                    throw;
                }
            }

            var clientThread = new Thread(async () =>
            {
                await ClientAction();
            });
            clientThread.Start();
            clientThread.Join();
            
            Assert.True(remoteServiceCalled);
        }
        
        [Fact]
        public async Task Call_on_Proxy_should_be_invoked_on_remote_service_without_MessageEncryption()
        {
            bool remoteServiceCalled = false;

            var testService =
                new TestService()
                {
                    TestMethodFake = arg =>
                    {
                        remoteServiceCalled = true;
                        return arg;
                    }
                };
            
            var serverConfig =
                new ServerConfig()
                {
                    //RegisterServicesAction = container =>
                    //    container.RegisterService<ITestService>(
                    //        factoryDelegate: () => testService,
                    //        lifetime: ServiceLifetime.Singleton)
                    CreateInstance = (t) => testService
                };

          
            await using var server = new NativeServer(9094, serverConfig);
			server.RegisterService<ITestService, TestService>();
			server.Start();

            async Task ClientAction()
            {
                try
                {
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    await using var client = new NativeClient(9094, new ClientConfig());

                    stopWatch.Stop();
                    _testOutputHelper.WriteLine($"Creating client took {stopWatch.ElapsedMilliseconds} ms");
                    stopWatch.Reset();
                    stopWatch.Start();
                    
                    //client.Connect();

                    stopWatch.Stop();
                    _testOutputHelper.WriteLine($"Establishing connection took {stopWatch.ElapsedMilliseconds} ms");
                    stopWatch.Reset();
                    stopWatch.Start();
                    
                    var proxy = client.CreateProxy<ITestService>();
                    
                    stopWatch.Stop();
                    _testOutputHelper.WriteLine($"Creating proxy took {stopWatch.ElapsedMilliseconds} ms");
                    stopWatch.Reset();
                    stopWatch.Start();
                    
                    var result = proxy.TestMethod("test");

                    stopWatch.Stop();
                    _testOutputHelper.WriteLine($"Remote method invocation took {stopWatch.ElapsedMilliseconds} ms");
                    stopWatch.Reset();
                    stopWatch.Start();
                    
                    var result2 = proxy.TestMethod("test");

                    stopWatch.Stop();
                    _testOutputHelper.WriteLine($"Second remote method invocation took {stopWatch.ElapsedMilliseconds} ms");
                    
                    Assert.Equal("test", result);
                    Assert.Equal("test", result2);
                }
                catch (Exception e)
                {
                    _testOutputHelper.WriteLine(e.ToString());
                    throw;
                }
            }

            var clientThread = new Thread(async () => 
            { 
                await ClientAction(); 
            });
            clientThread.Start();
            clientThread.Join();
            
            Assert.True(remoteServiceCalled);
        }

        [Fact]
        public async Task Delegate_invoked_on_server_should_callback_client()
        {
            string argumentFromServer = null;

            var testService = new TestService();
            
            var serverConfig =
                new ServerConfig()
                {
                    //RegisterServicesAction = container =>
                    //    container.RegisterService<ITestService>(
                    //        factoryDelegate: () => testService,
                    //        lifetime: ServiceLifetime.Singleton)
                };

            await using var server = new NativeServer(9095, serverConfig);
			server.RegisterService<ITestService, TestService>();
			server.Start();

            async Task ClientAction()
            {
                try
                {
                    await using var client = new NativeClient(9095, new ClientConfig());

                    var proxy = client.CreateProxy<ITestService>();
                    proxy.TestMethodWithDelegateArg(arg => argumentFromServer = arg);
                }
                catch (Exception e)
                {
                    _testOutputHelper.WriteLine(e.ToString());
                    throw;
                }
            }

            var clientThread = new Thread(async () =>
            {
                await ClientAction();
            });
            clientThread.Start();
            clientThread.Join();
                
            Assert.Equal("test", argumentFromServer);
        }
        
        [Fact]
        public async Task Events_should_NOT_work_remotly()
        {
            var testService = new TestService();

            var serverConfig =
                new ServerConfig()
                {
                    //RegisterServicesAction = container =>
                    //    container.RegisterService<ITestService>(
                    //        factoryDelegate: () => testService,
                    //        lifetime: ServiceLifetime.Singleton)
                    CreateInstance = (t) => testService
                };

            bool serviceEventCalled = false;
            
            await using var server = new NativeServer(9096, serverConfig);
            server.RegisterService<ITestService, TestService>();
            server.Start();

            await using var client = new NativeClient(9096, new ClientConfig());

            var proxy = client.CreateProxy<ITestService>();
            
            // Does not support this. But maybe we should fail better than we do currently?
            // Calling a delegate in client from server is in GrpcRemoting only supported while the call is active,
            // because only then is the callback channel open.
            proxy.ServiceEvent += () => serviceEventCalled = true;

            Assert.Throws<System.Threading.Channels.ChannelClosedException>(() => proxy.FireServiceEvent());

            Assert.False(serviceEventCalled);
        }
        
        [Fact]
        public async Task External_types_should_work_as_remote_service_parameters()
        {
            bool remoteServiceCalled = false;
            DataClass parameterValue = null;

            var testService =
                new TestService()
                {
                    TestExternalTypeParameterFake = arg =>
                    {
                        remoteServiceCalled = true;
                        parameterValue = arg;
                    }
                };

            var serverConfig =
                new ServerConfig()
                {
                    //RegisterServicesAction = container =>
                    //    container.RegisterService<ITestService>(
                    //        factoryDelegate: () => testService,
                    //        lifetime: ServiceLifetime.Singleton)
                    CreateInstance = (t) => testService
                };

           
            await using var server = new NativeServer(9097, serverConfig);
            server.RegisterService<ITestService, TestService>();
            server.Start();

            async Task ClientAction()
            {
                try
                {
                    await using var client = new NativeClient(9097, new ClientConfig());

                    var proxy = client.CreateProxy<ITestService>();
                    proxy.TestExternalTypeParameter(new DataClass() {Value = 42});

                    Assert.Equal(42, parameterValue.Value);
                }
                catch (Exception e)
                {
                    _testOutputHelper.WriteLine(e.ToString());
                    throw;
                }
            }

            var clientThread = new Thread(async () =>
            {
                await ClientAction();
            });
            clientThread.Start();
            clientThread.Join();
            
            Assert.True(remoteServiceCalled);
        }
        
        #region Service with generic method
        
        public interface IGenericEchoService
        {
            T Echo<T>(T value);
        }

        public class GenericEchoService : IGenericEchoService
        {
            public T Echo<T>(T value)
            {
                return value;
            }
        }

        #endregion
        
        [Fact]
        public async Task Generic_methods_should_be_called_correctly()
        {
            var serverConfig =
                new ServerConfig()
                {
                    //RegisterServicesAction = container =>
                    //    container.RegisterService<IGenericEchoService, GenericEchoService>(
                    //        lifetime: ServiceLifetime.Singleton)
                };

            await using var server = new NativeServer(9197, serverConfig);
            server.RegisterService<IGenericEchoService, GenericEchoService>();
            server.Start();

            await using var client = new NativeClient(9197, new ClientConfig());

            var proxy = client.CreateProxy<IGenericEchoService>();

            var result = proxy.Echo("Yay");
            
            Assert.Equal("Yay", result);
        }
        
        #region Service with enum as operation argument

        public enum TestEnum
        {
            First = 1,
            Second = 2
        }

        public interface IEnumTestService
        {
            TestEnum Echo(TestEnum inputValue);
        }

        public class EnumTestService : IEnumTestService
        {
            public TestEnum Echo(TestEnum inputValue)
            {
                return inputValue;
            }
        }

        #endregion
        
        [Fact]
        public async Task Enum_arguments_should_be_passed_correctly()
        {
            var serverConfig =
                new ServerConfig()
                {
                    //RegisterServicesAction = container =>
                    //    container.RegisterService<IEnumTestService, EnumTestService>(
                    //        lifetime: ServiceLifetime.Singleton)
                };

            await using var server = new NativeServer(9198, serverConfig);
			server.RegisterService<IEnumTestService, EnumTestService>();
			server.Start();

            await using var client = new NativeClient(9198, new ClientConfig());

            var proxy = client.CreateProxy<IEnumTestService>();

            var resultFirst = proxy.Echo(TestEnum.First);
            var resultSecond = proxy.Echo(TestEnum.Second);
            
            Assert.Equal(TestEnum.First, resultFirst);
            Assert.Equal(TestEnum.Second, resultSecond);
        }
    }
}
