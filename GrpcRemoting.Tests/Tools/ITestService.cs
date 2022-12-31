using System;
using GrpcRemoting.Tests.ExternalTypes;

namespace GrpcRemoting.Tests.Tools
{
//    [ReturnAsProxy]
    public interface ITestService
    {
        event Action ServiceEvent;
        
        object TestMethod(object arg);

        void TestMethodWithDelegateArg(Action<string> callback);

        void FireServiceEvent();

//        [OneWay]
        void OneWayMethod();

        void TestExternalTypeParameter(DataClass data);

        string Echo(string text);

        void MethodWithOutParameter(out int counter);
    }
}