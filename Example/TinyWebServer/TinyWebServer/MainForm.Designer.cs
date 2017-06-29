namespace TinyWebServer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.mnClose = new System.Windows.Forms.MenuItem();
            this.mnMenu = new System.Windows.Forms.MenuItem();
            this.mnStart = new System.Windows.Forms.MenuItem();
            this.mnStop = new System.Windows.Forms.MenuItem();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblIpAddr = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.mnClose);
            this.mainMenu1.MenuItems.Add(this.mnMenu);
            // 
            // mnClose
            // 
            this.mnClose.Text = "Close";
            this.mnClose.Click += new System.EventHandler(this.mnClose_Click);
            // 
            // mnMenu
            // 
            this.mnMenu.MenuItems.Add(this.mnStart);
            this.mnMenu.MenuItems.Add(this.mnStop);
            this.mnMenu.Text = "Menu";
            // 
            // mnStart
            // 
            this.mnStart.Text = "Start server";
            this.mnStart.Click += new System.EventHandler(this.mnStart_Click);
            // 
            // mnStop
            // 
            this.mnStop.Text = "Stop server";
            this.mnStop.Click += new System.EventHandler(this.mnStop_Click);
            // 
            // listBoxLog
            // 
            this.listBoxLog.Location = new System.Drawing.Point(3, 43);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(234, 212);
            this.listBoxLog.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 20);
            this.label1.Text = "Log:";
            // 
            // lblIpAddr
            // 
            this.lblIpAddr.Location = new System.Drawing.Point(52, 20);
            this.lblIpAddr.Name = "lblIpAddr";
            this.lblIpAddr.Size = new System.Drawing.Size(185, 20);
            this.lblIpAddr.Text = "...";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lblIpAddr);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBoxLog);
            this.Menu = this.mainMenu1;
            this.Name = "MainForm";
            this.Text = "Tiny Web Server";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem mnClose;
        private System.Windows.Forms.MenuItem mnMenu;
        private System.Windows.Forms.MenuItem mnStart;
        private System.Windows.Forms.MenuItem mnStop;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblIpAddr;

    }
}

