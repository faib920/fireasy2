// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    [ToolboxBitmap(typeof(TreeList))]
    [ToolboxItem(true)]
    public partial class TreeList : Control, IBorderStylization, IBackgroundAligning
    {
        private TreeListColumnCollection _columns;
        private TreeListItemCollection _nodes;
        private TreeListGroupCollection _groups;
        private TreeListRenderer _renderer;
        private TreeListItem _footer;
        private TreeListHitTestInfo _lastHoverHitInfo;
        private readonly TreeListBound _bound;
        private TreeListSelectedItemCollection _selectedList = null;
        private TreeListCheckedItemCollection _checkedList = null;
        private VScrollBar _vbar;
        private HScrollBar _hbar;
        private Panel _psize;
        private readonly ToolTipWrapper _tip;
        private readonly TreeListEditController _editor;
        private int _sortVersion;
        private SortOrder _sortedOrder = SortOrder.None;
        private TreeListColumn _sortedColumn;
        private readonly VirtualItemManager _virMgr;
        private readonly List<TreeListCell> _invalidateCells = new List<TreeListCell>();

        //记录列头调整宽度时拖动线的x位置
        private int _dragSizePos = -1;
        private bool _isUpdating = false;

        //检测列头调整宽度是否超出右边框的距离
        private const int RIGHT_EDGE = 4;

        private int _itemHeight = 26;
        private int _headerHeight = 26;
        private int _footerHeight = 30;
        private int _rowNumberWidth = 30;
        private int _indent = 20;
        private bool _showHeader = true;
        private bool _showPlusMinusLines = true;
        private bool _showGridLines = true;
        private bool _showFooter;
        private bool _showPlusMinus;
        private bool _showCheckBoxes;
        private bool _showCheckAllBoxOnHeader;
        private bool _columnHorizontalCenter;
        private bool _useDefaultNodeImage;
        private Image _defaultImage;
        private Image _defaultExpandImage;
        private Image _defaultCollapseImage;
        private ImageList _imageList;
        private string _noneItemText = "没有可显示的数据";
        private Color _alternateBackColor = Color.Empty;
        private Color _groupForeColor = Color.DarkBlue;

        /// <summary>
        /// 初始化 <see cref="TreeList"/> 类的新实例。
        /// </summary>
        public TreeList()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            _bound = new TreeListBound(this);
            _tip = new ToolTipWrapper(this);
            _editor = new TreeListEditController(this);

            BackColor = SystemColors.Window;
            BorderStyle = BorderStyle.Fixed3D;
            KeepSelectedItems = true;
            Sortable = true;
            BackgroundImageAligment = System.Drawing.ContentAlignment.TopLeft;
            ShowLoading = true;
            LoadingText = "正在加载，请稍候...";
            GroupFont = new System.Drawing.Font("Consolas", 12);

            _virMgr = new VirtualItemManager(this);

            InitScrollBars();
        }

        #region 属性
        /// <summary>
        /// 获取或设置 <see cref="TreeList"/> 的呈现器。
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeListRenderer Renderer
        {
            get
            {
                return _renderer ?? (_renderer = ThemeManager.TreeListRenderer);
            }
            set
            {
                _renderer = value;
            }
        }

        /// <summary>
        /// 获取或设置背景颜色。
        /// </summary>
        [Category("Appearance")]
        [Description("获取或设置背景颜色。")]
        [DefaultValue(typeof(Color), "Window")]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        /// <summary>
        /// 获取或设置交替行的背景颜色。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Empty")]
        [Description("获取或设置交替行的背景颜色。")]
        public Color AlternateBackColor
        {
            get { return _alternateBackColor; }
            set
            {
                if (_alternateBackColor != value)
                {
                    _alternateBackColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置行的高度。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(26)]
        [Description("获取或设置行的高度。")]
        public int ItemHeight
        {
            get { return _itemHeight; }
            set
            {
                if (_itemHeight != value)
                {
                    _itemHeight = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置行号列的宽度
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(30)]
        [Description("获取或设置行号列的宽度。")]
        public int RowNumberWidth
        {
            get { return _rowNumberWidth; }
            set
            {
                if (_rowNumberWidth != value)
                {
                    _rowNumberWidth = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置行号初值。
        /// </summary>
        [Browsable(false)]
        public int RowNumberIndex { get; set; }

        /*
        /// <summary>
        /// 获取或设置组的高度。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(26)]
        [Description("获取或设置组的高度。")]
        public int GroupHeight
        {
            get { return groupHeight; }
            set
            {
                if (groupHeight != value)
                {
                    groupHeight = value;
                    Invalidate();
                }
            }
        }
         */

        /// <summary>
        /// 获取或设置列头的高度。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(26)]
        [Description("获取或设置列头的高度。")]
        public int HeaderHeight
        {
            get { return _headerHeight; }
            set
            {
                if (_headerHeight != value)
                {
                    _headerHeight = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置页尾的高度。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(30)]
        [Description("获取或设置页尾的高度。")]
        public int FooterHeight
        {
            get { return _footerHeight; }
            set
            {
                if (_footerHeight != value)
                {
                    _footerHeight = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置节点缩进的宽度。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(20)]
        [Description("获取或设置节点缩进的宽度。")]
        public int Indent
        {
            get { return _indent; }
            set
            {
                if (_indent != value)
                {
                    _indent = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 没有可显示的数据时显示的文本。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("没有可显示的数据时显示的文本")]
        public string NoneItemText
        {
            get { return _noneItemText; }
            set
            {
                if (_noneItemText != value)
                {
                    _noneItemText = value;
                    if (Items.Count == 0)
                    {
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 获取或设置正在加载时显示的文本。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("正在加载，请稍候...")]
        [Description("获取或设置正在加载时显示的文本。")]
        public string LoadingText { get; set; }

        /// <summary>
        /// 获取或设置是否显示加载的状态。
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("获取或设置是否显示加载的状态。")]
        public bool ShowLoading { get; set; }

        /// <summary>
        /// 获取或设置边框样式。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(typeof(BorderStyle), "Fixed3D")]
        [Description("获取或设置边框样式。")]
        public BorderStyle BorderStyle { get; set; }

        /// <summary>
        /// 获取或设置重新绑定数据源后是否记住之前的选择项。
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("获取或设置重新绑定数据源后是否记住之前的选择项。")]
        public bool KeepSelectedItems { get; set; }

        /// 获取或设置背景的对齐方式。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(typeof(System.Drawing.ContentAlignment), "TopLeft")]
        [Description("获取或设置背景的对齐方式。")]
        public System.Drawing.ContentAlignment BackgroundImageAligment { get; set; }

        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        [DefaultValue((string)null)]
        [Localizable(true)]
        public TreeListColumnCollection Columns
        {
            get
            {
                return _columns ?? (_columns = new TreeListColumnCollection(this));
            }
        }

        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        [DefaultValue((string)null)]
        [Localizable(true)]
        public TreeListGroupCollection Groups
        {
            get
            {
                return _groups ?? (_groups = new TreeListGroupCollection(this));
            }
        }

        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        [DefaultValue((string)null)]
        [Localizable(true)]
        public TreeListItemCollection Items
        {
            get
            {
                return _nodes ?? (_nodes = new TreeListItemCollection(this, null, 0));
            }
        }

        /// <summary>
        /// 获取或设置是否显示列头。
        /// </summary>
        [DefaultValue(true)]
        [Category("Appearance")]
        [Description("获取或设置是否显示列头。")]
        public bool ShowHeader
        {
            get { return _showHeader; }
            set
            {
                if (_showHeader != value)
                {
                    _showHeader = value;
                    _bound.Reset();
                    SetScrollBars();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否显示页脚统计部份。
        /// </summary>
        [DefaultValue(false)]
        [Category("Appearance")]
        [Description("获取或设置是否显示页脚统计部份。")]
        public bool ShowFooter
        {
            get { return _showFooter; }
            set
            {
                if (_showFooter != value)
                {
                    _showFooter = value;
                    _bound.Reset();
                    SetScrollBars();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否显示网格线。
        /// </summary>
        [DefaultValue(true)]
        [Category("Appearance")]
        [Description("获取或设置是否显示网格线。")]
        public bool ShowGridLines
        {
            get { return _showGridLines; }
            set
            {
                if (_showGridLines != value)
                {
                    _showGridLines = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否显示节点的展开/收缩按钮。
        /// </summary>
        [DefaultValue(false)]
        [Category("Appearance")]
        [Description("获取或设置是否显示节点的展开/收缩按钮。")]
        public bool ShowPlusMinus
        {
            get { return _showPlusMinus; }
            set
            {
                if (_showPlusMinus != value)
                {
                    _showPlusMinus = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否显示节点的展开/收缩线条。
        /// </summary>
        [DefaultValue(true)]
        [Category("Appearance")]
        [Description("获取或设置是否显示节点的展开/收缩线条。")]
        public bool ShowPlusMinusLines
        {
            get { return _showPlusMinusLines; }
            set
            {
                if (_showPlusMinusLines != value)
                {
                    _showPlusMinusLines = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否使用复选框。
        /// </summary>
        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("获取或设置是否使用复选框。")]
        public bool ShowCheckBoxes
        {
            get { return _showCheckBoxes; }
            set
            {
                if (_showCheckBoxes != value)
                {
                    _showCheckBoxes = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否在列头上显示全选复选框。
        /// </summary>
        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("获取或设置是否在列头上显示全选复选框。")]
        public bool ShowCheckAllBoxOnHeader
        {
            get { return _showCheckAllBoxOnHeader; }
            set
            {
                if (_showCheckAllBoxOnHeader != value)
                {
                    _showCheckAllBoxOnHeader = value;
                    Invalidate(_bound.ColumnBound);
                }
            }
        }

        /// <summary>
        /// 获取或设置列头是否始终水平居中对齐。
        /// </summary>
        [DefaultValue(false)]
        [Category("Appearance")]
        [Description("获取或设置列头是否始终水平居中对齐。")]
        public bool ColumnHorizontalCenter
        {
            get { return _columnHorizontalCenter; }
            set
            {
                if (_columnHorizontalCenter != value)
                {
                    _columnHorizontalCenter = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否显示行号列。
        /// </summary>
        [DefaultValue(false)]
        [Category("Appearance")]
        [Description("获取或设置是否显示行号列。")]
        public bool ShowRowNumber { get; set; }

        /// <summary>
        /// 获取或设置是否可以选择多项。
        /// </summary>
        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("获取或设置是否可以选择多项。")]
        public bool MultiSelect { get; set; }

        /// <summary>
        /// 获取或设置鼠标移上时是否显示状态。
        /// </summary>
        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("获取或设置鼠标移上时是否显示状态。")]
        public bool HotTracking { get; set; }

        /// <summary>
        /// 获取或设置单击列头时是否可排序。
        /// </summary>
        [DefaultValue(true)]
        [Category("Behavior")]
        [Description("获取或设置单击列头时是否可排序。")]
        public bool Sortable { get; set; }

        /// <summary>
        /// 获取或设置行移上时显示手形鼠标。
        /// </summary>
        [DefaultValue(true)]
        [Category("Behavior")]
        [Description("获取或设置行移上时显示手形鼠标。")]
        public bool HandCursor { get; set; }

        /// <summary>
        /// 获取或设置 <see cref="TreeListItem"/> 默认的图像。
        /// </summary>
        [DefaultValue(null)]
        [Category("Appearance")]
        [Description("获取或设置 TreeListItem 默认的图像。")]
        public Image DefaultImage
        {
            get { return _defaultImage; }
            set
            {
                if (_defaultImage != value)
                {
                    _defaultImage = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置 <see cref="TreeListItem"/> 节点展开时的默认图像。
        /// </summary>
        [DefaultValue(null)]
        [Category("Appearance")]
        [Description("获取或设置 TreeListItem 节点展开时的默认图像。")]
        public Image DefaultExpandNodeImage
        {
            get { return _defaultExpandImage; }
            set
            {
                if (_defaultExpandImage != value)
                {
                    _defaultExpandImage = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置 <see cref="TreeListItem"/> 节点收缩时的默认图像。
        /// </summary>
        [DefaultValue(null)]
        [Category("Appearance")]
        [Description("获取或设置 TreeListItem 节点收缩时的默认图像。")]
        public Image DefaultCollapseNodeImage
        {
            get { return _defaultCollapseImage; }
            set
            {
                if (_defaultCollapseImage != value)
                {
                    _defaultCollapseImage = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置是否使用默认的节点图片。
        /// </summary>
        [DefaultValue(false)]
        [Category("Appearance")]
        [Description("获取或设置是否使用默认的节点图片。")]
        public bool UseDefaultNodeImage
        {
            get { return _useDefaultNodeImage; }
            set
            {
                if (_useDefaultNodeImage != value)
                {
                    _useDefaultNodeImage = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置 <see cref="TreeListItem"/> 的 KeyValue 值所对应的属性名称。
        /// </summary>
        [DefaultValue((string)null)]
        [Category("Data")]
        [Description("获取或设置 TreeListItem 的 KeyValue 值所对应的属性名称。")]
        public string KeyField { get; set; }

        /// <summary>
        /// 获取或设置用于显示图像的 ImageList 控件。
        /// </summary>
        [Category("Appearance")]
        [Description("获取或设置用于显示图像的 ImageList 控件。")]
        [DefaultValue((ImageList)null)]
        public ImageList ImageList
        {
            get { return _imageList; }
            set
            {
                if (_imageList != value)
                {
                    _imageList = value;
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// 获取或设置组的字体。
        /// </summary>
        [Category("Appearance")]
        [Description("获取或设置组的字体。")]
        public Font GroupFont { get; set; }

        /// <summary>
        /// 获取或设置组的前景颜色。
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DarkBlue")]
        [Description("获取或设置组的前景颜色。")]
        public Color GroupForeColor
        {
            get { return _groupForeColor; }
            set
            {
                if (_groupForeColor != value)
                {
                    _groupForeColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取数据是否验证成功。
        /// </summary>
        [Browsable(false)]
        public bool IsValid
        {
            get { return _invalidateCells.Count == 0; }
        }

        /// <summary>
        /// 获取选中项集合。
        /// </summary>
        [Browsable(false)]
        public TreeListSelectedItemCollection SelectedItems
        {
            get { return _selectedList ?? (_selectedList = new TreeListSelectedItemCollection(this)); }
        }

        /// <summary>
        /// 获取勾选项集合。
        /// </summary>
        [Browsable(false)]
        public TreeListCheckedItemCollection CheckedItems
        {
            get { return _checkedList ?? (_checkedList = new TreeListCheckedItemCollection(this)); }
        }

        /// <summary>
        /// 获取或设置页脚项。
        /// </summary>
        [Browsable(false)]
        public TreeListItem Footer
        {
            get
            {
                return _footer;
            }
            set
            {
                if (value != null && value.GetType() == typeof(TreeListItem))
                {
                    _footer = new TreeListFooterItem(value);
                }
                else
                {
                    _footer = value;
                }

                if (_footer != null)
                {
                    _footer.Update(this, null, 0);
                }
            }
        }

        /// <summary>
        /// 获取是否显示水平滚动条。
        /// </summary>
        internal bool ShowHorScrollBar
        {
            get
            {
                return GetColumnTotalWidth() + (ShowVerScrollBar ? _vbar.Width : 0) > _bound.WorkBound.Width;
            }
        }

        /// <summary>
        /// 获取是否显示垂直滚动条。
        /// </summary>
        internal bool ShowVerScrollBar
        {
            get
            {
                var height = _virMgr.Items.Count * GetAdjustItemHeight();
                return height > _bound.WorkBound.Height - _bound.ColumnBound.Height - (ShowFooter ? FooterHeight : 0);
            }
        }

        /// <summary>
        /// 获取是否获得当前焦点。
        /// </summary>
        public override bool Focused
        {
            get
            {
                return base.Focused || _editor.IsEditing;
            }
        }

        /// <summary>
        /// 获取或设置排序的标识。
        /// </summary>
        [Browsable(false)]
        public string SortKey { get; set; }

        /// <summary>
        /// 获取或设置排序的顺序。
        /// </summary>
        [Browsable(false)]
        public SortOrder SortOrder { get; set; }

        #endregion

        #region 方法重载
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_lastHoverHitInfo != null &&
                _lastHoverHitInfo.HitTestType == TreeListHitTestType.ColumnSize &&
                e.Button == MouseButtons.Left)
            {
                if (e.X <= _lastHoverHitInfo.Bounds.Left || e.X > Width - RIGHT_EDGE)
                {
                    return;
                }

                //拖动调整线
                DrawDragLine(e.X);

                if (_dragSizePos != -1)
                {
                    //抹去原来的调整线
                    DrawDragLine(_dragSizePos);
                }

                _dragSizePos = e.X;
            }
            else
            {
                var current = HitTest(e.X, e.Y, TreeListHitTestEventType.MouseMove);

                if (current != null && (_lastHoverHitInfo == null || !_lastHoverHitInfo.Equals(current)))
                {
                    if (e.Button != MouseButtons.Left ||
                        current.HitTestType != TreeListHitTestType.ColumnSize)
                    {
                        ProcessLastHitTestInfo(current, _lastHoverHitInfo);
                        ProcessHitTestInfo(current, e.Button == MouseButtons.Left ? DrawState.Pressed : DrawState.Hot);
                        _lastHoverHitInfo = current;
                    }
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!_tip.IsShow)
            {
                ProcessLastHitTestInfo(null, _lastHoverHitInfo);
                _lastHoverHitInfo = null;
            }

            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!Focused)
            {
                Focus();
            }

            if (e.Button == MouseButtons.Left &&
                _lastHoverHitInfo != null &&
                _lastHoverHitInfo.HitTestType == TreeListHitTestType.ColumnSize)
            {
                HideEditor();
            }
            else if (e.Button == MouseButtons.Left)
            {
                var current = HitTest(e.X, e.Y, TreeListHitTestEventType.MouseDown);

                if (current != null)
                {
                    ProcessHitTestInfoClick(current, TreeListHitTestEventType.MouseDown);
                    ProcessHitTestInfo(current, DrawState.Pressed);
                    _lastHoverHitInfo = current;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_lastHoverHitInfo != null &&
                _lastHoverHitInfo.HitTestType == TreeListHitTestType.ColumnSize &&
                e.Button == MouseButtons.Left)
            {
                ResizeColumnWidth((TreeListColumn)_lastHoverHitInfo.Element, e.X);

                //抹去原来的调整线
                DrawDragLine(_dragSizePos);

                _dragSizePos = -1;

                _bound.Reset();
                SetScrollBars();

                Invalidate();
            }
            else if (e.Button == MouseButtons.Left)
            {
                var current = HitTest(e.X, e.Y, TreeListHitTestEventType.MouseUp);

                if (current != null)
                {
                    ProcessHitTestInfoClick(current, TreeListHitTestEventType.MouseUp);
                    ProcessHitTestInfo(current, DrawState.Hot);
                }
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_vbar.Visible && Focused)
            {
                int newVal = _vbar.Value - ((e.Delta / 120) * SystemInformation.MouseWheelScrollLines * GetAdjustItemHeight());
                if (newVal < 0)
                {
                    newVal = 0;
                }
                else if (newVal > _vbar.Maximum - _vbar.LargeChange + 1)
                {
                    newVal = _vbar.Maximum - _vbar.LargeChange + 1;
                }

                _vbar.Value = Math.Max(0, newVal);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            RedrawSelectedItems();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (!_editor.IsEditing)
            {
                EndEdit();
                RedrawSelectedItems();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var current = HitTest(e.X, e.Y, TreeListHitTestEventType.MouseUp);

                if (current != null)
                {
                    ProcessHitTestInfoDbClick(current);
                }
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.W_SHOWWINDOW)
            {
                ProcessSpringColumnWidth();
            }

            base.WndProc(ref m);
        }

        protected override void CreateHandle()
        {
            _bound.Reset();
            base.CreateHandle();
        }

        protected override void OnResize(EventArgs e)
        {
            _bound.Reset();
            ProcessSpringColumnWidth();
            SetScrollBars();
            base.OnResize(e);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            Invalidate();
            base.OnControlRemoved(e);
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 开始更新数据。
        /// </summary>
        public void BeginUpdate()
        {
            _isUpdating = true;

            if (ShowLoading)
            {
                using (var g = CreateGraphics())
                {
                    Renderer.DrawLoading(new TreeListRenderEventArgs(this, g, _bound.ItemBound));
                }
            }

            NativeMethods.SendMessage(Handle, NativeMethods.W_SETREDRAW, 0, 0);
        }

        /// <summary>
        /// 终止更新数据。
        /// </summary>
        public void EndUpdate()
        {
            _isUpdating = false;
            _lastRowIndex = -1;
            NativeMethods.SendMessage(Handle, NativeMethods.W_SETREDRAW, 1, 0);
            UpdateItems();
        }

        /// <summary>
        /// 确保指定的 <see cref="TreeListItem"/> 可见。
        /// </summary>
        /// <param name="listitem"></param>
        public void EnsureVisible(TreeListItem listitem)
        {
            var vitem = _virMgr.Items.FirstOrDefault(s => s.Item == listitem);
            if (vitem == null)
            {
                var parent = listitem.Parent;
                while (parent != null)
                {
                    parent.Expended = true;
                    parent = parent.Parent;
                }
            }

            vitem = _virMgr.Items.FirstOrDefault(s => s.Item == listitem);
            if (vitem == null)
            {
                return;
            }

            var offsetTop = GetOffsetTop();
            var itemHeight = GetAdjustItemHeight();
            var y = vitem.Index * itemHeight - offsetTop;
            if (y + _bound.ItemBound.Y < _bound.ItemBound.Y)
            {
                _vbar.Value = Math.Max(0, vitem.Index * itemHeight);
            }
            else if (y >= _bound.ItemBound.Height - itemHeight)
            {
                _vbar.Value = Math.Max(0, Math.Min((vitem.Index - (_bound.ItemBound.Height + (_showHeader ? HeaderHeight : 0)) / itemHeight) * itemHeight, _vbar.Maximum));
            }
        }

        /// <summary>
        /// 获取指定的 <see cref="TreeListItem"/> 的位置。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Point GetItemPosition(TreeListItem item)
        {
            Guard.ArgumentNull(item, nameof(item));

            VirtualTreeListItem vitem;
            if ((vitem = _virMgr.Items.FirstOrDefault(s => s.Item.Equals(item))) != null)
            {
                var y = _bound.ColumnBound.Bottom + vitem.Index * GetAdjustItemHeight() - GetOffsetTop();
                return new Point(GetBorderWidth(), y);
            }

            return Point.Empty;
        }

        /// <summary>
        /// 获取指定的 <see cref="TreeListCell"/> 的位置。
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Point GetCellPosition(TreeListCell cell)
        {
            Guard.ArgumentNull(cell, nameof(cell));

            VirtualTreeListItem vitem;
            if ((vitem = _virMgr.Items.FirstOrDefault(s => s.Item.Equals(cell.Item))) != null)
            {
                var y = _bound.ColumnBound.Bottom + vitem.Index * GetAdjustItemHeight() - GetOffsetTop();
                var r = GetColumnBound(cell.Column);
                return new Point(r.Left, y);
            }

            return Point.Empty;
        }

        /// <summary>
        /// 对数据行进行分组或取消分组。
        /// </summary>
        /// <param name="grouping">为 true 则分组。</param>
        public void Grouping(bool grouping)
        {
            Groups.Clear();

            if (grouping)
            {
                foreach (var kvp in Items.GroupBy(s => s.Group).ToDictionary(s => s.Key, s => s.ToList()))
                {
                    var group = new TreeListGroup(kvp.Key);
                    Groups.Add(group);
                    group.Items.AddRange(kvp.Value);
                }
            }
        }

        /// <summary>
        /// 对集合进行排序。
        /// </summary>
        /// <param name="column"></param>
        /// <param name="order"></param>
        public void Sort(TreeListColumn column, SortOrder order)
        {
            Items.Sort(++_sortVersion, column, order);
            UpdateItems();
        }

        /// <summary>
        /// 重置所有高亮的项。
        /// </summary>
        public void ResetHighlight()
        {
            _groups.SelectMany(s => s.Items).Where(s => s.Highlight).ForEach(s => s.Highlight = false);
            Items.Where(s => s.Highlight).ForEach(s => s.Highlight = false);
        }

        #endregion

        #region 内部方法
        internal void SelectItem(TreeListItem item, bool selected, bool clearSelected = true)
        {
            if (clearSelected)
            {
                for (var i = SelectedItems.Count - 1; i >= 0; i--)
                {
                    SelectedItems[i].SetSelected(false);
                }

                SelectedItems.InternalClear();
                Invalidate(_bound.AvlieBound);
            }

            item.SetSelected(selected);
            HideEditor();

            if (selected)
            {
                SelectedItems.InternalAdd(item);
            }
            else
            {
                SelectedItems.InternalRemove(item);
            }

            RaiseItemSelectionChangedEvent();
            EnsureVisible(item);
            InvalidateItem(item);
        }

        internal void CheckItem(TreeListItem item, bool @checked)
        {
            if (@checked)
            {
                CheckedItems.InternalAdd(item);
            }
            else
            {
                CheckedItems.InternalRemove(item);
            }

            RaiseItemCheckChanged(item);

            Invalidate(_bound.AvlieBound);
        }
        #endregion

        #region 私有方式
        /// <summary>
        /// 绘制所有列头。
        /// </summary>
        /// <param name="g"></param>
        private void DrawColumns(Graphics g)
        {
            if (!ShowHeader)
            {
                return;
            }

            var workRect = _bound.ColumnBound;
            var x = workRect.X - GetOffsetLeft();

            g.KeepClip(_bound.ColumnBound, () =>
            {
                foreach (var column in Columns)
                {
                    if (column.Hidden)
                    {
                        continue;
                    }

                    var rect = new Rectangle(x, workRect.Top, column.Width, HeaderHeight);
                    var e = new TreeListColumnRenderEventArgs(column, g, rect);
                    DrawColumn(e);
                    x += column.Width;
                }

                //列头的总宽度小于控件的总宽度，则画最右的空列头
                if (x <= Width)
                {
                    var rect = new Rectangle(x, workRect.Top, Width - x, HeaderHeight);
                    var e = new TreeListColumnRenderEventArgs(null, g, rect);
                    DrawColumn(e);
                }
            });
        }

        private void DrawRowNumberColumn(Graphics g)
        {
            if (!ShowHeader || !ShowRowNumber)
            {
                return;
            }

            var rect = new Rectangle(_bound.WorkBound.X, _bound.WorkBound.Top, RowNumberWidth, HeaderHeight);
            var e = new TreeListColumnRenderEventArgs(null, g, rect);
            Renderer.DrawRowNumberColumn(e);
        }

        private void DrawColumn(TreeListColumnRenderEventArgs e)
        {
            e.Graphics.KeepClip(e.Bounds, () =>
            {
                Renderer.DrawColumnHeader(e);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphics"></param>
        private void DrawItems(Graphics graphics)
        {
            var workRect = _bound.ItemBound;

            if (_virMgr.Items.Count == 0 && !string.IsNullOrEmpty(NoneItemText))
            {
                Renderer.DrawNoneItem(new TreeListRenderEventArgs(this, graphics, workRect));
                return;
            }

            var y = workRect.Top;
            var isDrawing = false;
            var width = GetColumnTotalWidth();

            var fr = _vbar.Value / GetAdjustItemHeight();
            if (fr < 0)
            {
                fr = 0;
            }

            y -= _vbar.Value % GetAdjustItemHeight();

            graphics.KeepClip(_bound.AvlieBound, () =>
            {
                for (var i = fr; i < _virMgr.Items.Count; i++)
                {
                    var vitem = _virMgr.Items[i];
                    var rect = GetDrawItemRect(vitem, workRect, y, width);
                    if (!_bound.ItemBound.IntersectsWith(rect) && isDrawing)
                    {
                        break;
                    }

                    switch (vitem.ItemType)
                    {
                        case ItemType.Item:
                            var litem = vitem.Item as TreeListItem;
                            var e1 = new TreeListItemRenderEventArgs(litem, graphics, rect)
                            {
                                Alternate = i % 2 != 0,
                                DrawState = litem.Selected ? DrawState.Selected : DrawState.Normal
                            };
                            DrawItem(e1, false);

                            y = IncreaseItemY(y);
                            break;
                        case ItemType.Group:
                            var gitem = vitem.Item as TreeListGroup;
                            var e2 = new TreeListGroupRenderEventArgs(gitem, graphics, rect);
                            DrawGroup(e2, false);
                            y = IncreaseGroupY(y);
                            break;
                    }

                    isDrawing = true;
                }
            });
        }

        private Rectangle GetDrawItemRect(VirtualTreeListItem vitem, Rectangle workRect, int y, int width)
        {
            switch (vitem.ItemType)
            {
                case ItemType.Item:
                    return new Rectangle(workRect.X - GetOffsetLeft(), y, width, ItemHeight);
                case ItemType.Group:
                    return new Rectangle(workRect.X - GetOffsetLeft(), y, width, ItemHeight); //GroupHeight
                default:
                    return Rectangle.Empty;
            }
        }


        private TreeListItemRenderEventArgs GetListItemRenderEventArgs(Graphics graphics, VirtualTreeListItem item, DrawState state)
        {
            var width = GetColumnTotalWidth();

            var y = _bound.ItemBound.Y + item.Index * GetAdjustItemHeight() - GetOffsetTop();
            var rect = new Rectangle(_bound.ItemBound.X - _hbar.Value, y, width, ItemHeight);

            if (item.Item is TreeListItem titem)
            {
                return new TreeListItemRenderEventArgs(titem, graphics, rect)
                {
                    Alternate = item.Index % 2 != 0,
                    DrawState = state
                };
            }

            return null;
        }

        /// <summary>
        /// 重置所有列为无排序。
        /// </summary>
        /// <param name="column"></param>
        private void ResetSortOrders(TreeListColumn column)
        {
            foreach (var c in Columns)
            {
                if (!c.Equals(column))
                {
                    using (var graphics = CreateGraphics())
                    {
                        var e = new TreeListColumnRenderEventArgs(c, graphics, GetColumnBound(c));
                        graphics.KeepClip(_bound.ColumnBound, () => Renderer.DrawColumnHeader(e));
                    }
                }
            }
        }

        /// <summary>
        /// 处理上一个鼠标经过的对象。
        /// </summary>
        private void ProcessLastHitTestInfo(TreeListHitTestInfo current, TreeListHitTestInfo info)
        {
            if (info == null || info.Bounds.IsEmpty)
            {
                return;
            }

            HideToolTip();

            switch (info.HitTestType)
            {
                case TreeListHitTestType.Column:
                    using (var graphics = CreateGraphics())
                    {
                        var drawArgs = new TreeListColumnRenderEventArgs((TreeListColumn)info.Element, graphics, info.Bounds)
                        {
                            DrawState = DrawState.Normal
                        };
                        graphics.KeepClip(_bound.ColumnBound, () => Renderer.DrawColumnHeader(drawArgs));
                    }

                    break;
                case TreeListHitTestType.ColumnSize:
                    Cursor = Cursors.Default;
                    break;

                case TreeListHitTestType.Cell:
                    if ((info.Owner != null && current != null && current.HitTestType == TreeListHitTestType.Cell && !info.Owner.Equals(current.Owner)) ||
                        current == null || current.HitTestType != TreeListHitTestType.Cell)
                    {
                        ProcessLastHitTestInfo(current, info.Owner);
                    }

                    break;
                case TreeListHitTestType.Item:
                    if (HandCursor)
                    {
                        Cursor = Cursors.Default;
                    }

                    if (!HotTracking)
                    {
                        return;
                    }

                    var vitem = (VirtualTreeListItem)info.Element;
                    var item = (TreeListItem)vitem.Item;
                    if (item.Selected)
                    {
                        return;
                    }

                    using (var graphics = CreateGraphics())
                    {
                        var rect = info.Bounds;
                        var h = vitem.Index * GetAdjustItemHeight() + _bound.ItemBound.Top - GetOffsetTop() - info.Bounds.Y;
                        if (h != 0)
                        {
                            rect.Offset(0, h);
                        }

                        var drawArgs = new TreeListItemRenderEventArgs(item, graphics, rect)
                        {
                            DrawState = DrawState.Normal,
                            Alternate = vitem.Index % 2 != 0
                        };
                        DrawItem(drawArgs);
                    }

                    break;
            }
        }

        /// <summary>
        /// 重新调整 <see cref="TreeListColumn"/> 的宽度。
        /// </summary>
        /// <param name="column"></param>
        /// <param name="x"></param>
        private void ResizeColumnWidth(TreeListColumn column, int x)
        {
            //如果超出容器的右边，则容器宽度减去RIGHT_EDGE
            var width = x > Width - RIGHT_EDGE ? Width - RIGHT_EDGE : (x < _lastHoverHitInfo.Bounds.Left ? 1 :
                x - _lastHoverHitInfo.Bounds.Left + 4);
            column.SetWidth(width);
        }

        private int GetColumnTotalWidth()
        {
            return Columns.Aggregate(0, (d, c) => d += (c.Hidden ? 0 : c.Width));
        }

        /// <summary>
        /// 获取有效的数据区刷新范围。
        /// </summary>
        /// <returns></returns>
        private Rectangle GetAvailableItemBound()
        {
            var r = _bound.ItemBound;
            r.Width = Math.Min(GetColumnTotalWidth(), _bound.WorkBound.Width);
            return r;
        }

        /// <summary>
        /// 获取指定 <see cref="TreeListColumn"/> 的绘制区域。
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        internal Rectangle GetColumnBound(TreeListColumn column)
        {
            var workRect = _bound.WorkBound;
            var x = workRect.X - GetOffsetLeft();
            if (ShowRowNumber)
            {
                x += RowNumberWidth;
            }

            foreach (var c in Columns)
            {
                if (c.Hidden)
                {
                    continue;
                }

                if (column.Equals(c))
                {
                    return new Rectangle(x, workRect.Top, column.Width, HeaderHeight);
                }

                x += c.Width;
            }

            return Rectangle.Empty;
        }

        internal TreeListBound GetBoundSet()
        {
            return _bound;
        }

        internal void OnClearItems()
        {
            CheckedItems.InternalClear();
        }

        internal void OnRemoveItem(TreeListItem item)
        {
            if (CheckedItems.Contains(item))
            {
                CheckedItems.InternalRemove(item);
            }
        }

        internal void UpdateItems(int index = -1)
        {
            if (_isUpdating)
            {
                return;
            }

            if (Items.Count > 0 && index >= 0)
            {
                var newItem = index <= Items.Count - 1 ? Items[index] : Items.Last();
                SelectItem(newItem, true);
            }
            else
            {
                SelectedItems.InternalClear();
            }

            _virMgr.Recalc();
            SetScrollBars();
            Invalidate();
        }

        internal SortOrder GetSortOrder(TreeListColumn column)
        {
            if (column == _sortedColumn)
            {
                return _sortedOrder;
            }

            return SortOrder.None;
        }

        /// <summary>
        /// 初始化滚动条。
        /// </summary>
        private void InitScrollBars()
        {
            var h = GetAdjustItemHeight();
            _vbar = new VScrollBar() { Visible = false, LargeChange = h * 10, SmallChange = h };
            _vbar.ValueChanged += (o, e) =>
            {
                HideEditor();
                Invalidate(_bound.WorkBound);
            };
            Controls.Add(_vbar);

            _hbar = new HScrollBar() { Visible = false, LargeChange = 50, SmallChange = 5 };
            _hbar.ValueChanged += (o, e) =>
            {
                HideEditor();
                Invalidate(_bound.WorkBound);
            };

            Controls.Add(_hbar);

            _psize = new Panel() { BackColor = SystemColors.Control, Visible = false, Width = _vbar.Width, Height = _hbar.Height };
            Controls.Add(_psize);
        }

        /// <summary>
        /// 设置并显示滚动条。
        /// </summary>
        private void SetScrollBars()
        {
            var itemHeight = GetAdjustItemHeight();
            var width = GetColumnTotalWidth();
            var borderWidth = GetBorderWidth();
            var height = _virMgr.Items.Count * itemHeight;
            var large = GetAdjustItemHeight() * 10;

            if (ShowVerScrollBar)
            {
                _vbar.Location = new Point(Width - _vbar.Width - borderWidth, borderWidth);
                _vbar.Height = Height - borderWidth * 2 - (ShowHorScrollBar ? _hbar.Height : 0);
                _vbar.Maximum = height - _bound.ItemBound.Height;
                _vbar.LargeChange = _vbar.Maximum <= large ? 10 : large;
                _vbar.Maximum += (_vbar.LargeChange + 1);
                _vbar.Visible = true;
            }
            else
            {
                _vbar.Minimum = _vbar.Maximum = 0;
                _vbar.Visible = false;
                _vbar.Value = 0;
            }

            if (ShowHorScrollBar)
            {
                _hbar.Location = new Point(borderWidth, Height - _hbar.Height - borderWidth);
                _hbar.Width = Width - borderWidth * 2 - (ShowVerScrollBar ? _vbar.Width : 0);
                _hbar.Maximum = width - _bound.ItemBound.Width;
                _hbar.LargeChange = _hbar.Maximum <= 50 ? 10 : 50;
                _hbar.Maximum += (_hbar.LargeChange + 1);
                _hbar.Visible = true;
            }
            else
            {
                _hbar.Minimum = _hbar.Maximum = 0;
                _hbar.Visible = false;
                _hbar.Value = 0;
            }

            if (ShowVerScrollBar && ShowHorScrollBar)
            {
                _psize.Location = new Point(_hbar.Right, _vbar.Bottom);
                _psize.Visible = true;
            }
            else
            {
                _psize.Visible = false;
            }

            _bound.Reset();
        }

        private Rectangle OffsetHorRectangle(Rectangle rect)
        {
            var r = rect;
            r.Offset(-_hbar.Value, 0);
            return r;
        }

        private int GetAdjustItemHeight()
        {
            return ShowGridLines ? ItemHeight + 1 : ItemHeight;
        }

        private int GetBorderWidth()
        {
            return _bound.ItemBound.X - _bound.RowNumberBound.Width;
        }

        private int IncreaseItemY(int y)
        {
            y += ItemHeight;
            if (ShowGridLines)
            {
                y++;
            }

            return y;
        }

        private int IncreaseGroupY(int y)
        {
            y += ItemHeight; // GroupHeight
            if (ShowGridLines)
            {
                y++;
            }

            return y;
        }

        private int GetOffsetLeft()
        {
            return _hbar.Visible ? _hbar.Value : 0;
        }

        private int GetOffsetTop()
        {
            return _vbar.Visible ? _vbar.Value : 0;
        }

        private TreeListColumn GetFirstColumn()
        {
            return Columns.Count > 0 ? Columns[0] : null;
        }

        /// <summary>
        /// 获取 <see cref="TreeListCell"/> 中文本显示的矩形区域。
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="rect"><see cref="TreeListCell"/> 的矩形区域。</param>
        /// <returns></returns>
        private Rectangle GetCellTextRectangle(TreeListCell cell, Rectangle rect)
        {
            //如果是第一列
            if (GetFirstColumn() == cell.Column)
            {
                var offsetR = cell.Item.Level * Indent;

                //显示 +/- 需要缩进
                if (ShowPlusMinus)
                {
                    offsetR += 16;
                }

                //显示图像，需要缩进
                var image = cell.Item.GetImage() ?? cell.Item.TreeList.DefaultImage;
                if (image != null)
                {
                    offsetR += image.Width + 2;
                }

                if (ShowCheckBoxes)
                {
                    offsetR += 16;
                }

                var r = rect.ReduceRight(offsetR);

                //文本已看不到
                if (r.X > rect.Right)
                {
                    return Rectangle.Empty;
                }

                return r;
            }

            return rect;
        }

        /// <summary>
        /// 判断 <see cref="TreeListCell"/> 的文本是否超出矩形区域。
        /// </summary>
        /// <param name="cell">要检查的项。</param>
        /// <param name="rect">文本的矩形区域。</param>
        /// <returns></returns>
        private bool IsTextOverflow(TreeListCell cell, Rectangle rect)
        {
            var font = cell.Column.Font ?? cell.Column.TreeList.Font;
            using (var g = CreateGraphics())
            {
                int w = (int)g.MeasureString(cell.Text, font).Width;
                return w > rect.Width;
            }
        }

        /// <summary>
        /// 在 <see cref="TreeListCell"/> 上显示的个提示控件。
        /// </summary>
        /// <param name="cell">要显示的子项。</param>
        /// <param name="rect">要显示的矩形区域。</param>
        private void ShowToolTip(TreeListCell cell, Rectangle rect)
        {
            if (!rect.IsEmpty && IsTextOverflow(cell, rect) && _editor.Cell != cell)
            {
                _tip.Show(cell, rect);
            }
            else
            {
                HideToolTip();
            }
        }

        /// <summary>
        /// 隐藏 <see cref="TreeListCell"/> 上的提示控件。
        /// </summary>
        private void HideToolTip()
        {
            if (!_tip.IsShow)
            {
                return;
            }

            _tip.Hide();
        }

        private void HideEditor()
        {
            _editor.AcceptEdit();
        }

        public void BeginEdit(TreeListCell cell)
        {
            var rect = GetCellRectangle(cell);
            rect = GetCellTextRectangle(cell, rect);
            if (rect.IsEmpty)
            {
                return;
            }

            if (!Focused)
            {
                Focus();
            }

            if (!cell.Item.Selected)
            {
                cell.Item.Selected = true;
            }

            _editor.BeginEdit(cell, rect);
        }

        public void EndEdit()
        {
            _editor.AcceptEdit();
        }

        private Rectangle GetCellRectangle(TreeListCell cell)
        {
            var item = _virMgr.Items.FirstOrDefault(s => s.Item == cell.Item);
            if (item == null)
            {
                return Rectangle.Empty;
            }

            var r = GetColumnBound(cell.Column);
            var y = _bound.ItemBound.Y + item.Index * GetAdjustItemHeight() - GetOffsetTop();
            return new Rectangle(r.Left, y, r.Width, ItemHeight);
        }

        private void InvalidateItem(VirtualTreeListItem item)
        {
            var y = _bound.ItemBound.Y + item.Index * GetAdjustItemHeight() - GetOffsetTop();
            var rect = new Rectangle(_bound.AvlieBound.X - _hbar.Value, y, _bound.AvlieBound.Width, ItemHeight);

            Invalidate(rect);
        }

        internal void InvalidateItem(TreeListItem item)
        {
            if (Footer == item)
            {
                Invalidate(_bound.FooterBound);
            }
            else
            {
                var vitem = _virMgr.Items.FirstOrDefault(s => s.Item == item);
                if (vitem != null)
                {
                    InvalidateItem(vitem);
                }
            }
        }

        internal void InvalidateColumn(TreeListColumn column)
        {
            if (ShowHeader)
            {
                Invalidate(_bound.ColumnBound);
            }
        }

        internal void UpdateValidateFlags(TreeListCell cell)
        {
            if (!cell.IsValid && !_invalidateCells.Contains(cell))
            {
                _invalidateCells.Add(cell);
            }
            else if (cell.IsValid && _invalidateCells.Contains(cell))
            {
                _invalidateCells.Remove(cell);
            }
        }

        internal void RemoveValidateFlags(TreeListItem item)
        {
            for (var i = _invalidateCells.Count - 1; i >= 0; i--)
            {
                if (_invalidateCells[i].Item == item)
                {
                    _invalidateCells.RemoveAt(i);
                }
            }
        }

        internal void RemoveValidateFlags()
        {
            _invalidateCells.Clear();
        }
        #endregion
    }
}
