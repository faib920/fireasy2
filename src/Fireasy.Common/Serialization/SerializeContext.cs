using Fireasy.Common.ComponentModel;
using Fireasy.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fireasy.Common.Serialization
{
    internal class SerializeContext : IDisposable
    {
        private readonly List<object> objects = new List<object>();

        public SerializeContext()
        {
            GetAccessors = new Dictionary<Type, List<PropertyGetAccessorCache>>();
            SetAccessors = new Dictionary<Type, Dictionary<string, PropertyAccessor>>();
        }

        public Dictionary<Type, List<PropertyGetAccessorCache>> GetAccessors { get; set; }

        public Dictionary<Type, Dictionary<string, PropertyAccessor>> SetAccessors { get; set; }

        /// <summary>
        /// 尝试使用方法序列化对象。
        /// </summary>
        /// <param name="obj">要锁定的对象。</param>
        /// <param name="serializeMethod">被锁定的方法。</param>
        /// <exception cref="SerializationException">该对象被循环引用，即嵌套引用。</exception>
        internal void TrySerialize(object obj, Action serializeMethod)
        {
            if (obj == null)
            {
                serializeMethod();
                return;
            }

            if (objects.IndexOf(obj) != -1)
            {
                throw new SerializationException(SR.GetString(SRKind.LoopReferenceSerialize, obj));
            }

            try
            {
                objects.Add(obj);
                serializeMethod();
            }
            finally
            {
                objects.Remove(obj);
            }
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        public void Dispose()
        {
            GetAccessors.Clear();
            objects.Clear();
        }
    }

    internal class PropertyGetAccessorCache
    {
        public PropertyAccessor Accessor { get; set; }

        public Func<PropertyInfo, ILazyManager, bool> Filter { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public string PropertyName { get; set; }
    }
}
