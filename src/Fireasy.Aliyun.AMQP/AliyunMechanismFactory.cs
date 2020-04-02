// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using RabbitMQ.Client;

namespace Fireasy.Aliyun.AMQP
{
	public class AliyunMechanismFactory : AuthMechanismFactory
	{
		public string Name => "PLAIN";

		public AuthMechanism GetInstance()
		{
			return new AliyunMechanism();
		}
	}
}
