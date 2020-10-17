// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Threading;

namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 为对象提供生存能力。无法继承此类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AliveObject<T> : DisposableBase where T : class
    {
        private readonly Func<T> _creator;
        private readonly Timer _timer;
        private readonly AliveOptions _options;
        private T _value;
        private Exception _lastException;
        private readonly object _locker = new object();

        /// <summary>
        /// 初始化 <see cref="AliveObject{T}"/> 类的新实例。
        /// </summary>
        /// <param name="crator">用于生成实例的函数。</param>
        /// <param name="policy">检测策略。</param>
        /// <param name="options">选项。</param>
        public AliveObject(Func<T> crator, AliveCheckPolicy<T> policy, AliveOptions options = null)
        {
            _creator = crator;
            _options = options;
            _timer = new Timer(o =>
            {
                if (_value == null)
                {
                    GetValue();
                }
                else if (policy.HealthChecker != null && _value != null && !policy.HealthChecker(_value))
                {
                    GetValue(true);
                }
            }, null, TimeSpan.FromSeconds(5), policy.Period);
        }

        /// <summary>
        /// 当值创建时（包括重新创建）的通知方法。
        /// </summary>
        public Action<T> OnValueCreated { get; set; }

        /// <summary>
        /// 获取当前实例的值。
        /// </summary>
        public T Value
        {
            get
            {
                if (GetValue() == null && _options?.ThrowException == true && _lastException != null)
                {
                    throw _lastException;
                }

                return _value;
            }
        }

        /// <summary>
        /// 获取值是否已创建。
        /// </summary>
        public bool IsValueCreated => _value != null;

        protected override bool Dispose(bool disposing)
        {
            _timer?.Dispose();
            _value?.TryDispose();

            return base.Dispose(disposing);
        }

        private T GetValue(bool forceNew = false)
        {
            try
            {
                if (_value != null && !forceNew)
                {
                    return _value;
                }

                lock (_locker)
                {
                    if (forceNew || _value == null)
                    {
                        if (_value != null)
                        {
                            _value.TryDispose();
                        }

                        _value = _creator();
                    }

                    if (_value != null)
                    {
                        OnValueCreated?.Invoke(_value);
                    }
                }

                return _value;
            }
            catch (Exception exp)
            {
                _lastException = exp;
                _value = null;
                return null;
            }
        }
    }

    /// <summary>
    /// 对象生存检测的策略.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AliveCheckPolicy<T>
    {
        /// <summary>
        /// 初始化 <see cref="AliveCheckPolicy{T}"/> 类的新实例.
        /// </summary>
        /// <param name="healthChecker">健康检测函数。</param>
        /// <param name="period">健康检测的间隔时间。</param>
        public AliveCheckPolicy(Func<T, bool> healthChecker, TimeSpan period)
        {
            HealthChecker = healthChecker;
            Period = period;
        }

        /// <summary>
        /// 获取或设置健康检测函数。
        /// </summary>
        public Func<T, bool> HealthChecker { get; set; }

        /// <summary>
        /// 获取或设置健康检测的间隔时间。
        /// </summary>
        public TimeSpan Period { get; set; }
    }

    public class AliveOptions
    {
        /// <summary>
        /// 获取或设置是否抛出异常。
        /// </summary>
        public bool ThrowException { get; set; }
    }
}
