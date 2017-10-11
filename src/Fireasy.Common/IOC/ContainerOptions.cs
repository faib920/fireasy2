// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Ioc
{
    /// <summary>
    /// 往容器里注册类型时所的选项。
    /// </summary>
    public sealed class ContainerOptions
    {
        /// <summary>
        /// 初始化 <see cref="ContainerOptions"/> 类的新实例。
        /// </summary>
        public ContainerOptions()
        {
            AllowOverriding = true;
        }

        /// <summary>
        /// 获取或设置再次注册已经存在的类型时，是否允许注册新的 <see cref="IRegistration"/> 进行覆盖。默认为 true。
        /// </summary>
        public bool AllowOverriding { get; set; }
    }
}
