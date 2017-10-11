// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;

namespace Fireasy.Data
{
    /// <summary>
    /// 在当前线程范围内检索 <see cref="IDatabase"/> 对象。无法继承此类。
    /// </summary>
    public sealed class DatabaseScope : Scope<DatabaseScope>
    {
        private readonly IDatabase database;

        /// <summary>
        /// 初始化 <see cref="DatabaseScope"/> 类的新实例。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        internal DatabaseScope(IDatabase database)
            : base (false)
        {
            this.database = database;
        }

        /// <summary>
        /// 返回当前线程内的 <see cref="IDatabase"/> 对象。
        /// </summary>
        public IDatabase Database 
        {
            get
            {
                return new NoDisposeDatabase(database);
            }
        }

        /// <summary>
        /// 获取实例名称。
        /// </summary>
        public string InstanceName { get; internal set; }
    }
}
