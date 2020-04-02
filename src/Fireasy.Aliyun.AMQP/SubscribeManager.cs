// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using RabbitMQ.Client;
using System.Collections.Generic;

namespace Fireasy.Aliyun.AMQP
{
    public class SubscribeManager : RabbitMQ.SubscribeManager
    {
        protected override ConnectionFactory CreateConnectionFactory()
        {
            var factory = base.CreateConnectionFactory();
            factory.AuthMechanisms = new List<AuthMechanismFactory>() { new AliyunMechanismFactory() };
            return factory;
        }
    }
}
