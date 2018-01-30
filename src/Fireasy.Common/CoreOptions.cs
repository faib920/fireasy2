// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD2_0
using Fireasy.Common.Subscribe;

namespace Fireasy.Common
{
    public class CoreOptions
    {
        public void AddSubscribe<T>(ISubscriber subscriber) where T : ISubject
        {
            SubscribeManager.Register<T>(subscriber);
        }
    }
}
#endif
