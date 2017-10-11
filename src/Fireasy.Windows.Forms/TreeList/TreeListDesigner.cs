using System.Collections;
using System.Web.UI.Design;

namespace Fireasy.Windows.Forms
{
    internal class TreeListDesigner : ControlDesigner
    {
        protected override void PostFilterProperties(IDictionary Properties)
        {
            Properties.Remove("Text");
        }
    }
}
