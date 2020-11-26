// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity.Subscribes
{
    internal interface IPersistentSubscriberPredicatePair
    {
        Func<PersistentSubject, bool> Filter { get; }

        PersistentSubscriber Subscriber { get; }
    }

    internal interface IAsyncPersistentSubscriberPredicatePair
    {
        Func<PersistentSubject, bool> Filter { get; }

        AsyncPersistentSubscriber Subscriber { get; }
    }

    internal class PersistentSubscriberPredicatePair<TSubscriber> : IPersistentSubscriberPredicatePair where TSubscriber : PersistentSubscriber
    {
        public PersistentSubscriberPredicatePair(Func<PersistentSubject, bool> filter, PersistentSubscriber subscriber)
        {
            Filter = filter;
            Subscriber = subscriber;
        }

        public Func<PersistentSubject, bool> Filter { get; }

        public PersistentSubscriber Subscriber { get; }
    }

    internal class AsyncPersistentSubscriberPredicatePair<TSubscriber> : IAsyncPersistentSubscriberPredicatePair where TSubscriber : AsyncPersistentSubscriber
    {
        public AsyncPersistentSubscriberPredicatePair(Func<PersistentSubject, bool> filter, AsyncPersistentSubscriber subscriber)
        {
            Filter = filter;
            Subscriber = subscriber;
        }

        public Func<PersistentSubject, bool> Filter { get; }

        public AsyncPersistentSubscriber Subscriber { get; }
    }
}
