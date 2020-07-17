// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Fireasy.Aliyun.AMQP
{
    public class SubscribeManager : RabbitMQ.SubscribeManager
    {
        public SubscribeManager()
            : base()
        {
        }

        public SubscribeManager(IServiceProvider serviceProvider)
            : base (serviceProvider)
        {
        }

        protected override ConnectionFactory CreateConnectionFactory()
        {
            var factory = base.CreateConnectionFactory();
            factory.AuthMechanisms = new List<AuthMechanismFactory>() { new AliyunMechanismFactory() };
            return factory;
        }
    }
}
