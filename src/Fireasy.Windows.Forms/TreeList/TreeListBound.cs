using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    internal class TreeListBound
    {
        private TreeList treeList;

        public TreeListBound(TreeList treeList)
        {
            this.treeList = treeList;
        }

        public Rectangle WorkBound { get; private set; }
        public Rectangle ColumnBound { get; private set; }
        public Rectangle ItemBound { get; private set; }
        public Rectangle RowNumberBound { get; private set; }
        public Rectangle AvlieBound { get; set; }
        public Rectangle FooterBound { get; set; }

        public void Reset()
        {
            GetWorkspaceRectangle(treeList.ClientRectangle);
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
            if (!Application.RenderWithVisualStyles && treeList.BorderStyle == BorderStyle.Fixed3D)
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

            if (treeList.ShowHeader)
            {
                width = WorkBound.Width;
                top += treeList.HeaderHeight;
                height -= treeList.HeaderHeight;
            }

            if (treeList.ShowVerScrollBar)
            {
                width -= 17;
            }

            if (treeList.ShowHorScrollBar)
            {
                height -= 17;
            }

            if (treeList.ShowFooter)
            {
                height -= treeList.FooterHeight;
            }

            ItemBound = new Rectangle(WorkBound.X + RowNumberBound.Width, top, width, height);
        }

        private void GetRowNumberRectangle()
        {
            if (treeList.ShowRowNumber)
            {
                RowNumberBound = new Rectangle(WorkBound.X, WorkBound.Y, treeList.RowNumberWidth, WorkBound.Height);
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
            if (!treeList.ShowHeader)
            {
                ColumnBound = Rectangle.Empty;
                AvlieBound = WorkBound;
            }
            else
            {
                ColumnBound = new Rectangle(WorkBound.X + RowNumberBound.Width, WorkBound.Y, WorkBound.Width, treeList.HeaderHeight);
                AvlieBound = new Rectangle(WorkBound.X, ColumnBound.Bottom, WorkBound.Width, WorkBound.Height - treeList.HeaderHeight);
            }
        }

        private void GetFooterRectangle()
        {
            if (!treeList.ShowFooter)
            {
                FooterBound = Rectangle.Empty;
            }
            else
            {
                FooterBound = new Rectangle(WorkBound.X, ItemBound.Bottom, WorkBound.Width, treeList.FooterHeight);
            }
        }
    }
}
