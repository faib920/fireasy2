// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Threading.Tasks;

namespace Fireasy.Common.Subscribes
{
    /// <summary>
    /// 订阅者委托的抽象类。
    /// </summary>
    public abstract class SubscribeDelegate
    {
        /// <summary>
        /// 初始化 <see cref="SubscribeDelegate"/> 类的新实例。
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="handler"></param>
        protected SubscribeDelegate(Type dataType, Delegate handler)
        {
            Guard.ArgumentNull(handler, nameof(handler));

            Handler = handler;
            DataType = dataType ?? handler.Method.GetParameters()[0].ParameterType;
        }

        /// <summary>
        /// 获取数据类型。
        /// </summary>
        public Type DataType { get; private set; }

        /// <summary>
        /// 获取委托。
        /// </summary>
        public Delegate Handler { get; private set; }

        /// <summary>
        /// 执行订阅。
        /// </summary>
        /// <param name="data"></param>
        public abstract void Invoke(object data);
    }

    /// <summary>
    /// 同步的订阅者委托。
    /// </summary>
    public class SyncSubscribeDelegate : SubscribeDelegate
    {
        /// <summary>
        /// 初始化 <see cref="SyncSubscribeDelegate"/> 类的新实例。
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="handler"></param>
        public SyncSubscribeDelegate(Type dataType, Delegate handler)
            : base(dataType, handler)
        {
        }

        /// <summary>
        /// 执行订阅。
        /// </summary>
        /// <param name="data"></param>
        public override void Invoke(object data)
        {
            Handler.DynamicInvoke(data);
        }
    }

    /// <summary>
    /// 异步的订阅者委托。
    /// </summary>
    public class AsyncSubscribeDelegate : SubscribeDelegate
    {
        /// <summary>
        /// 初始化 <see cref="AsyncSubscribeDelegate"/> 类的新实例。
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="handler"></param>
        public AsyncSubscribeDelegate(Type dataType, Delegate handler)
            : base(dataType, handler)
        {
        }

        /// <summary>
        /// 执行订阅。
        /// </summary>
        /// <param name="data"></param>
        public override void Invoke(object data)
        {
            ((Task)Handler.DynamicInvoke(data)).AsSync();
        }
    }
}
