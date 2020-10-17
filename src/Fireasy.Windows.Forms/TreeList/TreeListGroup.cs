// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.ComponentModel;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    [DefaultProperty("Text")]
    public class TreeListGroup : Component, IVirtualItem
    {
        private bool _expanded = true;

        public TreeListGroup(string text)
        {
            Text = text;
        }

        public string Text { get; set; }

        public TreeListItemCollection Items { get; private set; }

        /// <summary>
        /// 获取或设置所属的 <see cref="TreeList"/> 容器。
        /// </summary>
        [Browsable(false)]
        public TreeList TreeList { get; internal set; }

        bool IVirtualItem.Selected { get; set; }

        /// <summary>
        /// 获取或设置节点是否展开。
        /// </summary>
        [Browsable(false)]
        public bool Expended
        {
            get { return _expanded; }
            set
            {
                if (_expanded != value)
                {
                    _expanded = value;
                    if (TreeList != null)
                    {
                        if (value && ShowExpanded)
                        {
                            //TreeList.RaiseDamanLoadEvent(this);
                            //IsDemandLoad = true;
                        }

                        //TreeList.ProcessItemExpand(this);
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

        internal void Update(TreeList treelist)
        {
            TreeList = treelist;
            Items = new TreeListItemCollection(treelist, null, 0);
        }
    }
}
