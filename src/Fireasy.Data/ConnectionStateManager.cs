// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// <see cref="DbConnection"/> 对象的状态管理器。
    /// </summary>
    public sealed class ConnectionStateManager
    {
        private readonly DbConnection _connection;
        private ConnectionState _state;

        /// <summary>
        /// 初始化 <see cref="ConnectionStateManager"/> 类的新实例。
        /// </summary>
        /// <param name="connection">一个 <see cref="DbConnection"/> 实例。</param>
        public ConnectionStateManager(DbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// 尝试打开连接。
        /// </summary>
        /// <returns></returns>
        public ConnectionStateManager TryOpen()
        {
            _state = _connection.State;
            if (_state != ConnectionState.Open)
            {
                _connection.Open();
            }

            return this;
        }

        /// <summary>
        /// 异常的，尝试打开链接。
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ConnectionStateManager> TryOpenAsync(CancellationToken cancellationToken = default)
        {
            _state = _connection.State;
            if (_state != ConnectionState.Open)
            {
                await _connection.OpenAsync(cancellationToken);
            }

            return this;
        }

        /// <summary>
        /// 尝试关闭链接。
        /// </summary>
        /// <returns></returns>
        public ConnectionStateManager TryClose()
        {
            if (_state == ConnectionState.Closed)
            {
                _connection.Close();
            }

            return this;
        }

        /// <summary>
        /// 异步的，尝试关闭链接。
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ConnectionStateManager> TryCloseAsync(CancellationToken cancellationToken = default)
        {
            if (_state == ConnectionState.Closed)
            {
#if NETSTANDARD2_1_OR_GREATER
                await _connection.CloseAsync();
#else
                _connection.Close();
#endif
            }

            return this;
        }
    }
}
