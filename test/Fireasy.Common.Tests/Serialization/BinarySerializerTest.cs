// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fireasy.Common.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Common.Security;

namespace Fireasy.Common.Tests.Serialization
{
    [TestClass()]
	public class BinarySerializerTest
	{
		[TestMethod()]
		public void TestCompressSerialize()
		{
			var serializer = new BinaryCompressSerializer();

			var obj = new BinaryCryptoData
				{
					Age = 12,
					Name = "huangxd",
					Birthday = DateTime.Parse("1982-9-20")
				};

			var bytes = serializer.Serialize(obj);

			Assert.IsNotNull(bytes);
		}

		[TestMethod()]
		public void TestCompressDeserialize()
		{
			var serializer = new BinaryCompressSerializer();

			#region
			var bytes = new byte[] {
				101,
				144,
				203,
				74,
				3,
				65,
				16,
				69,
				59,
				137,
				49,
				1,
				31,
				43,
				93,
				184,
				155,
				189,
				67,
				99,
				92,
				207,
				136,
				78,
				66,
				22,
				10,
				33,
				144,
				224,
				54,
				148,
				51,
				197,
				164,
				152,
				126,
				132,
				126,
				128,
				237,
				194,
				181,
				63,
				227,
				79,
				228,
				199,
				180,
				91,
				116,
				161,
				222,
				130,
				170,
				197,
				189,
				156,
				11,
				197,
				122,
				140,
				177,
				143,
				168,
				116,
				147,
				142,
				251,
				113,
				221,
				207,
				201,
				32,
				216,
				192,
				167,
				90,
				74,
				173,
				214,
				104,
				157,
				205,
				179,
				71,
				52,
				150,
				180,
				42,
				39,
				252,
				42,
				77,
				158,
				77,
				189,
				112,
				222,
				96,
				169,
				208,
				59,
				3,
				34,
				207,
				150,
				254,
				73,
				80,
				253,
				128,
				97,
				173,
				59,
				84,
				165,
				242,
				66,
				12,
				19,
				120,
				249,
				155,
				199,
				87,
				104,
				8,
				4,
				189,
				128,
				139,
				60,
				158,
				240,
				188,
				34,
				5,
				38,
				68,
				127,
				103,
				208,
				218,
				159,
				0,
				154,
				175,
				238,
				203,
				111,
				215,
				132,
				157,
				211,
				51,
				112,
				48,
				136,
				208,
				243,
				98,
				1,
				18,
				111,
				186,
				205,
				166,
				130,
				186,
				35,
				213,
				206,
				9,
				69,
				115,
				81,
				84,
				100,
				220,
				182,
				129,
				240,
				215,
				57,
				43,
				238,
				218,
				127,
				241,
				30,
				27,
				156,
				156,
				174,
				130,
				117,
				40,
				249,
				12,
				107,
				146,
				32,
				210,
				3,
				14,
				83,
				193,
				104,
				235,
				65,
				181,
				207,
				13,
				187,
				125,
				125,
				59,
				216,
				191,
				143,
				199,
				195,
				254,
				228,
				250,
				232,
				19 };
			#endregion

			var obj = serializer.Deserialize<BinaryCryptoData>(bytes);

			Assert.IsNotNull(obj);
		}

        /// <summary>
        /// 构造器测试。
        /// </summary>
        [TestMethod()]
        public void TestCryptoSerializer()
        {
            var crypt = CryptographyFactory.Create("des");
            var serializer = new BinaryCryptoSerializer(crypt);

            Assert.IsNotNull(serializer);
        }

        /// <summary>
        /// 测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestCryptoSerialize1()
        {
            var crypt = CryptographyFactory.Create("des");
            var serializer = new BinaryCryptoSerializer(crypt);

            var obj = new BinaryCryptoData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var bytes = serializer.Serialize(obj);

            Assert.IsNotNull(bytes);
        }

