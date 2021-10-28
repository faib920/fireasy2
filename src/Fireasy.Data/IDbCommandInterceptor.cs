// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// 定义用于拦截命令的执行的方法。
    /// </summary>
    public interface IDbCommandInterceptor
    {
        /// <summary>
        /// 执行 ExecuteNonQuery 方法之前。
        /// </summary>
        /// <param name="context"></param>
        void OnBeforeExecuteNonQuery(DbCommandInterceptContext<int> context);

        /// <summary>
        /// 执行 ExecuteNonQueryAsync 方法之前。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnBeforeExecuteNonQueryAsync(DbCommandInterceptContext<int> context, CancellationToken cancellationToken);

        /// <summary>
        /// 执行 ExecuteNonQuery 方法之后。
        /// </summary>
        /// <param name="context"></param>
        void OnAfterExecuteNonQuery(DbCommandInterceptContext<int> context);

        /// <summary>
        /// 执行 ExecuteNonQueryAsync 方法之后。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnAfterExecuteNonQueryAsync(DbCommandInterceptContext<int> context, CancellationToken cancellationToken);

        /// <summary>
        /// 执行 ExecuteScalar 方法之前。
        /// </summary>
        /// <param name="context"></param>
        void OnBeforeExecuteScalar(DbCommandInterceptContext<object> context);

        /// <summary>
        /// 执行 ExecuteScalarAsync 方法之前。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnBeforeExecuteScalarAsync(DbCommandInterceptContext<object> context, CancellationToken cancellationToken);

        /// <summary>
        /// 执行 ExecuteScalar 方法之后。
        /// </summary>
        /// <param name="context"></param>
        void OnAfterExecuteScalar(DbCommandInterceptContext<object> context);

        /// <summary>
        /// 执行 ExecuteScalarAsync 方法之后。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnAfterExecuteScalarAsync(DbCommandInterceptContext<object> context, CancellationToken cancellationToken);

        /// <summary>
        /// 执行 ExecuteEnumerable 方法之前。
        /// </summary>
        /// <param name="context"></param>
        void OnBeforeExecuteEnumerable(DbCommandInterceptContext<IEnumerable<dynamic>> context);

        /// <summary>
        /// 执行 ExecuteEnumerableAsync 方法之前。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnBeforeExecuteEnumerableAsync(DbCommandInterceptContext<IEnumerable<dynamic>> context, CancellationToken cancellationToken);

        /// <summary>
        /// 执行 ExecuteEnumerable 方法之后。
        /// </summary>
        /// <param name="context"></param>
        void OnAfterExecuteEnumerable(DbCommandInterceptContext<IEnumerable<dynamic>> context);

        /// <summary>
        /// 执行 ExecuteEnumerableAsync 方法之后。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnAfterExecuteEnumerableAsync(DbCommandInterceptContext<IEnumerable<dynamic>> context, CancellationToken cancellationToken);

        /// <summary>
        /// 执行 ExecuteEnumerable 方法之前。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        void OnBeforeExecuteEnumerable<T>(DbCommandInterceptContext<IEnumerable<T>> context);

        /// <summary>
        /// 执行 ExecuteEnumerableAsync 方法之前。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnBeforeExecuteEnumerableAsync<T>(DbCommandInterceptContext<IEnumerable<T>> context, CancellationToken cancellationToken);

        /// <summary>
        /// 执行 ExecuteEnumerable 方法之后。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        void OnAfterExecuteEnumerable<T>(DbCommandInterceptContext<IEnumerable<T>> context);

        /// <summary>
        /// 执行 ExecuteEnumerableAsync 方法之后。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnAfterExecuteEnumerableAsync<T>(DbCommandInterceptContext<IEnumerable<T>> context, CancellationToken cancellationToken);

        /// <summary>
        /// 执行 ExecuteDataReader 方法之前。
        /// </summary>
        /// <param name="context"></param>
        void OnBeforeExecuteReader(DbCommandInterceptContext<IDataReader> context);

        /// <summary>
        /// 执行 ExecuteDataReaderAsync 方法之前。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnBeforeExecuteReaderAsync(DbCommandInterceptContext<IDataReader> context, CancellationToken cancellationToken);

        /// <summary>
        /// 执行 ExecuteDataReader 方法之后。
        /// </summary>
        /// <param name="context"></param>
        void OnAfterExecuteReader(DbCommandInterceptContext<IDataReader> context);

        /// <summary>
        /// 执行 ExecuteDataReaderAsync 方法之后。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnAfterExecuteReaderAsync(DbCommandInterceptContext<IDataReader> context, CancellationToken cancellationToken);
    }
}