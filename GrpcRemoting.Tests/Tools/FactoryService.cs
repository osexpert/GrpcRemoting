namespace GrpcRemoting.Tests.Tools
{
    using GrpcRemoting;
    
    public class FactoryService : IFactoryService
    {
        public ITestService GetTestService()
        {
            return new TestService();
        }
    }
}