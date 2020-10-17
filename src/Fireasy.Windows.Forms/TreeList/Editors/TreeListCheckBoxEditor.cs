using System;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 一个用于勾选的编辑器。
    /// </summary>
    public class TreeListCheckBoxEditor : TreeListWrapEditor<CheckBox>
    {
        /// <summary>
        /// 初始化 <see cref="TreeListCheckBoxEditor"/> 类的新实例。
        /// </summary>
        public TreeListCheckBoxEditor()
            : base(new CheckBox() { AutoSize = true })
        {
        }

        /// <summary>
        /// 将编辑器显示在指定的范围内。
        /// </summary>
        /// <param name="rect">编辑器放置的范围。</param>
        public override void Show(Rectangle rect)
        {
            base.Show(rect);

            var checkbox = Inner as CheckBox;
            checkbox.Location = new Point((rect.Width - checkbox.Width) / 2, (rect.Height - checkbox.Height) / 2);
        }

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(object value)
        {
            (Inner as CheckBox).Checked = Convert.ToBoolean(value);
        }

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return (Inner as CheckBox).Checked;
        }
    }
}
