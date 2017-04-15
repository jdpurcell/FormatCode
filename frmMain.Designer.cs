namespace FormatCode {
	partial class frmMain {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.chkMoveOpenBracesUp = new System.Windows.Forms.CheckBox();
			this.chkRequireNewLineAtEnd = new System.Windows.Forms.CheckBox();
			this.txtTabSize = new System.Windows.Forms.TextBox();
			this.lblTabSize = new System.Windows.Forms.Label();
			this.panMain = new System.Windows.Forms.Panel();
			this.grpTabs = new System.Windows.Forms.GroupBox();
			this.panTabsSpaces = new System.Windows.Forms.Panel();
			this.rbTabsAsTabs = new System.Windows.Forms.RadioButton();
			this.rbTabsAsSpaces = new System.Windows.Forms.RadioButton();
			this.panMain.SuspendLayout();
			this.grpTabs.SuspendLayout();
			this.panTabsSpaces.SuspendLayout();
			this.SuspendLayout();
			// 
			// chkMoveOpenBracesUp
			// 
			this.chkMoveOpenBracesUp.AutoSize = true;
			this.chkMoveOpenBracesUp.Location = new System.Drawing.Point(12, 64);
			this.chkMoveOpenBracesUp.Name = "chkMoveOpenBracesUp";
			this.chkMoveOpenBracesUp.Size = new System.Drawing.Size(188, 17);
			this.chkMoveOpenBracesUp.TabIndex = 1;
			this.chkMoveOpenBracesUp.Text = "Move opening braces to same line";
			this.chkMoveOpenBracesUp.UseVisualStyleBackColor = true;
			// 
			// chkRequireNewLineAtEnd
			// 
			this.chkRequireNewLineAtEnd.AutoSize = true;
			this.chkRequireNewLineAtEnd.Location = new System.Drawing.Point(12, 88);
			this.chkRequireNewLineAtEnd.Name = "chkRequireNewLineAtEnd";
			this.chkRequireNewLineAtEnd.Size = new System.Drawing.Size(163, 17);
			this.chkRequireNewLineAtEnd.TabIndex = 2;
			this.chkRequireNewLineAtEnd.Text = "Require newline at end of file";
			this.chkRequireNewLineAtEnd.UseVisualStyleBackColor = true;
			// 
			// txtTabSize
			// 
			this.txtTabSize.Location = new System.Drawing.Point(44, 16);
			this.txtTabSize.Name = "txtTabSize";
			this.txtTabSize.Size = new System.Drawing.Size(32, 20);
			this.txtTabSize.TabIndex = 1;
			this.txtTabSize.Text = "4";
			// 
			// lblTabSize
			// 
			this.lblTabSize.AutoSize = true;
			this.lblTabSize.Location = new System.Drawing.Point(8, 20);
			this.lblTabSize.Name = "lblTabSize";
			this.lblTabSize.Size = new System.Drawing.Size(30, 13);
			this.lblTabSize.TabIndex = 0;
			this.lblTabSize.Text = "Size:";
			// 
			// panMain
			// 
			this.panMain.Controls.Add(this.grpTabs);
			this.panMain.Controls.Add(this.chkMoveOpenBracesUp);
			this.panMain.Controls.Add(this.chkRequireNewLineAtEnd);
			this.panMain.Location = new System.Drawing.Point(0, 0);
			this.panMain.Name = "panMain";
			this.panMain.Size = new System.Drawing.Size(248, 180);
			this.panMain.TabIndex = 0;
			// 
			// grpTabs
			// 
			this.grpTabs.Controls.Add(this.panTabsSpaces);
			this.grpTabs.Controls.Add(this.lblTabSize);
			this.grpTabs.Controls.Add(this.txtTabSize);
			this.grpTabs.Location = new System.Drawing.Point(8, 8);
			this.grpTabs.Name = "grpTabs";
			this.grpTabs.Size = new System.Drawing.Size(212, 48);
			this.grpTabs.TabIndex = 0;
			this.grpTabs.TabStop = false;
			this.grpTabs.Text = "Tabs";
			// 
			// panTabsSpaces
			// 
			this.panTabsSpaces.Controls.Add(this.rbTabsAsSpaces);
			this.panTabsSpaces.Controls.Add(this.rbTabsAsTabs);
			this.panTabsSpaces.Location = new System.Drawing.Point(88, 18);
			this.panTabsSpaces.Name = "panTabsSpaces";
			this.panTabsSpaces.Size = new System.Drawing.Size(120, 20);
			this.panTabsSpaces.TabIndex = 2;
			// 
			// rbTabsAsTabs
			// 
			this.rbTabsAsTabs.AutoSize = true;
			this.rbTabsAsTabs.Checked = true;
			this.rbTabsAsTabs.Location = new System.Drawing.Point(0, 0);
			this.rbTabsAsTabs.Name = "rbTabsAsTabs";
			this.rbTabsAsTabs.Size = new System.Drawing.Size(49, 17);
			this.rbTabsAsTabs.TabIndex = 0;
			this.rbTabsAsTabs.TabStop = true;
			this.rbTabsAsTabs.Text = "Tabs";
			this.rbTabsAsTabs.UseVisualStyleBackColor = true;
			// 
			// rbTabsAsSpaces
			// 
			this.rbTabsAsSpaces.AutoSize = true;
			this.rbTabsAsSpaces.Location = new System.Drawing.Point(56, 0);
			this.rbTabsAsSpaces.Name = "rbTabsAsSpaces";
			this.rbTabsAsSpaces.Size = new System.Drawing.Size(61, 17);
			this.rbTabsAsSpaces.TabIndex = 1;
			this.rbTabsAsSpaces.Text = "Spaces";
			this.rbTabsAsSpaces.UseVisualStyleBackColor = true;
			// 
			// frmMain
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(248, 180);
			this.Controls.Add(this.panMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Code Formatter";
			this.TopMost = true;
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmMain_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmMain_DragEnter);
			this.panMain.ResumeLayout(false);
			this.panMain.PerformLayout();
			this.grpTabs.ResumeLayout(false);
			this.grpTabs.PerformLayout();
			this.panTabsSpaces.ResumeLayout(false);
			this.panTabsSpaces.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.CheckBox chkMoveOpenBracesUp;
		private System.Windows.Forms.CheckBox chkRequireNewLineAtEnd;
		private System.Windows.Forms.TextBox txtTabSize;
		private System.Windows.Forms.Label lblTabSize;
		private System.Windows.Forms.Panel panMain;
		private System.Windows.Forms.GroupBox grpTabs;
		private System.Windows.Forms.Panel panTabsSpaces;
		private System.Windows.Forms.RadioButton rbTabsAsSpaces;
		private System.Windows.Forms.RadioButton rbTabsAsTabs;
	}
}

