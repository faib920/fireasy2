// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 一个支持 <see cref="Decimal"/> 类型输入的编辑器。
    /// </summary>
    public class TreeListNumericEditor : TreeListTextEditor
    {
        /// <summary>
        /// 初始化 <see cref="TreeListNumericEditor"/> 类的新实例。
        /// </summary>
        public TreeListNumericEditor()
            : base(new NumericTextBox())
        {
        }

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(object value)
        {
            ((NumericTextBox)Inner).Decimal = value == null ? 0 : Convert.ToDecimal(value);
        }

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return ((NumericTextBox)Inner).Decimal;
        }

        /// <summary>
        /// 获取值是否有效。
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return ((NumericTextBox)Inner).IsValid();
        }

        /// <summary>
        /// 获取或设置 <see cref="Decimal"/> 允许的最大值。
        /// </summary>
        public decimal Max
        {
            get { return (decimal)((NumericTextBox)Inner).RangeMax; }
            set { ((NumericTextBox)Inner).RangeMax = (double)value; }
        }

        /// <summary>
        /// 获取或设置 <see cref="Decimal"/> 允许的最小值。
        /// </summary>
        public decimal Min
        {
            get { return (decimal)((NumericTextBox)Inner).RangeMin; }
            set { ((NumericTextBox)Inner).RangeMin = (double)value; }
        }
    }
}
