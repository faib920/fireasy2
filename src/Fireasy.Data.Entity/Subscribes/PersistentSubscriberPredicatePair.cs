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

        public Type ContextType { get; }

        PersistentSubscriber Subscriber { get; }
    }

    internal interface IAsyncPersistentSubscriberPredicatePair
    {
        Func<PersistentSubject, bool> Filter { get; }

        public Type ContextType { get; }

        AsyncPersistentSubscriber Subscriber { get; }
    }

    internal class PersistentSubscriberPredicatePair<TSubscriber> : IPersistentSubscriberPredicatePair where TSubscriber : PersistentSubscriber
    {
        public PersistentSubscriberPredicatePair(Func<PersistentSubject, bool> filter, PersistentSubscriber subscriber)
            : this(null, filter, subscriber)
        {
        }

        public PersistentSubscriberPredicatePair(Type contextType, Func<PersistentSubject, bool> filter, PersistentSubscriber subscriber)
        {
            ContextType = contextType;
            Filter = filter;
            Subscriber = subscriber;
        }

        public Func<PersistentSubject, bool> Filter { get; }

        public Type ContextType { get; }

        public PersistentSubscriber Subscriber { get; }
    }

    internal class AsyncPersistentSubscriberPredicatePair<TSubscriber> : IAsyncPersistentSubscriberPredicatePair where TSubscriber : AsyncPersistentSubscriber
    {
        public AsyncPersistentSubscriberPredicatePair(Func<PersistentSubject, bool> filter, AsyncPersistentSubscriber subscriber)
            : this (null, filter, subscriber)
        {
        }

        public AsyncPersistentSubscriberPredicatePair(Type contextType, Func<PersistentSubject, bool> filter, AsyncPersistentSubscriber subscriber)
        {
            ContextType = contextType;
            Filter = filter;
            Subscriber = subscriber;
        }

        public Func<PersistentSubject, bool> Filter { get; }

        public Type ContextType { get; }

        public AsyncPersistentSubscriber Subscriber { get; }
    }
}
