// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    internal class TreeListBound
    {
        private readonly TreeList _treeList;

        public TreeListBound(TreeList treeList)
        {
            _treeList = treeList;
        }

        public Rectangle WorkBound { get; private set; }
        public Rectangle FrozenColumnBound { get; private set; }
        public Rectangle ColumnBound { get; private set; }
        public Rectangle FrozenItemBound { get; private set; }
        public Rectangle ItemBound { get; private set; }
        public Rectangle RowNumberBound { get; private set; }
        public Rectangle AvlieBound { get; set; }
        public Rectangle FooterBound { get; set; }

        public void Reset()
        {
            GetWorkspaceRectangle(_treeList.ClientRectangle);
            GetRowNumberRectangle();
            GetColumnHeaderRectangle();
            GetItemRectangle();
            GetFooterRectangle();
        }

        /// <summary>
        /// 获取工作区域，即除去边框的范围。
        /// </summary>
        /// <returns></returns>
        private void GetWorkspaceRectangle(Rectangle rect)
        {
            var rr = rect;
            if (!Application.RenderWithVisualStyles && _treeList.BorderStyle == BorderStyle.Fixed3D)
            {
                rr.Inflate(-2, -2);
            }
            else
            {
                rr.Inflate(-1, -1);
            }

            WorkBound = rr;
        }

        /// <summary>
        /// 获取数据行区域。
        /// </summary>
        /// <returns></returns>
        private void GetItemRectangle()
        {
            var top = WorkBound.Top;
            var width = WorkBound.Width;
            var height = WorkBound.Height;

            if (_treeList.ShowHeader)
            {
                width = WorkBound.Width;
                top += _treeList.HeaderHeight;
                height -= _treeList.HeaderHeight;
            }

            if (_treeList.ShowVerScrollBar)
            {
                width -= 17;
            }

            if (_treeList.ShowHorScrollBar)
            {
                height -= 17;
            }

            if (_treeList.ShowFooter)
            {
                height -= _treeList.FooterHeight;
            }

            var frozenWidth = 0;// _treeList.Columns.Where(s => s.Frozen).Sum(s => s.Width);

            ItemBound = new Rectangle(WorkBound.X + RowNumberBound.Width + frozenWidth, top, width, height);
            FrozenItemBound = new Rectangle(WorkBound.X + RowNumberBound.Width, top, frozenWidth, height);
        }

        private void GetRowNumberRectangle()
        {
            if (_treeList.ShowRowNumber)
            {
                RowNumberBound = new Rectangle(WorkBound.X, WorkBound.Y, _treeList.RowNumberWidth, WorkBound.Height);
            }
            else
            {
                RowNumberBound = Rectangle.Empty;
            }
        }

        /// <summary>
        /// 获取列头区域。
        /// </summary>
        /// <returns></returns>
        private void GetColumnHeaderRectangle()
        {
            if (!_treeList.ShowHeader)
            {
                ColumnBound = Rectangle.Empty;
                AvlieBound = WorkBound;
            }
            else
            {
                var frozenWidth = 0;// _treeList.Columns.Where(s => s.Frozen).Sum(s => s.Width);

                ColumnBound = new Rectangle(WorkBound.X + RowNumberBound.Width + frozenWidth, WorkBound.Y, WorkBound.Width - RowNumberBound.Width, _treeList.HeaderHeight);
                FrozenColumnBound = new Rectangle(WorkBound.X + RowNumberBound.Width, WorkBound.Y, frozenWidth, _treeList.HeaderHeight);
                AvlieBound = new Rectangle(WorkBound.X, ColumnBound.Bottom, WorkBound.Width, WorkBound.Height - _treeList.HeaderHeight);
            }
        }

        private void GetFooterRectangle()
        {
            if (!_treeList.ShowFooter)
            {
                FooterBound = Rectangle.Empty;
            }
            else
            {
                FooterBound = new Rectangle(WorkBound.X, ItemBound.Bottom, WorkBound.Width, _treeList.FooterHeight);
            }
        }
    }
}
