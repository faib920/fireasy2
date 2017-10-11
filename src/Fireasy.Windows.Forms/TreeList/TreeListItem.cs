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
    [DefaultProperty("Text")]
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class TreeListItem : Component, IVirtualItem, IImageDefinition
    {
        private TreeListItemCollection items;
        private TreeListCellCollection cells;
        private bool selected;
        private bool @checked;
        private bool expanded;
        private bool showBox = true;

        private Color foreColor;
        private Color backgroundColor;
        private Font font;
        private bool enabled = true;
        private int imageIndex = -1;
        private string imageKey;
        private Image image;

        public TreeListItem()
        {
            ImageIndex = -1;
            Enabled = true;
        }

        public TreeListItem(object value)
            : this()
        {
            if (Cells.Count == 0)
            {
                Cells.Add(new TreeListCell() { Value = value });
            }
            else
            {
                Cells[0].Value = value;
            }
        }

        /// <summary>
        /// 获取或设置附加的数据对象。
        /// </summary>
        [Description("获取或设置附加的数据对象。")]
        [DefaultValue((string)null)]
        public object Tag { get; set; }

        [Browsable(false)]
        public object KeyValue { get; set; }

        /// <summary>
        /// 获取或设置显示的文本。
        /// </summary>
        [Description("获取或设置显示的文本。")]
        [DefaultValue((string)null)]
        [Browsable(false)]
        public string Text
        {
            get
            {
                return Cells.Count > 0 ? Cells[0].Value.ToString() : string.Empty;
            }
            set
            {
                if (Cells.Count > 0)
                {
                    Cells[0].Value = value;
                }
            }
        }

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
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 获取或设宽度是否固定（不可调整）。
        /// </summary>
        [Description("获取或设宽度是否固定（不可调整）。")]
        [DefaultValue(false)]
        public bool Fixed { get; set; }

        /// <summary>
        /// 获取或设是否可用。
        /// </summary>
        [Description("获取或设是否可用。")]
        [DefaultValue(true)]
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    InvalidateItem();
                }
            }
        }

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
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 获取或设置显示的背景颜色。
        /// </summary>
        [Description("获取或设置显示的背景颜色。")]
        [DefaultValue(typeof(Color), "Empty")]
        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (backgroundColor != value)
                {
                    backgroundColor = value;
                    InvalidateItem();
                }
            }
        }

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
                    InvalidateItem();
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
                    InvalidateItem();
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
                    InvalidateItem();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否显示按钮。
        /// </summary>
        [Description("获取或设置是否显示按钮。")]
        [DefaultValue(true)]
        public bool ShowBox
        {
            get { return showBox; }
            set
            {
                if (showBox != value)
                {
                    showBox = value;
                    InvalidateItem();
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

        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        [DefaultValue((string)null)]
        [Localizable(true)]
        public TreeListItemCollection Items
        {
            get { return items ?? (items = new TreeListItemCollection(TreeList, this, Level + 1)); }
        }

        [Browsable(false)]
        public TreeListCellCollection Cells
        {
            get { return cells ?? (cells = new TreeListCellCollection(this)); }
        }

        /// <summary>
        /// 获取或设置是否选中。
        /// </summary>
        [Browsable(false)]
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected == value)
                {
                    return;
                }

                selected = value;
                if (TreeList != null)
                {
                    TreeList.SelectItem(this, value, !TreeList.MultiSelect);
                }
            }
        }

        public bool Checked
        {
            get { return @checked; }
            set
            {
                if (@checked == value)
                {
                    return;
                }

                @checked = value;
                if (TreeList != null)
                {
                    TreeList.CheckItem(this, value);
                }
            }
        }

        [Browsable(false)]
        public bool Mixed { get; internal set; }

        /// <summary>
        /// 获取或设置节点是否展开。
        /// </summary>
        [Browsable(false)]
        public bool Expended
        {
            get { return expanded; }
            set
            {
                if (expanded != value)
                {
                    expanded = value;
                    if (TreeList != null)
                    {
                        if (value && ShowExpanded && !IsDemandLoad)
                        {
                            TreeList.RaiseDamanLoadEvent(this);
                            IsDemandLoad = true;
                        }

                        TreeList.ProcessItemExpand(this);
                    }
                }
            }
        }

        /// <summary>
        /// 获取或设置是否显示节点 + 图标。
        /// </summary>
        [DefaultValue(false)]
        [Description("获取或设置是否显示节点 + 图标。")]
        public bool ShowExpanded { get; set; }

        /// <summary>
        /// 获取项的深度，从 0 开始。
        /// </summary>
        [Browsable(false)]
        public int Level { get; internal set; }

        /// <summary>
        /// 获取或设置分组名称。
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 获取或设置所属的 <see cref="TreeList"/> 容器。
        /// </summary>
        [Browsable(false)]
        public TreeList TreeList { get; private set; }

        public TreeListItem Parent { get; internal set; }

        internal TreeListItemCollection Owner { get; set; }

        /// <summary>
        /// 获取或设置是否第一次加载。
        /// </summary>
        internal bool IsDemandLoad { get; set; }

        internal int DataIndex { get; set; }

        [Browsable(false)]
        public int Index { get; set; }

        public void Bind(object dataItem)
        {

        }

        public override string ToString()
        {
            return Text;
        }

        public void EnsureVisible()
        {
            if (TreeList != null)
            {
                TreeList.EnsureVisible(this);
            }
        }

        internal void SetSelected(bool selected)
        {
            this.selected = selected;
        }

        internal void SetChecked(bool @checked)
        {
            this.@checked = @checked;
        }

        internal void InvalidateItem()
        {
            if (TreeList != null)
            {
                TreeList.InvalidateItem(this);
            }
        }

        internal virtual void Update(TreeList treelist, TreeListItem parent, int level)
        {
            if (TreeList == null)
            {
                TreeList = treelist;
                Parent = parent;

                for (var i = Cells.Count; i < TreeList.Columns.Count; i++)
                {
                    Cells.Add(new TreeListCell());
                }

                for (var i = 0; i < TreeList.Columns.Count; i++)
                {
                    Cells[i].Update(this, i);
                }

                for (var i = 0; i < Items.Count; i++)
                {
                    Items[i].Update(treelist, this, level + 1);
                }
            }
        }
    }
}
