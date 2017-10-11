using System;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    [DefaultProperty("Text")]
    public class TreeListColumn : Component, IImageDefinition, IDataFormattable, ICloneable
    {
        private ITreeListEditor editor = new TreeListTextEditor();
        private TreeListCellDataType dbType;
        private int width = 100;
        private bool hidden;
        private Color foreColor = Color.Empty;
        private Font font;
        private int imageIndex = -1;
        private string imageKey;
        private Image image;
        private string text;
        private HorizontalAlignment textAlign;
        private HorizontalAlignment imageAlign;
        private string dataFormat;
        private string dataKey;

        /// <summary>
        /// 初始化 <see cref="TreeListColumn"/> 类的新实例。
        /// </summary>
        public TreeListColumn()
        {
            Sortable = true;
        }

        /// <summary>
        /// 获取或设置列的宽度。
        /// </summary>
        [Description("获取或设置列的宽度。")]
        [DefaultValue(100)]
        public int Width
        {
            get { return width; }
            set
            {
                if (width != value)
                {
                    width = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否自动填满剩余空间。
        /// </summary>
        [Description("获取或设置是否自动填满剩余空间。")]
        [DefaultValue(false)]
        public bool Spring { get; set; }

        /// <summary>
        /// 指定列是否隐藏。
        /// </summary>
        [Description("指定列是否隐藏。")]
        [DefaultValue(false)]
        public bool Hidden
        {
            get { return hidden; }
            set
            {
                if (hidden != value)
                {
                    hidden = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置显示的文本。
        /// </summary>
        [Description("获取或设置显示的文本。")]
        [DefaultValue((string)null)]
        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    InvalidateColumn();
                }
            }
        }

        /// <summary>
        /// 获取或设置用于绑定数据的键值，比如对象的属性名称。
        /// </summary>
        [Description("获取或设置用于绑定数据的键值，比如对象的属性名称。")]
        [DefaultValue((string)null)]
        public string DataKey
        {
            get { return dataKey; }
            set
            {
                if (dataKey != value)
                {
                    dataKey = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置数据的输出格式。
        /// </summary>
        [Description("获取或设置数据的输出格式。")]
        [DefaultValue((string)null)]
        [Editor(typeof(DataFormatEditor), typeof(UITypeEditor))]
        public string DataFormat
        {
            get { return dataFormat; }
            set
            {
                if (dataFormat != value)
                {
                    dataFormat = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置格式化器。
        /// </summary>
        [Browsable(false)]
        public Func<object, string> Formatter { get; set; }

        /// <summary>
        /// 获取或设置验证器。
        /// </summary>
        [Browsable(false)]
        public Func<object, bool> Validator { get; set; }

        /// <summary>
        /// 获取或设置附加的数据对象。
        /// </summary>
        [Description("获取或设置附加的数据对象。")]
        [DefaultValue((string)null)]
        [TypeConverter(typeof(StringConverter))]
        public object Tag { get; set; }

        /// <summary>
        /// 获取或设置显示的字体。
        /// </summary>
        [Description("获取或设置显示的字体。")]
        [DefaultValue(null)]
        public Font Font
        {
            get { return font; }
            set
            {
                if (font != value)
                {
                    font = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置单元格上显示的字体。
        /// </summary>
        [Description("获取或设置单元格上显示的字体。")]
        [DefaultValue(null)]
        public Font CellFont { get; set; }

        /// <summary>
        /// 获取或设宽度是否固定（不可调整）。
        /// </summary>
        [Description("获取或设置宽度是否固定（不可调整）。")]
        [DefaultValue(false)]
        public bool Fixed { get; set; }

        /// <summary>
        /// 获取或设置是否冻结在左边。
        /// </summary>
        [Description("获取或设置是否冻结在左边。")]
        [DefaultValue(false)]
        public bool Frozen { get; set; }

        /// <summary>
        /// 获取或设置文本对齐方式。
        /// </summary>
        [Description("获取或设置文本对齐方式。")]
        [DefaultValue(typeof(HorizontalAlignment), "Left")]
        public HorizontalAlignment TextAlign
        {
            get { return textAlign; }
            set
            {
                if (textAlign != value)
                {
                    textAlign = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置图像对齐方式。
        /// </summary>
        [Description("获取或设置图像对齐方式。")]
        [DefaultValue(typeof(HorizontalAlignment), "Left")]
        public HorizontalAlignment ImageAlign
        {
            get { return imageAlign; }
            set
            {
                if (imageAlign != value)
                {
                    imageAlign = value;
                    InvalidateColumn();
                }
            }
        }

        /// <summary>
        /// 获取或设是否可以排序。
        /// </summary>
        [Description("获取或设是否可以排序。")]
        [DefaultValue(true)]
        public bool Sortable { get; set; }

        /// <summary>
        /// 获取或设置显示的前景颜色。
        /// </summary>
        [Description("获取或设置显示的前景颜色。")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color ForeColor
        {
            get { return foreColor; }
            set
            {
                if (foreColor != value)
                {
                    foreColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置在单元格上显示的前景颜色。
        /// </summary>
        [Description("获取或设置在单元格上显示的前景颜色。")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color CellForeColor { get; set; }

        /// <summary>
        /// 获取或设置为该项显示的图像的索引。
        /// </summary>
        [DefaultValue(-1)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design", typeof(UITypeEditor))]
        [TypeConverter(typeof(ImageIndexConverter))]
        [Description("获取或设置为该项显示的图像的索引。")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public int ImageIndex
        {
            get { return imageIndex; }
            set
            {
                if (imageIndex != value)
                {
                    imageIndex = value;
                    InvalidateColumn();
                }
            }
        }

        /// <summary>
        /// 获取或设置为该项显示的图像的键。
        /// </summary>
        [DefaultValue((string)null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design", typeof(UITypeEditor))]
        [TypeConverter(typeof(ImageKeyConverter))]
        [Description("获取或设置为该项显示的图像的键。")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public string ImageKey
        {
            get { return imageKey; }
            set
            {
                if (imageKey != value)
                {
                    imageKey = value;
                    InvalidateColumn();
                }
            }
        }

        /// <summary>
        /// 获取或设置显示的图像。
        /// </summary>
        [Description("获取或设置显示的图像。")]
        public Image Image
        {
            get { return image; }
            set
            {
                if (image != value)
                {
                    image = value;
                    InvalidateColumn();
                }
            }
        }

        /// <summary>
        /// 获取用于显示图标的 <see cref="ImageList"/> 控件。
        /// </summary>
        [Browsable(false)]
        public ImageList ImageList
        {
            get { return TreeList.ImageList; }
        }

        /// <summary>
        /// 获取此列的索引。
        /// </summary>
        [Browsable(false)]
        public int Index { get; internal set; }

        /// <summary>
        /// 获取或设置此列是否允许编辑。
        /// </summary>
        [DefaultValue(false)]
        [Description("指定此列是否允许编辑。")]
        public bool Editable { get; set; }

        /// <summary>
        /// 获取或设置此列中单元格内容的数据类型。
        /// </summary>
        [DefaultValue(typeof(TreeListCellDataType), "String")]
        public TreeListCellDataType DataType
        {
            get { return dbType; }
            set
            {
                if (dbType != value)
                {
                    dbType = value;
                    editor = TreeListEditorFactory.Create(dbType);
                }
            }
        }

        /// <summary>
        /// 获取或设置所属的 <see cref="TreeList"/> 容器。
        /// </summary>
        [Browsable(false)]
        public TreeList TreeList { get; private set; }

        /// <summary>
        /// 获取编辑器对象。
        /// </summary>
        [Browsable(false)]
        public ITreeListEditor Editor
        {
            get { return editor; }
        }

        /// <summary>
        /// 设置编辑器对象。
        /// </summary>
        /// <param name="editor">用于编辑此列的编辑器。</param>
        public void SetEditor(ITreeListEditor editor)
        {
            this.editor = editor;
        }

        internal void Update(TreeList treelist)
        {
            if (TreeList == null)
            {
                TreeList = treelist;
            }
        }

        internal void SetWidth(int width)
        {
            this.width = width;
        }

        internal void InvalidateColumn()
        {
            if (TreeList != null)
            {
                TreeList.InvalidateColumn(this);
            }
        }

        internal void Invalidate()
        {
            if (TreeList != null)
            {
                TreeList.Invalidate();
            }
        }

        public object Clone()
        {
            return (TreeListColumn)this.MemberwiseClone();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (TreeList != null))
            {
                int index = this.Index;
                if (index != -1)
                {
                    TreeList.Columns.RemoveAt(index);
                }
            }

            base.Dispose(disposing);
        }
    }
}
