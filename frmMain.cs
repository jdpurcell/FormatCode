// --------------------------------------------------------------------------------
// Copyright (c) J.D. Purcell
//
// Licensed under the MIT License (see LICENSE.txt)
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace FormatCode {
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
			var formatter = new CodeFormatter {
				TabSize = Int32.Parse(txtTabSize.Text),
				TabsAsSpaces = rbTabsAsSpaces.Checked,
				MoveOpenBracesUp = chkMoveOpenBracesUp.Checked,
				RequireNewLineAtEnd = chkRequireNewLineAtEnd.Checked
			};
			var dirsAndFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
			FormatCode(formatter, dirsAndFiles);
		}

		private void FormatCode(CodeFormatter formatter, string[] dirsAndFiles) {
			void Run() {
				IEnumerable<string> paths = Enumerable.Empty<string>();
				foreach (string path in dirsAndFiles) {
					if (File.Exists(path)) {
						paths = paths.Concat(Enumerable.Repeat(path, 1));
					}
					else if (Directory.Exists(path)) {
						paths = paths.Concat(Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories));
					}
				}

				string[] ignoreNames = new string[] {  };
				string[] ignoreSuffixes = new string[] {  };
				string[] ignoreDirectories = new string[] {  };

				int processedCount = 0;
				TimeSpan uiUpdateInterval = TimeSpan.FromMilliseconds(15);
				DateTime nextUIUpdateTime = DateTime.UtcNow;
				string errorMessage = null;
				foreach (string path in paths) {
					if (!Path.GetExtension(path).Equals(".cs", StringComparison.OrdinalIgnoreCase)) continue;
					if (ignoreNames.Any(n => Path.GetFileName(path).Equals(n, StringComparison.OrdinalIgnoreCase))) continue;
					if (ignoreSuffixes.Any(s => path.EndsWith(s, StringComparison.OrdinalIgnoreCase))) continue;
					if (ignoreDirectories.Any(d => path.IndexOf($@"\{d}\", StringComparison.OrdinalIgnoreCase) != -1)) continue;
					try {
						formatter.Format(path);
					}
					catch (Exception ex) {
						errorMessage = $"{path}\r\n{ex.Message}";
						break;
					}
					processedCount++;
					DateTime timeNow = DateTime.UtcNow;
					if (timeNow >= nextUIUpdateTime) {
						this.Invoke(() => {
							lblStatus.Text = $"Processed {processedCount:#,##0} files.";
						});
						nextUIUpdateTime = timeNow + uiUpdateInterval;
					}
				}
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
			Application.UseWaitCursor = true;
			panMain.Enabled = false;
			lblInstructions.Visible = false;
			lblStatus.Visible = true;
			lblStatus.Text = "";
			new Thread(Run).Start();
		}
	}
}
