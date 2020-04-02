// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using RabbitMQ.Client;
using System;
using System.Text;

namespace Fireasy.Aliyun.AMQP
{
	public class AliyunMechanism : AuthMechanism
    {
		public byte[] handleChallenge(byte[] challenge, IConnectionFactory factory)
		{
			if (factory is ConnectionFactory val)
			{
				return Encoding.UTF8.GetBytes("\0" + GetUserName(val) + "\0" + AliyunUtils.GetPassword(val.Password));
			}

			throw new InvalidCastException("need ConnectionFactory");
		}

		private string GetUserName(ConnectionFactory cf)
		{
			string text;
			try
			{
				text = cf.HostName.Split('.')[0];
			}
			catch (Exception)
			{
				throw new InvalidProgramException("hostName invalid");
			}

			return AliyunUtils.GetUserName(cf.UserName, text);
		}
	}
}
