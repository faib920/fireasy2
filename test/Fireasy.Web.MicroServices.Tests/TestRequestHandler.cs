using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fireasy.Web.MicroServices.Tests
{
    public class TestRequestHandler : RequestHandlerBase<TestQuery, List<TestResult>>
    {
        public override async Task<List<TestResult>> ProcessAsync(TestQuery request)
        {
            return new List<TestResult> { new TestResult { Name = request.Name , Address = "kunming", Year = 2019 } }; 
        }
    }

    public class TestTemplate
    {
        public string Name { get; set; }
    }

    public class TestQuery : TestTemplate
    {
        public int Year { get; set; }
    }

    public class TestResult : TestTemplate
    {
        public int Year { get; set; }

        public string Address { get; set; }
    }

    [RequestTicket("test")]
    public class TestRequest : RequestBase<TestQuery, List<TestResult>>
    {
        public TestRequest(TestQuery query)
            : base (query)
        {
        }
    }
}
