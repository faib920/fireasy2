using Fireasy.Common.Caching;
using Fireasy.Common.Serialization;
using System.Data;
using Fireasy.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fireasy.Data;
using Fireasy.Data.Provider;
using Fireasy.Common.Extensions;

namespace Fireasy.Windows.Forms.Tests
{
    public partial class Form1 : Form
    {
        private DbContext db = new DbContext();

        public Form1()
        {
            InitializeComponent();

            stringLocalizerHolder1.Apply();

            Fireasy.Common.Subscribes.DefaultSubscribeManager.Instance.AddSubscriber("", (b) => { });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MemoryCacheManager.Instance.Add("aa", "dfasfdafasfd", new RelativeTime(TimeSpan.FromSeconds(100)));

            var c1 = new ComplexComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            var c11 = new TreeListComplexComboBoxEditor(c1);
            c1.Items.Add("dfadfadfsf");
            c1.Items.Add("dfadfadfsf");
            c1.Items.Add("dfadfadfsf");
            c1.Items.Add("dfadfadfsf");
            treeList1.Columns[1].SetEditor(c11);
            treeList1.Columns[1].Editable = true;

            var c2 = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            var c21 = new TreeListComboBoxEditor(c2);
            c2.Items.Add("dfadfadfsf");
            c2.Items.Add("dfadfadfsf");
            c2.Items.Add("dfadfadfsf");
            c2.Items.Add("dfadfadfsf");
            treeList1.Columns[2].SetEditor(c21);
            treeList1.Columns[2].Editable = true;

            for (var i = 0; i < 100; i++)
            {
                var r = treeList1.Items.AddCells(i.ToString(), "aa", "bb", "cc", "dd");
            }

            comboBox1.Items.Add(new TreeListItem { Text = "aaa", KeyValue = 1 });
            comboBox1.Items.Add(new TreeListItem { Text = "bbb", KeyValue = 2 });
            comboBox1.Items.Add(new TreeListItem { Text = "ccc", KeyValue = 3 });
            comboBox1.Items.Add(new TreeListItem { Text = "ddd", KeyValue = 4 });
        }

        public class DataItem
        {
            public string Name { get; set; }

            public object Value { get; set; }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

    public class DbContext : EntityContext
    {
        protected override void OnConfiguring(EntityContextOptionsBuilder builder)
        {
            builder.UseSqlServer("dfadf");

        }
    }
}
