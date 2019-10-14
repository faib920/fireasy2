namespace Fireasy.Windows.Forms.Tests
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.treeList1 = new Fireasy.Windows.Forms.TreeList();
            this.treeListColumn1 = new Fireasy.Windows.Forms.TreeListColumn();
            this.treeListColumn2 = new Fireasy.Windows.Forms.TreeListColumn();
            this.treeListColumn3 = new Fireasy.Windows.Forms.TreeListColumn();
            this.treeListColumn4 = new Fireasy.Windows.Forms.TreeListColumn();
            this.treeListColumn5 = new Fireasy.Windows.Forms.TreeListColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // treeList1
            // 
            this.treeList1.AlternateBackColor = System.Drawing.Color.Empty;
            this.treeList1.Columns.AddRange(new Fireasy.Windows.Forms.TreeListColumn[] {
            this.treeListColumn1,
            this.treeListColumn2,
            this.treeListColumn3,
            this.treeListColumn4,
            this.treeListColumn5});
            this.treeList1.DataSource = null;
            this.treeList1.Footer = null;
            this.treeList1.GroupFont = new System.Drawing.Font("Consolas", 12F);
            this.treeList1.HandCursor = false;
            this.treeList1.Location = new System.Drawing.Point(12, 12);
            this.treeList1.Name = "treeList1";
            this.treeList1.NoneItemText = "没有可显示的数据";
            this.treeList1.RowNumberIndex = 0;
            this.treeList1.Size = new System.Drawing.Size(828, 494);
            this.treeList1.SortKey = null;
            this.treeList1.SortOrder = System.Windows.Forms.SortOrder.None;
            this.treeList1.TabIndex = 0;
            this.treeList1.Text = "treeList1";
            // 
            // treeListColumn1
            // 
            this.treeListColumn1.CellForeColor = System.Drawing.Color.Empty;
            this.treeListColumn1.ForeColor = System.Drawing.Color.Empty;
            this.treeListColumn1.Formatter = null;
            this.treeListColumn1.Image = null;
            this.treeListColumn1.Text = "treeListColumn1";
            this.treeListColumn1.Validator = null;
            this.treeListColumn1.Width = 200;
            // 
            // treeListColumn2
            // 
            this.treeListColumn2.CellForeColor = System.Drawing.Color.Empty;
            this.treeListColumn2.ForeColor = System.Drawing.Color.Empty;
            this.treeListColumn2.Formatter = null;
            this.treeListColumn2.Image = null;
            this.treeListColumn2.Text = "treeListColumn2";
            this.treeListColumn2.Validator = null;
            this.treeListColumn2.Width = 200;
            // 
            // treeListColumn3
            // 
            this.treeListColumn3.CellForeColor = System.Drawing.Color.Empty;
            this.treeListColumn3.ForeColor = System.Drawing.Color.Empty;
            this.treeListColumn3.Formatter = null;
            this.treeListColumn3.Image = null;
            this.treeListColumn3.Text = "treeListColumn3";
            this.treeListColumn3.Validator = null;
            this.treeListColumn3.Width = 200;
            // 
            // treeListColumn4
            // 
            this.treeListColumn4.CellForeColor = System.Drawing.Color.Empty;
            this.treeListColumn4.ForeColor = System.Drawing.Color.Empty;
            this.treeListColumn4.Formatter = null;
            this.treeListColumn4.Image = null;
            this.treeListColumn4.Text = "treeListColumn4";
            this.treeListColumn4.Validator = null;
            this.treeListColumn4.Width = 200;
            // 
            // treeListColumn5
            // 
            this.treeListColumn5.CellForeColor = System.Drawing.Color.Empty;
            this.treeListColumn5.ForeColor = System.Drawing.Color.Empty;
            this.treeListColumn5.Formatter = null;
            this.treeListColumn5.Image = null;
            this.treeListColumn5.Text = "treeListColumn5";
            this.treeListColumn5.Validator = null;
            this.treeListColumn5.Width = 200;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(353, 489);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 47);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(852, 518);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.treeList1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private TreeList treeList1;
        private TreeListColumn treeListColumn1;
        private TreeListColumn treeListColumn2;
        private TreeListColumn treeListColumn3;
        private TreeListColumn treeListColumn4;
        private TreeListColumn treeListColumn5;
        private System.Windows.Forms.Button button1;
    }
}

