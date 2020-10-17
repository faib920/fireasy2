// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Subscribes
{
    internal class SubscribeHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<IEnumerable<Type>> _typesGetter;

        public SubscribeHostedService(IServiceProvider serviceProvider, Func<IEnumerable<Type>> typesGetter)
        {
            _serviceProvider = serviceProvider;
            _typesGetter = typesGetter;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            Helper.RegisterSubscribers(_serviceProvider, _serviceProvider.TryGetService<ISubscribeManager>(), _typesGetter(), (sp, type) => sp.GetService(type));
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
#endif