// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Provider;
using Fireasy.Data.RecordWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// 提供拦截器的 <see cref="IDatabase"/> 实现类。
    /// </summary>
    public class InterceptedDatabase : Database
    {
        private DbCommandInterceptor _interceptor;
        private bool _isInitialized;

        /// <summary>
        /// 初始化 <see cref="InterceptedDatabase"/> 类的新实例。
        /// </summary>
        /// <param name="provider">数据库提供者。</param>
        protected InterceptedDatabase(IProvider provider)
            : base (provider)
        {
        }

        /// <summary>
        /// 初始化 <see cref="InterceptedDatabase"/> 类的新实例。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <param name="provider">数据库提供者。</param>
        public InterceptedDatabase(ConnectionString connectionString, IProvider provider)
            : base(connectionString, provider)
        {
        }

        /// <summary>
        /// 初始化 <see cref="InterceptedDatabase"/> 类的新实例。
        /// </summary>
        /// <param name="connectionStrings">数据库连接字符串组。</param>
        /// <param name="provider">数据库提供者。</param>
        public InterceptedDatabase(List<DistributedConnectionString> connectionStrings, IProvider provider)
            : base(connectionStrings, provider)
        {
        }

        /// <summary>
        /// 执行查询文本并将结果以一个 <see cref="IEnumerable{T}"/> 的序列返回。
        /// </summary>
        /// <typeparam name="T">查询对象类型。</typeparam>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="rowMapper">数据行映射器。</param>
        /// <returns>一个 <typeparamref name="T"/> 类型的对象的枚举器。</returns>
        public override IEnumerable<T> ExecuteEnumerable<T>(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, IDataRowMapper<T> rowMapper = null)
        {
            var interceptor = GetInterceptor();
            if (interceptor == null)
            {
                return base.ExecuteEnumerable(queryCommand, segment, parameters, rowMapper);
            }

            var ic = new DbCommandInterceptContext<IEnumerable<T>>(this, queryCommand, segment, parameters);
            interceptor.OnBeforeExecuteEnumerable(ic);
            if (ic.Skip)
            {
                return ic.Result;
            }

            ic.Result = base.ExecuteEnumerable(queryCommand, segment, parameters, rowMapper);

            interceptor.OnAfterExecuteEnumerable(ic);

            return ic.Result;
        }

        /// <summary>
        /// 异步的，执行查询文本并将结果以一个 <see cref="IEnumerable{T}"/> 的序列返回。
        /// </summary>
        /// <typeparam name="T">查询对象类型。</typeparam>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="rowMapper">数据映射函数。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>一个 <typeparamref name="T"/> 类型的对象的枚举器。</returns>
        public override async Task<IEnumerable<T>> ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, Func<IRecordWrapper, IDataReader, T> rowMapper, IDataSegment segment = null, ParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            var interceptor = GetInterceptor();
            if (interceptor == null)
            {
                return await base.ExecuteEnumerableAsync(queryCommand, rowMapper, segment, parameters, cancellationToken);
            }

            var ic = new DbCommandInterceptContext<IEnumerable<T>>(this, queryCommand, segment, parameters);
            await interceptor.OnBeforeExecuteEnumerableAsync(ic);
            if (ic.Skip)
            {
                return ic.Result;
            }

            ic.Result = await base.ExecuteEnumerableAsync(queryCommand, rowMapper, segment, parameters, cancellationToken);

            await interceptor.OnAfterExecuteEnumerableAsync(ic);

            return ic.Result;
        }

        /// <summary>
        /// 执行查询文本并将结果以一个 <see cref="IEnumerable{T}"/> 的序列返回。
        /// </summary>
        /// <typeparam name="T">查询对象类型。</typeparam>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="rowMapper">数据映射函数。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>一个 <typeparamref name="T"/> 类型的对象的枚举器。</returns>
        public override IEnumerable<T> ExecuteEnumerable<T>(IQueryCommand queryCommand, Func<IRecordWrapper, IDataReader, T> rowMapper, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            var interceptor = GetInterceptor();
            if (interceptor == null)
            {
                return base.ExecuteEnumerable(queryCommand, rowMapper, segment, parameters);
            }

            var ic = new DbCommandInterceptContext<IEnumerable<T>>(this, queryCommand, segment, parameters);
            interceptor.OnBeforeExecuteEnumerable(ic);
            if (ic.Skip)
            {
                return ic.Result;
            }

            ic.Result = base.ExecuteEnumerable(queryCommand, rowMapper, segment, parameters);

            interceptor.OnAfterExecuteEnumerable(ic);

            return ic.Result;
        }

        /// <summary>
        /// 异步的，执行查询文本并将结果以一个 <see cref="IEnumerable{T}"/> 的序列返回。
        /// </summary>
        /// <typeparam name="T">查询对象类型。</typeparam>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="rowMapper">数据行映射器。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>一个 <typeparamref name="T"/> 类型的对象的枚举器。</returns>
        public override async Task<IEnumerable<T>> ExecuteEnumerableAsync<T>(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, IDataRowMapper<T> rowMapper = null, CancellationToken cancellationToken = default)
        {
            var interceptor = GetInterceptor();
            if (interceptor == null)
            {
                return await base.ExecuteEnumerableAsync(queryCommand, segment, parameters, rowMapper, cancellationToken);
            }

            var ic = new DbCommandInterceptContext<IEnumerable<T>>(this, queryCommand, segment, parameters);
            await interceptor.OnBeforeExecuteEnumerableAsync(ic);
            if (ic.Skip)
            {
                return ic.Result;
            }

            ic.Result = await base.ExecuteEnumerableAsync(queryCommand, segment, parameters, rowMapper, cancellationToken);

            await interceptor.OnAfterExecuteEnumerableAsync(ic);

            return ic.Result;
        }

        /// <summary>
        /// 根据自定义的SQL语句查询返回一组动态对象。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>一个动态对象的枚举器。</returns>
        public override IEnumerable<dynamic> ExecuteEnumerable(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            var interceptor = GetInterceptor();
            if (interceptor == null)
            {
                return base.ExecuteEnumerable(queryCommand, segment, parameters);
            }

            var ic = new DbCommandInterceptContext<IEnumerable<dynamic>>(this, queryCommand, segment, parameters);
            interceptor.OnBeforeExecuteEnumerable(ic);
            if (ic.Skip)
            {
                return ic.Result;
            }

            ic.Result = base.ExecuteEnumerable(queryCommand, segment, parameters);

            interceptor.OnAfterExecuteEnumerable(ic);

            return ic.Result;
        }

        /// <summary>
        /// 异步的，执行查询文本并将结果并返回动态序列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>一个动态对象的枚举器。</returns>
        public override async Task<IEnumerable<dynamic>> ExecuteEnumerableAsync(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            var interceptor = GetInterceptor();
            if (interceptor == null)
            {
                return await base.ExecuteEnumerableAsync(queryCommand, segment, parameters, cancellationToken);
            }

            var ic = new DbCommandInterceptContext<IEnumerable<dynamic>>(this, queryCommand, segment, parameters);
            await interceptor.OnBeforeExecuteEnumerableAsync(ic);
            if (ic.Skip)
            {
                return ic.Result;
            }

            ic.Result = await base.ExecuteEnumerableAsync(queryCommand, segment, parameters, cancellationToken);

            await interceptor.OnAfterExecuteEnumerableAsync(ic);

            return ic.Result;
        }

        /// <summary>
        /// 执行查询文本，返回受影响的记录数。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>所影响的记录数。</returns>
        public override int ExecuteNonQuery(IQueryCommand queryCommand, ParameterCollection parameters = null)
        {
            var interceptor = GetInterceptor();
            if (interceptor == null)
            {
                return base.ExecuteNonQuery(queryCommand, parameters);
            }

            var ic = new DbCommandInterceptContext<int>(this, queryCommand, null, parameters);
            interceptor.OnBeforeExecuteNonQuery(ic);
            if (ic.Skip)
            {
                return ic.Result;
            }

            ic.Result = base.ExecuteNonQuery(queryCommand, parameters);

            interceptor.OnAfterExecuteNonQuery(ic);

            return ic.Result;
        }

        /// <summary>
        /// 异步的，执行查询文本，返回受影响的记录数。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>所影响的记录数。</returns>
        public override async Task<int> ExecuteNonQueryAsync(IQueryCommand queryCommand, ParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            var interceptor = GetInterceptor();
            if (interceptor == null)
            {
                return await base.ExecuteNonQueryAsync(queryCommand, parameters, cancellationToken);
            }

            var ic = new DbCommandInterceptContext<int>(this, queryCommand, null, parameters);
            await interceptor.OnBeforeExecuteNonQueryAsync(ic);
            if (ic.Skip)
            {
                return ic.Result;
            }

            ic.Result = await base.ExecuteNonQueryAsync(queryCommand, parameters, cancellationToken);

            await interceptor.OnAfterExecuteNonQueryAsync(ic);

            return ic.Result;
        }

        /// <summary>
        /// 执行查询文本，并返回第一行的第一列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>第一行的第一列数据。</returns>
        public override object ExecuteScalar(IQueryCommand queryCommand, ParameterCollection parameters = null)
        {
            var interceptor = GetInterceptor();
            if (interceptor == null)
            {
                return base.ExecuteScalar(queryCommand, parameters);
            }

            var ic = new DbCommandInterceptContext<object>(this, queryCommand, null, parameters);
            interceptor.OnBeforeExecuteScalar(ic);
            if (ic.Skip)
            {
                return ic.Result;
            }

            ic.Result = base.ExecuteScalar(queryCommand, parameters);

            interceptor.OnAfterExecuteScalar(ic);

            return ic.Result;
        }

        /// <summary>
        /// 异步的，执行查询文本，并返回第一行的第一列。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>第一行的第一列数据。</returns>
        public override async Task<object> ExecuteScalarAsync(IQueryCommand queryCommand, ParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            var interceptor = GetInterceptor();
            if (interceptor == null)
            {
                return await base.ExecuteScalarAsync(queryCommand, parameters, cancellationToken);
            }

            var ic = new DbCommandInterceptContext<object>(this, queryCommand, null, parameters);
            await interceptor.OnBeforeExecuteScalarAsync(ic);
            if (ic.Skip)
            {
                return ic.Result;
            }

            ic.Result = await base.ExecuteScalarAsync(queryCommand, parameters, cancellationToken);

            await interceptor.OnAfterExecuteScalarAsync(ic);

            return ic.Result;
        }

        private DbCommandInterceptor GetInterceptor()
        {
            if (!_isInitialized)
            {
                _interceptor = ServiceProvider?.TryGetService<DbCommandInterceptor>();
                _isInitialized = true;
            }

            return _interceptor;
        }
    }
}
