

using Orleans;
using OrleansCustomJsonConverter.Web.Models;

namespace OrleansCustomJsonConverter.Web.Grains
{
    public class TestGrain : ITestGrain
    {
        private readonly IClusterClient _clusterClient;

        public TestGrain(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        public Task<TestModel> Dosomething(TestModel testModel)
        {
            return Task.FromResult(testModel);
        }

        public Task<TestId> Dosomething2(TestId testModel)
        {
            return Task.FromResult(testModel);
        }

    }
}