namespace Fireasy.Windows.Forms
{
    partial class ErrorMessageBox
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer _components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _components.Dispose();
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
            this._components = new System.ComponentModel.Container();
            this._timer1 = new System.Windows.Forms.Timer(this._components);
            this._lnkShow = new System.Windows.Forms.LinkLabel();
            // 
            // timer1
            // 
            this._timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lnkShow
            // 
            this._lnkShow.AutoSize = true;
            this._lnkShow.Location = new System.Drawing.Point(0, 0);
            this._lnkShow.Name = "lnkShow";
            this._lnkShow.Size = new System.Drawing.Size(100, 23);
            this._lnkShow.TabIndex = 0;
            this._lnkShow.TabStop = true;
            this._lnkShow.Text = "显示详细信息";
            this._lnkShow.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkShow_LinkClicked);

        }

        #endregion

        private System.Windows.Forms.Timer _timer1;
        private System.Windows.Forms.LinkLabel _lnkShow;
    }
}
