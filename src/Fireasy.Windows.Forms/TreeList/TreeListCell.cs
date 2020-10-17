﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 表示一个单元格。
    /// </summary>
    public class TreeListCell
    {
        private object _value;
        private string _nullText;
        private BoxType _boxType;
        private bool _checked;

        /// <summary>
        /// 初始化 <see cref="TreeListCell"/> 类的新实例。
        /// </summary>
        public TreeListCell()
        {
        }

        /// <summary>
        /// 使用一个值初始化 <see cref="TreeListCell"/> 类的新实例。
        /// </summary>
        /// <param name="value">单元格的值。</param>
        public TreeListCell(object value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        /// 获取或设置单元格的值。
        /// </summary>
        [DefaultValue(null)]
        public object Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;

                    ValidateCellValue();
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 获取或设置当属性 Value 为 null 时显示的文本。
        /// </summary>
        [DefaultValue((string)null)]
        public string NullText
        {
            get { return _nullText; }
            set
            {
                if (_nullText != value)
                {
                    _nullText = value;
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 获取或设置单元格中显示的控件。
        /// </summary>
        [DefaultValue(typeof(BoxType), "None")]
        public BoxType BoxType
        {
            get { return _boxType; }
            set
            {
                if (_boxType != value)
                {
                    _boxType = value;
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 获取或设置单元格是否已选中。
        /// </summary>
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 获取或设置单元格是否已验证。
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// 获取单元格对应的列。
        /// </summary>
        public TreeListColumn Column { get; private set; }

        /// <summary>
        /// 获取单元格所属的项。
        /// </summary>
        public TreeListItem Item { get; private set; }

        /// <summary>
        /// 获取或设置单元格的编辑类型。
        /// </summary>
        [DefaultValue(typeof(TreeListCellDataType), "String")]
        public TreeListCellDataType EditType { get; set; }

        /// <summary>
        /// 获取输出文本。
        /// </summary>
        [Browsable(false)]
        public string Text
        {
            get
            {
                return GetCellText();
            }
        }

        internal void Update(TreeListItem item, int index)
        {
            if (Item == null)
            {
                Item = item;
            }

            if (Column == null && item.TreeList != null)
            {
                if (index + 1 > item.TreeList.Columns.Count)
                {
                    throw new Exception("超出");
                }

                Column = item.TreeList.Columns[index];

                ValidateCellValue();
            }
        }

        /// <summary>
        /// 根据 Value 值获取单元格的文本。
        /// </summary>
        /// <returns></returns>
        private string GetCellText()
        {
            if (Value != null)
            {
                if (Column.Formatter != null)
                {
                    return Column.Formatter(Value);
                }

                //日期，如果小于1900年，输出空字符串
                if (Value is DateTime time && time.Year <= 1900)
                {
                    return string.Empty;
                }

                //枚举值说明文本
                if (Value is Enum @enum && Column.DataFormat == "[ed]")
                {
                    return @enum.GetDescription();
                }

                //使用IFormattable进行转换
                if (!string.IsNullOrEmpty(Column.DataFormat) && Value is IFormattable formater)
                {
                    return formater.ToString(Column.DataFormat, CultureInfo.CurrentCulture);
                }

                return Value.ToString();
            }

            return string.Empty;
        }

        private void InvalidateItem()
        {
            if (Item != null)
            {
                Item.InvalidateItem();
            }
        }

        private void ValidateCellValue()
        {
            if (Column != null && Column.Validator != null)
            {
                IsValid = Column.Validator(Value);
            }
            else
            {
                IsValid = true;
            }

            if (Item != null && Item.TreeList != null)
            {
                Item.TreeList.UpdateValidateFlags(this);
            }
        }
    }
}
