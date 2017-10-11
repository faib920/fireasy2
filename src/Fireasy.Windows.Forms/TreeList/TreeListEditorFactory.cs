// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Windows.Forms
{
    internal static class TreeListEditorFactory
    {
        internal static ITreeListEditor Create(TreeListCellDataType dbType)
        {
            switch (dbType)
            {
                case TreeListCellDataType.String:
                    return new TreeListTextEditor();
                case TreeListCellDataType.Integer:
                    return new TreeListIntegerEditor();
                case TreeListCellDataType.Decimal:
                    return new TreeListNumericEditor();
                case TreeListCellDataType.Boolean:
                    return new TreeListCheckBoxEditor();
                case TreeListCellDataType.DateTime:
                    return new TreeListDateTimeEditor();
                default:
                    return null;
            }
        }
    }
}
