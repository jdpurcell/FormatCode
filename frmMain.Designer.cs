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
			this.SuspendLayout();
			// 
			// chkMoveOpenBracesUp
			// 
			this.chkMoveOpenBracesUp.AutoSize = true;
			this.chkMoveOpenBracesUp.Location = new System.Drawing.Point(12, 38);
			this.chkMoveOpenBracesUp.Name = "chkMoveOpenBracesUp";
			this.chkMoveOpenBracesUp.Size = new System.Drawing.Size(188, 17);
			this.chkMoveOpenBracesUp.TabIndex = 2;
			this.chkMoveOpenBracesUp.Text = "Move opening braces to same line";
			this.chkMoveOpenBracesUp.UseVisualStyleBackColor = true;
			// 
			// chkRequireNewLineAtEnd
			// 
			this.chkRequireNewLineAtEnd.AutoSize = true;
			this.chkRequireNewLineAtEnd.Location = new System.Drawing.Point(12, 61);
			this.chkRequireNewLineAtEnd.Name = "chkRequireNewLineAtEnd";
			this.chkRequireNewLineAtEnd.Size = new System.Drawing.Size(163, 17);
			this.chkRequireNewLineAtEnd.TabIndex = 3;
			this.chkRequireNewLineAtEnd.Text = "Require newline at end of file";
			this.chkRequireNewLineAtEnd.UseVisualStyleBackColor = true;
			// 
			// txtTabSize
			// 
			this.txtTabSize.Location = new System.Drawing.Point(70, 12);
			this.txtTabSize.Name = "txtTabSize";
			this.txtTabSize.Size = new System.Drawing.Size(32, 20);
			this.txtTabSize.TabIndex = 1;
			this.txtTabSize.Text = "4";
			// 
			// lblTabSize
			// 
			this.lblTabSize.AutoSize = true;
			this.lblTabSize.Location = new System.Drawing.Point(12, 15);
			this.lblTabSize.Name = "lblTabSize";
			this.lblTabSize.Size = new System.Drawing.Size(52, 13);
			this.lblTabSize.TabIndex = 0;
			this.lblTabSize.Text = "Tab Size:";
			// 
			// frmMain
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(248, 180);
			this.Controls.Add(this.lblTabSize);
			this.Controls.Add(this.txtTabSize);
			this.Controls.Add(this.chkRequireNewLineAtEnd);
			this.Controls.Add(this.chkMoveOpenBracesUp);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Code Formatter";
			this.TopMost = true;
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmMain_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmMain_DragEnter);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox chkMoveOpenBracesUp;
		private System.Windows.Forms.CheckBox chkRequireNewLineAtEnd;
		private System.Windows.Forms.TextBox txtTabSize;
		private System.Windows.Forms.Label lblTabSize;

	}
}

