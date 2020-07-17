// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Threading;

namespace Fireasy.Common.Subscribes.Persistance
{
    /// <summary>
    /// 主题持久化定时器。无法继承此类。
    /// </summary>
    public sealed class PersistentTimer
    {
        private readonly Timer _timer;
        private bool _isInvoking;

        /// <summary>
        /// 初始化 <see cref="PersistentTimer"/> 类的新实例。
        /// </summary>
        /// <param name="persistance"></param>
        /// <param name="period"></param>
        /// <param name="action"></param>
        public PersistentTimer(TimeSpan period, Action action)
        {
            if (period == TimeSpan.Zero)
            {
                return;
            }

            _timer = new Timer(o =>
            {
                if (_isInvoking)
                {
                    return;
                }

                try
                {
                    _isInvoking = true;
                    action();
                }
                catch (Exception exp)
                {
                    Tracer.Error($"Throw exception when PersistentTimer is invoking:\n{exp.Output()}");
                }
                finally
                {
                    _isInvoking = false;
                }
            }, null, TimeSpan.FromSeconds(10), period);
        }
    }
}
