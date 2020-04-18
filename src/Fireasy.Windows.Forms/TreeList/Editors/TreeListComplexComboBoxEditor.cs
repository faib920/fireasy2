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
    public class TreeListComplexComboBoxEditor : TreeListWrapEditor<ComplexComboBox>
    {
        /// <summary>
        /// 初始化 <see cref="TreeListComplexComboBoxEditor"/> 类的新实例。
        /// </summary>
        /// <param name="combobox">指定真实的编辑器。</param>
        public TreeListComplexComboBoxEditor(ComplexComboBox combobox)
            : base(combobox)
        {
        }

        public TreeListComplexComboBoxEditor()
            : base(new ComplexComboBox())
        {
        }

        /// <summary>
        /// 将编辑器显示在指定的范围内。
        /// </summary>
        /// <param name="rect">编辑器放置的范围。</param>
        public override void Show(Rectangle rect)
        {
            base.Show(rect);

            var picker = (ComboBox)Inner;
            picker.Left = -1;

            picker.Width = Width + 2;
            picker.Top = -1;

            picker.Height = rect.Height;

            picker.DroppedDown = true;
            picker.Focus();
        }

        public override void Hide()
        {
            Inner.HideDropDown();
            base.Hide();
        }

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(object value)
        {
            var picker = Inner;
            picker.SelectedValue = value;
        }

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            var picker = Inner;
            return picker.SelectedValue;
        }

        /// <summary>
        /// 设置下拉控件的宽度。
        /// </summary>
        /// <param name="height">高度。</param>
        /// <returns></returns>
        public TreeListComplexComboBoxEditor SetDropDownHeight(int height)
        {
            Inner.DropDownHeight = height;
            return this;
        }

        /// <summary>
        /// 设置下拉控件的高度。
        /// </summary>
        /// <param name="width">宽度。</param>
        /// <returns></returns>
        public TreeListComplexComboBoxEditor SetDropDownWidth(int width)
        {
            Inner.DropDownWidth = width;
            return this;
        }

        /// <summary>
        /// 设置下拉控件的大小。
        /// </summary>
        /// <param name="width">宽度。</param>
        /// <param name="height">高度。</param>
        /// <returns></returns>
        public TreeListComplexComboBoxEditor SetDropDownSize(int width, int height)
        {
            Inner.DropDownWidth = width;
            Inner.DropDownHeight = height;
            return this;
        }
    }
}
