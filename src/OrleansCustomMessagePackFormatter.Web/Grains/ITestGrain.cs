using Orleans;
using OrleansCustomJsonConverter.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansCustomJsonConverter.Web.Grains
{
    public interface ITestGrain : IGrainWithStringKey
    {
        public Task<TestModel> Dosomething(TestModel weather);

        public Task<TestId> Dosomething2(TestId testModel);

    }
}
