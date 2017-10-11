using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 一个支持 <see cref="Int32"/> 类型输入的编辑器。
    /// </summary>
    public class TreeListIntegerEditor : TreeListTextEditor
    {
        /// <summary>
        /// 初始化 <see cref="TreeListIntegerEditor"/> 类的新实例。
        /// </summary>
        public TreeListIntegerEditor()
            : base(new IntegerTextBox())
        {
        }

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(object value)
        {
            ((IntegerTextBox)Inner).Int = value == null ? 0 : Convert.ToInt32(value);
        }

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return ((IntegerTextBox)Inner).Int;
        }

        /// <summary>
        /// 获取值是否有效。
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return ((IntegerTextBox)Inner).IsValid();
        }

        /// <summary>
        /// 获取或设置 <see cref="Int32"/> 允许的最大值。
        /// </summary>
        public int Max
        {
            get { return (int)((IntegerTextBox)Inner).RangeMax; }
            set { ((IntegerTextBox)Inner).RangeMax = value; }
        }

        /// <summary>
        /// 获取或设置 <see cref="Int32"/> 允许的最小值。
        /// </summary>
        public int Min
        {
            get { return (int)((IntegerTextBox)Inner).RangeMin; }
            set { ((IntegerTextBox)Inner).RangeMin = value; }
        }
    }
}
