namespace Fireasy.Windows.Forms
{
    partial class ErrorMessageBox
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

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lnkShow = new System.Windows.Forms.LinkLabel();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lnkShow
            // 
            this.lnkShow.AutoSize = true;
            this.lnkShow.Location = new System.Drawing.Point(0, 0);
            this.lnkShow.Name = "lnkShow";
            this.lnkShow.Size = new System.Drawing.Size(100, 23);
            this.lnkShow.TabIndex = 0;
            this.lnkShow.TabStop = true;
            this.lnkShow.Text = "显示详细信息";
            this.lnkShow.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkShow_LinkClicked);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.LinkLabel lnkShow;
    }
}
