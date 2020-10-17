// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common
{
    /// <summary>
    /// 用于循环中对第一轮进行处理。无法继承此类。
    /// </summary>
    public sealed class AssertFlag
    {
        private bool _flag;
        private readonly bool _initFlag; //初始的状态

        /// <summary>
        /// 使用初始的标志位初始化 <see cref="AssertFlag"/> 类的新实例。
        /// </summary>
        public AssertFlag()
        {
            _flag = _initFlag = true;
        }

        /// <summary>
        /// 判断当前的标志位是否已初始的值相等。循环中的第一轮始终返回 true，从第二轮开始，始终返回 false。
        /// </summary>
        /// <returns>相等则为 true，否则为 false。</returns>
        public bool AssertTrue()
        {
            if (_flag == _initFlag)
            {
                _flag = !_flag;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 重置标志位为初始状态。
        /// </summary>
        public void Reset()
        {
            _flag = _initFlag;
        }
    }
}
