// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 提供给单元格的编辑器方法。
    /// </summary>
    public interface ITreeListEditor
    {
        /// <summary>
        /// 获取或设置编辑器的控制器。
        /// </summary>
        TreeListEditController Controller { get; set; }

        /// <summary>
        /// 将编辑器显示在指定的范围内。
        /// </summary>
        /// <param name="rect">编辑器放置的范围。</param>
        void Show(Rectangle rect);

        /// <summary>
        /// 隐藏编辑器。
        /// </summary>
        void Hide();

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        void SetValue(object value);

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        object GetValue();

        /// <summary>
        /// 获取值是否有效。
        /// </summary>
        /// <returns></returns>
        bool IsValid();
    }
}
