// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Threading;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// 用于拦截命令的执行。
    /// </summary>
    public abstract class DbCommandInterceptor
    {
        /// <summary>
        /// 执行 ExecuteNonQuery 方法之前。
        /// </summary>
        /// <param name="context"></param>
        public virtual void OnBeforeExecuteNonQuery(DbCommandInterceptContext<int> context)
        {
        }

        /// <summary>
        /// 执行 ExecuteNonQueryAsync 方法之前。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task OnBeforeExecuteNonQueryAsync(DbCommandInterceptContext<int> context, CancellationToken cancellationToken = default)
        {
            return TaskCompatible.CompletedTask;
        }

        /// <summary>
        /// 执行 ExecuteNonQuery 方法之后。
        /// </summary>
        /// <param name="context"></param>
        public virtual void OnAfterExecuteNonQuery(DbCommandInterceptContext<int> context)
        {
        }

        /// <summary>
        /// 执行 ExecuteNonQueryAsync 方法之后。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task OnAfterExecuteNonQueryAsync(DbCommandInterceptContext<int> context, CancellationToken cancellationToken = default)
        {
            return TaskCompatible.CompletedTask;
        }

        /// <summary>
        /// 执行 ExecuteScalar 方法之前。
        /// </summary>
        /// <param name="context"></param>
        public virtual void OnBeforeExecuteScalar(DbCommandInterceptContext<object> context)
        {
        }

        /// <summary>
        /// 执行 ExecuteScalarAsync 方法之前。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task OnBeforeExecuteScalarAsync(DbCommandInterceptContext<object> context, CancellationToken cancellationToken = default)
        {
            return TaskCompatible.CompletedTask;
        }

        /// <summary>
        /// 执行 ExecuteScalar 方法之后。
        /// </summary>
        /// <param name="context"></param>
        public virtual void OnAfterExecuteScalar(DbCommandInterceptContext<object> context)
        {
        }

        /// <summary>
        /// 执行 ExecuteScalarAsync 方法之后。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task OnAfterExecuteScalarAsync(DbCommandInterceptContext<object> context, CancellationToken cancellationToken = default)
        {
            return TaskCompatible.CompletedTask;
        }

        /// <summary>
        /// 执行 ExecuteEnumerable 方法之前。
        /// </summary>
        /// <param name="context"></param>
        public virtual void OnBeforeExecuteEnumerable(DbCommandInterceptContext<IEnumerable<dynamic>> context)
        {
        }

        /// <summary>
        /// 执行 ExecuteEnumerableAsync 方法之前。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task OnBeforeExecuteEnumerableAsync(DbCommandInterceptContext<IEnumerable<dynamic>> context, CancellationToken cancellationToken = default)
        {
            return TaskCompatible.CompletedTask;
        }

        /// <summary>
        /// 执行 ExecuteEnumerable 方法之后。
        /// </summary>
        /// <param name="context"></param>
        public virtual void OnAfterExecuteEnumerable(DbCommandInterceptContext<IEnumerable<dynamic>> context)
        {
        }

        /// <summary>
        /// 执行 ExecuteEnumerableAsync 方法之后。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task OnAfterExecuteEnumerableAsync(DbCommandInterceptContext<IEnumerable<dynamic>> context, CancellationToken cancellationToken = default)
        {
            return TaskCompatible.CompletedTask;
        }

        /// <summary>
        /// 执行 ExecuteEnumerable 方法之前。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        public virtual void OnBeforeExecuteEnumerable<T>(DbCommandInterceptContext<IEnumerable<T>> context)
        {
        }

        /// <summary>
        /// 执行 ExecuteEnumerableAsync 方法之前。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task OnBeforeExecuteEnumerableAsync<T>(DbCommandInterceptContext<IEnumerable<T>> context, CancellationToken cancellationToken = default)
        {
            return TaskCompatible.CompletedTask;
        }

        /// <summary>
        /// 执行 ExecuteEnumerable 方法之后。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        public virtual void OnAfterExecuteEnumerable<T>(DbCommandInterceptContext<IEnumerable<T>> context)
        {
        }

        /// <summary>
        /// 执行 ExecuteEnumerableAsync 方法之后。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task OnAfterExecuteEnumerableAsync<T>(DbCommandInterceptContext<IEnumerable<T>> context, CancellationToken cancellationToken = default)
        {
            return TaskCompatible.CompletedTask;
        }
    }
}
