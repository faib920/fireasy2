using Fireasy.Common.Subscribes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fireasy.Common.Tests.Subscribes
{
    [TestClass]
    public class AliyunSubscribeTests
    {
        public AliyunSubscribeTests()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void Test()
        {
            var subMgr = SubscribeManagerFactory.CreateManager("aliyun");

            subMgr.Publish(new TestSubject { Key = "dfasfa" });
        }

        [Topic("TestSubject11")]
        public class TestSubject
        {
            public string Key { get; set; }
        }
    }
}
