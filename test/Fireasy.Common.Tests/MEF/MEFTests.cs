using Fireasy.Common.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;

namespace Fireasy.Common.Tests.MEF
{
    [TestClass]
    public class MEFTests
    {
        public MEFTests()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestExport()
        {
            var svr = Imports.GetService<IExportTest>();
            Assert.IsNotNull(svr);
        }
    }

    public interface IExportTest
    {

    }

    [Export(typeof(IExportTest))]
    public class ExportTest : IExportTest
    {

    }
}
