// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Data.Entity.Initializers
{
    public sealed class EntityContextPreInitializerCollection
    {
        private readonly List<IEntityContextPreInitializer> initializers = new List<IEntityContextPreInitializer>();

        public EntityContextPreInitializerCollection Add<T>(Action<T> setupAction = null) where T : IEntityContextPreInitializer
        {
            var initer = initializers.FirstOrDefault(s => s is T);
            if (initer == null)
            {
                initer = typeof(T).New<T>();
                initializers.Add(initer);
            }

            setupAction?.Invoke((T)initer);

            return this;
        }

        public EntityContextPreInitializerCollection Remove<T>() where T : IEntityContextPreInitializer
        {
            var initer = initializers.FirstOrDefault(s => s is T);
            if (initer != null)
            {
                initializers.Remove(initer);
            }

            return this;
        }

        public void PreInitialize(EntityContextPreInitializeContext context)
        {
            initializers.ForEach(s => s.PreInitialize(context));
        }
    }
}
