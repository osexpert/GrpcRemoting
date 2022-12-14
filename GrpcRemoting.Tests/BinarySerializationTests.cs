using System;
using GrpcRemoting.RpcMessaging;
using GrpcRemoting.Serialization.Binary;
using GrpcRemoting.Tests.Tools;
using Xunit;

namespace GrpcRemoting.Tests
{
    public class BinarySerializationTests
    {
        [Fact]
        public void BinarySerializerAdapter_should_deserialize_MethodCallMessage()
        {
            var serializer = new BinarySerializerAdapter();
            var testServiceInterfaceType = typeof(ITestService);
            
            var messageBuilder = new MethodCallMessageBuilder();

            var message =
                messageBuilder.BuildMethodCallMessage(
                    serializer,
                    testServiceInterfaceType.Name,
                    testServiceInterfaceType.GetMethod("TestMethod"),
                    new object[] { 4711});

            var rawData = serializer.Serialize(message);
            
            var deserializedMessage = serializer.Deserialize<MethodCallMessage>(rawData);
            
            deserializedMessage.UnwrapParametersFromDeserializedMethodCallMessage(
                out var parameterValues,
                out var parameterTypes);

            var parametersLength = deserializedMessage.Parameters.Length;
            
            Assert.Equal(1, parametersLength);
            Assert.NotNull(deserializedMessage.Parameters[0]);
            Assert.Equal("arg", deserializedMessage.Parameters[0].ParameterName);
            Assert.StartsWith("System.Object,", deserializedMessage.Parameters[0].ParameterTypeName);
            Assert.Equal(typeof(int), parameterValues[0].GetType());
            Assert.Equal(typeof(object), parameterTypes[0]);
            Assert.Equal(4711, parameterValues[0]);
        }

    }
}
