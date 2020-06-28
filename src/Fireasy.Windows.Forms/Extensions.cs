// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public static class Extensions
    {
        public static void Invoke(this Control control, Action method)
        {
            if (control.InvokeRequired)
            {
                control.Invoke((Delegate)method);
            }
            else
            {
                method();
            }
        }
    }
}
