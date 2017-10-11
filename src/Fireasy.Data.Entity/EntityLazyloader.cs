// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Properties;
using System;
using System.Linq;
using System.Threading;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体相关属性的延迟加载器。
    /// </summary>
    internal static class EntityLazyloader
    {
        /// <summary>
        /// 加载实体指定的属性的值。
        /// </summary>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="property">要加载的属性。</param>
        /// <returns></returns>
        internal static PropertyValue Load(IEntity entity, IProperty property)
        {
            Guard.ArgumentNull(entity, "entity");
            Guard.ArgumentNull(property, "property");

            var attr = property.GetType().GetCustomAttributes<PropertyLazyLoadderAttribute>().FirstOrDefault();
            Guard.Assert(attr != null, new EntityLazyloadException(SR.GetString(SRKind.NotRelationProperty, property.Name), entity, property));

            var loadder = attr.LoadderType.New<IPropertyLazyLoadder>();
            return loadder == null ? PropertyValue.Empty : loadder.GetValue(entity, property);
        }

        /// <summary>
        /// 异步加载实体指定的属性的值。
        /// </summary>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="property">要加载的属性。</param>
        /// <param name="initializer">初始化器。</param>
        internal static void AsyncLoad(IEntity entity, IProperty property, Action<PropertyValue> initializer = null)
        {
            var attr = property.GetType().GetCustomAttributes<PropertyLazyLoadderAttribute>().FirstOrDefault();
            Guard.Assert(attr != null, new EntityLazyloadException(SR.GetString(SRKind.NotRelationProperty, property.Name), entity, property));

            var loadder = attr.LoadderType.New<IPropertyLazyLoadder>();
            var asyncLoadder = new AsyncLoadder 
                { 
                    Entity = entity, 
                    Property = property, 
                    Loadder = loadder,
                    Initializer = initializer
                };

            var thread = new Thread(asyncLoadder.Load)
                {
                    IsBackground = true
                };

            thread.Start();
        }

        private class AsyncLoadder
        {
            public IEntity Entity { get; set; }

            public IProperty Property { get; set; }

            public IPropertyLazyLoadder Loadder { get; set; }

            public Action<PropertyValue> Initializer { get; set; }

            public void Load()
            {
                var value = Loadder.GetValue(Entity, Property);
                if (Initializer != null)
                {
                    Initializer(value);
                }
                else
                {
                    Entity.SetValue(Property, value);
                }
            }
        }
    }

}
