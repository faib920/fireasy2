using Fireasy.Common.Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace Fireasy.Common.Tests.Localization
{
    [TestClass]
    public class XmlStringLocalizerTest
    {
        public XmlStringLocalizerTest()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestString()
        {
            var localizer = StringLocalizerFactory.CreateManager("xml").GetLocalizer("Localization.Test", typeof(DefaultStringLocalizerTest).Assembly);
            Console.WriteLine(localizer["String1"]);
        }

        [TestMethod]
        public void TestWithCulture()
        {
            var localizer = StringLocalizerFactory.CreateManager("xml").GetLocalizer("Localization.Test", CultureInfo.GetCultureInfo("en"), typeof(DefaultStringLocalizerTest).Assembly);
            Console.WriteLine(localizer["String1"]);
        }

        [TestMethod]
        public void TestFormat()
        {
            var localizer = StringLocalizerFactory.CreateManager("xml").GetLocalizer("Localization.Test", typeof(DefaultStringLocalizerTest).Assembly);
            Console.WriteLine(localizer["String3", "c:\\1.txt"]);
        }

        [TestMethod]
        public void TestNoExists()
        {
            var localizer = StringLocalizerFactory.CreateManager("xml").GetLocalizer("Localization.Test", CultureInfo.GetCultureInfo("ru"), typeof(DefaultStringLocalizerTest).Assembly);
            Console.WriteLine(localizer["String3", "c:\\1.txt"]);
        }

        [TestMethod]
        public void TestNoExistsKey()
        {
            var localizer = StringLocalizerFactory.CreateManager("xml").GetLocalizer("Localization.Test", typeof(DefaultStringLocalizerTest).Assembly);
            Console.WriteLine(localizer["String6"]);
        }
    }
}
