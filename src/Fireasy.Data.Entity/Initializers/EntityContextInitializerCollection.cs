// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fireasy.Data.Entity.Initializers
{
    public sealed class EntityContextInitializerCollection
    {
        private readonly List<IEntityContextPreInitializer> _preInitializers = new List<IEntityContextPreInitializer>();
        private readonly List<IEntityContextInitializer> _initializers = new List<IEntityContextInitializer>();

        public EntityContextInitializerCollection AddPreInitializer<T>(Action<T> setupAction = null) where T : IEntityContextPreInitializer
        {
            var initer = _preInitializers.FirstOrDefault(s => s is T);
            if (initer == null)
            {
                initer = typeof(T).New<T>();
                _preInitializers.Add(initer);
            }

            setupAction?.Invoke((T)initer);

            return this;
        }

        public EntityContextInitializerCollection AddInitializer<T>(Action<T> setupAction = null) where T : IEntityContextInitializer
        {
            var initer = _initializers.FirstOrDefault(s => s is T);
            if (initer == null)
            {
                initer = typeof(T).New<T>();
                _initializers.Add(initer);
            }

            setupAction?.Invoke((T)initer);

            return this;
        }

        public EntityContextInitializerCollection RemovePreInitializer<T>() where T : IEntityContextPreInitializer
        {
            var initer = _preInitializers.FirstOrDefault(s => s is T);
            if (initer != null)
            {
                _preInitializers.Remove(initer);
            }

            return this;
        }

        public EntityContextInitializerCollection RemoveInitializer<T>() where T : IEntityContextInitializer
        {
            var initer = _initializers.FirstOrDefault(s => s is T);
            if (initer != null)
            {
                _initializers.Remove(initer);
            }

            return this;
        }

        public void PreInitialize(EntityContextPreInitializeContext context)
        {
            _preInitializers.ForEach(s =>
            {
                var watch = Stopwatch.StartNew();
                s.PreInitialize(context);
                Tracer.Debug($"The {s.GetType().Name} was initialized ({watch.ElapsedMilliseconds}ms).");
            });
        }

        public void Initialize(EntityContextInitializeContext context)
        {
            _initializers.ForEach(s =>
            {
                var watch = Stopwatch.StartNew();
                s.Initialize(context);
                Tracer.Debug($"The {s.GetType().Name} was initialized ({watch.ElapsedMilliseconds}ms).");
            });
        }

    }
}
