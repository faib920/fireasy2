// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.Dynamic
{
    /// <summary>
    /// 用于为 <see cref="DynamicExpandoObject"/> 添加类型方法的成员。
    /// </summary>
    public sealed class DynamicDelegate
    {
        private DynamicInvokeDelegate _delegate;

        /// <summary>
        /// 定义一个动态调用的委托。
        /// </summary>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public static DynamicDelegate Define(DynamicInvokeDelegate @delegate)
        {
            return new DynamicDelegate { _delegate = @delegate };
        }

        internal object Invoke(object sender, object[] args)
        {
            if (_delegate == null)
            {
                return null;
            }

            return _delegate.DynamicInvoke(sender, args);
        }
    }

    /// <summary>
    /// 一个动态调用的委托。
    /// </summary>
    /// <param name="sender">被调用的当前对象。</param>
    /// <param name="args">调用的参数。</param>
    /// <returns></returns>
    public delegate object DynamicInvokeDelegate(dynamic sender, params object[] args);
}
