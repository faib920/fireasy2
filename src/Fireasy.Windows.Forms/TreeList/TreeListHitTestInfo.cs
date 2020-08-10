// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;

namespace Fireasy.Windows.Forms
{
    public class TreeListHitTestInfo
    {
        public TreeListHitTestInfo(TreeListHitTestType hitType, object element, Rectangle rect)
        {
            HitTestType = hitType;
            Element = element;
            Bounds = rect;
        }

        public TreeListHitTestInfo(TreeListHitTestType hitType)
        {
            HitTestType = hitType;
        }

        public Rectangle Bounds { get; internal set; }

        public object Element { get; internal set; }

        public TreeListHitTestInfo Owner { get; internal set; }

        public TreeListHitTestType HitTestType { get; internal set; }

        public override bool Equals(object obj)
        {
            var o = obj as TreeListHitTestInfo;
            if (o == null || Element == null)
            {
                return false;
            }

            return HitTestType == o.HitTestType && Element.Equals(o.Element);
        }
    }

    public enum TreeListHitTestType
    {
        Column,
        ColumnSize,
        Group,
        Item,
        Cell,
        PlusMinus,
        Checkbox
    }

    public enum TreeListHitTestEventType
    {
        MouseMove,
        MouseUp,
        MouseDown
    }
}
