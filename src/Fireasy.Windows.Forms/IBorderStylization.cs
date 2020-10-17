// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 表示该控件可以设置边框风格。
    /// </summary>
    public interface IBorderStylization
    {
        BorderStyle BorderStyle { get; set; }
    }
}
