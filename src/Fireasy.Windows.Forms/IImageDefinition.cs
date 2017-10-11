using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