        /// <summary>
        /// 测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestCryptoDeserializeTest()
        {
            var crypt = CryptographyFactory.Create("des");
            var serializer = new BinaryCryptoSerializer(crypt);

            #region
            var bytes = new byte[] {
                    117,
                    95,
                    97,
                    41,
                    160,
                    1,
                    189,
                    29,
                    162,
                    14,
                    160,
                    175,
                    84,
                    107,
                    87,
                    247,
                    155,
                    87,
                    129,
                    58,
                    66,
                    191,
                    30,
                    90,
                    1,
                    137,
                    109,
                    118,
                    182,
                    214,
                    144,
                    40,
                    164,
                    169,
                    148,
                    64,
                    193,
                    58,
                    136,
                    13,
                    192,
                    106,
                    55,
                    252,
                    182,
                    139,
                    33,
                    12,
                    87,
                    85,
                    131,
                    181,
                    116,
                    164,
                    36,
                    36,
                    52,
                    243,
                    199,
                    197,
                    237,
                    213,
                    74,
                    73,
                    78,
                    180,
                    205,
                    186,
                    58,
                    59,
                    211,
                    47,
                    122,
                    162,
                    156,
                    22,
                    230,
                    174,
                    15,
                    210,
                    108,
                    225,
                    183,
                    76,
                    160,
                    176,
                    210,
                    88,
                    168,
                    7,
                    210,
                    179,
                    14,
                    97,
                    131,
                    5,
                    56,
                    214,
                    77,
                    20,
                    9,
                    2,
                    209,
                    166,
                    1,
                    137,
                    109,
                    118,
                    182,
                    214,
                    144,
                    40,
                    82,
                    251,
                    42,
                    118,
                    31,
                    182,
                    151,
                    87,
                    0,
                    72,
                    127,
                    33,
                    192,
                    235,
                    90,
                    196,
                    241,
                    19,
                    70,
                    11,
                    9,
                    138,
                    214,
                    54,
                    38,
                    209,
                    252,
                    192,
                    99,
                    48,
                    82,
                    74,
                    150,
                    247,
                    109,
                    228,
                    218,
                    40,
                    94,
                    190,
                    97,
                    38,
                    96,
                    158,
                    70,
                    239,
                    112,
                    40,
                    12,
                    144,
                    223,
                    161,
                    20,
                    38,
                    247,
                    236,
                    249,
                    98,
                    118,
                    39,
                    51,
                    174,
                    148,
                    137,
                    58,
                    13,
                    64,
                    16,
                    195,
                    162,
                    174,
                    211,
                    73,
                    72,
                    169,
                    153,
                    102,
                    147,
                    227,
                    191,
                    80,
                    62,
                    20,
                    172,
                    28,
                    17,
                    82,
                    12,
                    217,
                    173,
                    240,
                    143,
                    114,
                    39,
                    218,
                    7,
                    32,
                    165,
                    143,
                    23,
                    230,
                    217,
                    182,
                    94,
                    185,
                    196,
                    207,
                    114,
                    175,
                    154,
                    225,
                    22,
                    90,
                    40,
                    204,
                    187,
                    133,
                    39,
                    98,
                    38,
                    169,
                    80,
                    104,
                    94,
                    117,
                    95,
                    187,
                    187,
                    152,
                    107,
                    125,
                    125,
                    205,
                    14,
                    154,
                    138,
                    20,
                    238,
                    89,
                    97,
                    151,
                    244,
                    96,
                    238,
                    219,
                    249,
                    140,
                    149,
                    170,
                    90,
                    175,
                    236,
                    254,
                    98,
                    91,
                    173,
                    71,
                    209,
                    87,
                    122,
                    47,
                    155,
                    174,
                    91,
                    62,
                    141,
                    88,
                    92,
                    156,
                    114,
                    93,
                    174,
                    217,
                    28,
                    118,
                    42,
                    154,
                    190,
                    242,
                    135,
                    98,
                    129,
                    129,
                    95,
                    166,
                    107,
                    223,
                    191,
                    2,
                    252,
                    151,
                    187,
                    145,
                    63,
                    109,
                    138,
                    153,
                    154,
                    246,
                    180 };
            #endregion

            var obj = serializer.Deserialize<BinaryCryptoData>(bytes);

            Assert.IsNotNull(obj);
        }

        /// <summary>
        /// 使用Md5加密算法测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeUseMd5()
        {
            var crypt = CryptographyFactory.Create("md5");
            var serializer = new BinaryCryptoSerializer(crypt);

            var obj = new BinaryCryptoData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var bytes = serializer.Serialize(obj);

            Assert.IsNotNull(bytes);
        }

        /// <summary>
        /// 使用Md5加密算法测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeUseMd5()
        {
            var crypt = CryptographyFactory.Create("md5");
            var serializer = new BinaryCryptoSerializer(crypt);

            #region
            var bytes = new byte[] {
                64,
                110,
                111,
                194,
                121,
                248,
                123,
                58,
                12,
                137,
                116,
                110,
                166,
                232,
                5,
                110 };
            #endregion

            var obj = serializer.Deserialize<BinaryCryptoData>(bytes);

            Assert.IsNotNull(obj);
        }

        /// <summary>
        /// 测试数据的结构。
        /// </summary>
        [Serializable]
		private class BinaryCryptoData
		{
			public string Name { get; set; }

			public DateTime Birthday { get; set; }

			public decimal? Age { get; set; }
		}
	}
}
