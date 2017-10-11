// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Fireasy.Windows.Forms
{
    public class DataFormatEditor : UITypeEditor
    {
        private DataFormatListBox modelUI;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                var edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                var control = (IDataFormattable)context.Instance;

                modelUI = new DataFormatListBox();
                modelUI.Start(edSvc, value);

                edSvc.DropDownControl(modelUI);
                value = modelUI.Value;
                modelUI.End();
            }

            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override bool IsDropDownResizable
        {
            get
            {
                return true;
            }
        }

        private class DataFormatListBox : ListBox
        {
            private IWindowsFormsEditorService edSvc;

            public DataFormatListBox()
            {
                Items.Add("0");
                Items.Add("0.00");
                Items.Add("#,##0");
                Items.Add("#,##0.00");
                Items.Add("￥#,##0.00");
                Items.Add("0%");
                Items.Add("0.00%");
                Items.Add("yyyy-MM-dd");
                Items.Add("yyyy-MM");
                Items.Add("yyyy年MM月dd日");
                Items.Add("yyyy年MM月");
                Items.Add("[ed]");

                Height = 200;

                SelectedIndexChanged += DataFormatListBox_SelectedIndexChanged;
            }

            void DataFormatListBox_SelectedIndexChanged(object sender, EventArgs e)
            {
                Value = base.SelectedItem;
                edSvc.CloseDropDown();
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                SelectedItems.Clear();
                this.edSvc = edSvc;
                Value = value;

                if (value == null)
                {
                    return;
                }

                foreach (var item in base.Items)
                {
                    if (item.Equals(value))
                    {
                        SelectedItem = item;
                        break;
                    }
                }
            }

            public void End()
            {
                edSvc = null;
            }

            public object Value { get; set; }
        }
    }
}
