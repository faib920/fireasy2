// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Fireasy.Data.Entity.Initializers
{
    public class EntityContextPreInitializerCollection
    {
        private List<IEntityContextPreInitializer> initializers = new List<IEntityContextPreInitializer>();

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
            foreach (var initer in initializers)
            {
                initer.PreInitialize(context);
            }
        }
    }
}
