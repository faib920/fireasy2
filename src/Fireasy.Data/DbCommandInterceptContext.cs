// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Fireasy.Data
{
    /// <summary>
    /// 命令拦截器的上下文。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbCommandInterceptContext<T>
    {
        /// <summary>
        /// 初始化 <see cref="DbCommandInterceptContext{T}"/> 类的新实例。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="queryCommand"></param>
        /// <param name="segment"></param>
        /// <param name="parameters"></param>
        public DbCommandInterceptContext(IDatabase database, IQueryCommand queryCommand, IDataSegment segment, ParameterCollection parameters)
        {
            ReturnType = typeof(T);
            Database = database;
            QueryCommand = queryCommand;
            Segment = segment;
            Parameters = parameters;
        }

        /// <summary>
        /// 获取返回类型。
        /// </summary>
        public Type ReturnType { get; }

        /// <summary>
        /// 获取当前的 <see cref="IDatabase"/> 实例。
        /// </summary>
        public IDatabase Database { get; }

        /// <summary>
        /// 获取当前的 <see cref="IQueryCommand"/> 实例。
        /// </summary>
        public IQueryCommand QueryCommand { get; }

        /// <summary>
        /// 获取当前的 <see cref="IDataSegment"/> 实例。
        /// </summary>
        public IDataSegment Segment { get; }

        /// <summary>
        /// 获取当前的 <see cref="ParameterCollection"/> 实例。
        /// </summary>
        public ParameterCollection Parameters { get; }

        /// <summary>
        /// 获取或设置返回值。
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// 获取或设置是否拦截以便跳过后续的执行。
        /// </summary>
        public bool Skip { get; set; }
    }
}
