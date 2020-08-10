// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 表示控件上的背景图像可以指定对齐方式。
    /// </summary>
    public interface IBackgroundAligning
    {
        /// <summary>
        /// 获取或设置背景图像的对齐方式。
        /// </summary>
        ContentAlignment BackgroundImageAligment { get; set; }
    }
}
