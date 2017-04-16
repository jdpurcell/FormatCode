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
			this.chkPreserveNewLineType = new System.Windows.Forms.CheckBox();
			this.txtTabSize = new System.Windows.Forms.TextBox();
			this.lblTabSize = new System.Windows.Forms.Label();
			this.panMain = new System.Windows.Forms.Panel();
			this.grpTabs = new System.Windows.Forms.GroupBox();
			this.panTabsSpaces = new System.Windows.Forms.Panel();
			this.rbTabStyleTabs = new System.Windows.Forms.RadioButton();
			this.rbTabStyleSpaces = new System.Windows.Forms.RadioButton();
			this.rbTabStyleDetect = new System.Windows.Forms.RadioButton();
			this.lblInstructions = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.panMain.SuspendLayout();
			this.grpTabs.SuspendLayout();
			this.panTabsSpaces.SuspendLayout();
			this.SuspendLayout();
			// 
			// chkMoveOpenBracesUp
			// 
			this.chkMoveOpenBracesUp.AutoSize = true;
			this.chkMoveOpenBracesUp.Location = new System.Drawing.Point(12, 88);
			this.chkMoveOpenBracesUp.Name = "chkMoveOpenBracesUp";
			this.chkMoveOpenBracesUp.Size = new System.Drawing.Size(188, 17);
			this.chkMoveOpenBracesUp.TabIndex = 1;
			this.chkMoveOpenBracesUp.Text = "Move opening braces to same line";
			this.chkMoveOpenBracesUp.UseVisualStyleBackColor = true;
			// 
			// chkRequireNewLineAtEnd
			// 
			this.chkRequireNewLineAtEnd.AutoSize = true;
			this.chkRequireNewLineAtEnd.Location = new System.Drawing.Point(12, 112);
			this.chkRequireNewLineAtEnd.Name = "chkRequireNewLineAtEnd";
			this.chkRequireNewLineAtEnd.Size = new System.Drawing.Size(163, 17);
			this.chkRequireNewLineAtEnd.TabIndex = 2;
			this.chkRequireNewLineAtEnd.Text = "Require newline at end of file";
			this.chkRequireNewLineAtEnd.UseVisualStyleBackColor = true;
			// 
			// chkPreserveNewLineType
			// 
			this.chkPreserveNewLineType.AutoSize = true;
			this.chkPreserveNewLineType.Location = new System.Drawing.Point(12, 136);
			this.chkPreserveNewLineType.Name = "chkPreserveNewLineType";
			this.chkPreserveNewLineType.Size = new System.Drawing.Size(130, 17);
			this.chkPreserveNewLineType.TabIndex = 3;
			this.chkPreserveNewLineType.Text = "Preserve newline type";
			this.chkPreserveNewLineType.UseVisualStyleBackColor = true;
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
			this.panMain.Controls.Add(this.chkPreserveNewLineType);
			this.panMain.Location = new System.Drawing.Point(0, 0);
			this.panMain.Name = "panMain";
			this.panMain.Size = new System.Drawing.Size(252, 156);
			this.panMain.TabIndex = 0;
			// 
			// grpTabs
			// 
			this.grpTabs.Controls.Add(this.panTabsSpaces);
			this.grpTabs.Controls.Add(this.lblTabSize);
			this.grpTabs.Controls.Add(this.txtTabSize);
			this.grpTabs.Location = new System.Drawing.Point(8, 8);
			this.grpTabs.Name = "grpTabs";
			this.grpTabs.Size = new System.Drawing.Size(200, 72);
			this.grpTabs.TabIndex = 0;
			this.grpTabs.TabStop = false;
			this.grpTabs.Text = "Tabs";
			// 
			// panTabsSpaces
			// 
			this.panTabsSpaces.Controls.Add(this.rbTabStyleTabs);
			this.panTabsSpaces.Controls.Add(this.rbTabStyleSpaces);
			this.panTabsSpaces.Controls.Add(this.rbTabStyleDetect);
			this.panTabsSpaces.Location = new System.Drawing.Point(12, 44);
			this.panTabsSpaces.Name = "panTabsSpaces";
			this.panTabsSpaces.Size = new System.Drawing.Size(184, 20);
			this.panTabsSpaces.TabIndex = 2;
			// 
			// rbTabStyleTabs
			// 
			this.rbTabStyleTabs.AutoSize = true;
			this.rbTabStyleTabs.Checked = true;
			this.rbTabStyleTabs.Location = new System.Drawing.Point(0, 0);
			this.rbTabStyleTabs.Name = "rbTabStyleTabs";
			this.rbTabStyleTabs.Size = new System.Drawing.Size(49, 17);
			this.rbTabStyleTabs.TabIndex = 0;
			this.rbTabStyleTabs.TabStop = true;
			this.rbTabStyleTabs.Text = "Tabs";
			this.rbTabStyleTabs.UseVisualStyleBackColor = true;
			// 
			// rbTabStyleSpaces
			// 
			this.rbTabStyleSpaces.AutoSize = true;
			this.rbTabStyleSpaces.Location = new System.Drawing.Point(56, 0);
			this.rbTabStyleSpaces.Name = "rbTabStyleSpaces";
			this.rbTabStyleSpaces.Size = new System.Drawing.Size(61, 17);
			this.rbTabStyleSpaces.TabIndex = 1;
			this.rbTabStyleSpaces.Text = "Spaces";
			this.rbTabStyleSpaces.UseVisualStyleBackColor = true;
			// 
			// rbTabStyleDetect
			// 
			this.rbTabStyleDetect.AutoSize = true;
			this.rbTabStyleDetect.Location = new System.Drawing.Point(124, 0);
			this.rbTabStyleDetect.Name = "rbTabStyleDetect";
			this.rbTabStyleDetect.Size = new System.Drawing.Size(57, 17);
			this.rbTabStyleDetect.TabIndex = 2;
			this.rbTabStyleDetect.Text = "Detect";
			this.rbTabStyleDetect.UseVisualStyleBackColor = true;
			// 
			// lblInstructions
			// 
			this.lblInstructions.Location = new System.Drawing.Point(8, 168);
			this.lblInstructions.Name = "lblInstructions";
			this.lblInstructions.Size = new System.Drawing.Size(236, 16);
			this.lblInstructions.TabIndex = 3;
			this.lblInstructions.Text = "Drop files/folders here.";
			this.lblInstructions.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblStatus
			// 
			this.lblStatus.Location = new System.Drawing.Point(8, 168);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(236, 16);
			this.lblStatus.TabIndex = 4;
			this.lblStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// frmMain
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(252, 192);
			this.Controls.Add(this.lblInstructions);
			this.Controls.Add(this.lblStatus);
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
		private System.Windows.Forms.CheckBox chkPreserveNewLineType;
		private System.Windows.Forms.TextBox txtTabSize;
		private System.Windows.Forms.Label lblTabSize;
		private System.Windows.Forms.Panel panMain;
		private System.Windows.Forms.GroupBox grpTabs;
		private System.Windows.Forms.Panel panTabsSpaces;
		private System.Windows.Forms.RadioButton rbTabStyleTabs;
		private System.Windows.Forms.RadioButton rbTabStyleSpaces;
		private System.Windows.Forms.RadioButton rbTabStyleDetect;
		private System.Windows.Forms.Label lblInstructions;
		private System.Windows.Forms.Label lblStatus;
	}
}

