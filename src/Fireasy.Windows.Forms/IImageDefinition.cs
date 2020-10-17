// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public interface IImageDefinition
    {
        ImageList ImageList { get; }

        string ImageKey { get; set; }

        int ImageIndex { get; set; }

        Image Image { get; set; }
    }
}
