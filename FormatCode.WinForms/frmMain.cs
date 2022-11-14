// --------------------------------------------------------------------------------
// Copyright (c) J.D. Purcell
//
// Licensed under the MIT License (see LICENSE.txt)
// --------------------------------------------------------------------------------
using FormatCode.Library;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FormatCode.WinForms;

public partial class frmMain : Form {
	public frmMain() {
		InitializeComponent();
	}

	private void frmMain_DragEnter(object sender, DragEventArgs e) {
		if (!panMain.Enabled) return;
		if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
		e.Effect = DragDropEffects.Copy;
	}

	private void frmMain_DragDrop(object sender, DragEventArgs e) {
		if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
		Run((string[])e.Data.GetData(DataFormats.FileDrop));
	}

	private void Run(IList<string> dirsAndFiles) {
		CodeFormatter formatter = new() {
			TabSize = Int32.Parse(txtTabSize.Text),
			TabStyle =
				rbTabStyleTabs.Checked ? TabStyle.Tabs :
				rbTabStyleSpaces.Checked ? TabStyle.Spaces :
				TabStyle.Detect,
			OpenBraceStyle =
				rbOpenBracesMoveDown.Checked ? OpenBraceStyle.MoveDown :
				rbOpenBracesMoveUp.Checked ? OpenBraceStyle.MoveUp :
				OpenBraceStyle.Leave,
			NewLineStyle = chkPreserveNewLineType.Checked ? NewLineStyle.Detect : NewLineStyle.MatchPlatform,
			RequireNewLineAtEnd = chkRequireNewLineAtEnd.Checked
		};
		Application.UseWaitCursor = true;
		panMain.Enabled = false;
		lblInstructions.Visible = false;
		lblStatus.Visible = true;
		lblStatus.Text = "";
		void OnProgress(int processedCount) {
			this.BeginInvoke(() => {
				lblStatus.Text = $"Processed {processedCount:#,##0} files.";
			});
		}
		void OnComplete(string errorMessage) {
			this.BeginInvoke(() => {
				Application.UseWaitCursor = false;
				Activate();
				if (errorMessage != null) {
					MessageBox.Show(this, errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else {
					MessageBox.Show(this, "Done!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				panMain.Enabled = true;
				lblStatus.Visible = false;
				lblInstructions.Visible = true;
			});
		}
		BatchCodeFormatter.BeginRun(formatter, CodeFileFinder.Find(dirsAndFiles), OnProgress, OnComplete);
	}
}
