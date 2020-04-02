// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Fireasy.Aliyun.AMQP
{
    internal class AliyunUtils
    {
        internal static readonly int FROM_USER = 0;

        internal static readonly string COLON = ":";

        internal static readonly DateTime EPOCH_START = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static string GetUserName(string ak, string resourceOwnerId)
        {
            var sb = new StringBuilder(64);
            sb.Append(FROM_USER).Append(COLON).Append(resourceOwnerId)
                .Append(COLON)
                .Append(ak);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        public static string GetUserName(string ak, string resourceOwnerId, string stsToken)
        {
            var sb = new StringBuilder(64);
            sb.Append(FROM_USER).Append(COLON).Append(resourceOwnerId)
                .Append(COLON)
                .Append(ak)
                .Append(COLON)
                .Append(stsToken);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        public static string GetPassword(string sk)
        {
            var value = Convert.ToInt64((DateTime.UtcNow - EPOCH_START).TotalMilliseconds);
            var sha1 = KeyedHashAlgorithm.Create("HMACSHA1");
            if (sha1 == null)
            {
                throw new InvalidOperationException("HMACSHA1 not exist!");
            }
            try
            {
                sha1.Key = Encoding.UTF8.GetBytes(value.ToString());
                var value2 = sha1.ComputeHash(Encoding.UTF8.GetBytes(sk)).ToHex();
                var sb = new StringBuilder(64);
                sb.Append(value2).Append(COLON).Append(value);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
            }
            finally
            {
                sha1.Clear();
            }
        }
    }
}
