// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Windows.Forms.Configuration;

namespace Fireasy.Windows.Forms
{
    public static class ThemeManager
    {
        static ThemeManager()
        {
            var section = ConfigurationUnity.GetSection<ThemeConfigurationSection>();
            if (section != null)
            {
                if (section.TabControlRedererType != null)
                {
                    BaseRenderer = section.BaseRedererType.New<BaseRenderer>();
                }

                if (section.TreeListRedererType != null)
                {
                    TreeListRenderer = section.TreeListRedererType.New<TreeListRenderer>();
                }

                if (section.TabControlRedererType != null)
                {
                    //TabControlRenderer = section.TabControlRedererType.New<TabControlRenderer>();
                }
            }

            if (BaseRenderer == null)
            {
                BaseRenderer = new BaseRenderer();
            }

            if (TreeListRenderer == null)
            {
                TreeListRenderer = new TreeListRenderer();
            }

            //if (TabControlRenderer == null)
            {
                //TabControlRenderer = new TabControlRenderer();
            }
        }

        public static BaseRenderer BaseRenderer { get; set; }

        public static TreeListRenderer TreeListRenderer { get; set; }

        //public static TabControlRenderer TabControlRenderer { get; set; }
    }
}
