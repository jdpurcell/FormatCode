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
			var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
			FormatCode(formatter, paths);
		}

		private void FormatCode(CodeFormatter formatter, string[] paths) {
			void Run() {
				var pathEnumList = new List<IEnumerable<string>>();
				foreach (string path in paths) {
					if (File.Exists(path)) {
						pathEnumList.Add(new[] { path });
					}
					else if (Directory.Exists(path)) {
						pathEnumList.Add(Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories));
					}
				}

				string[] ignoreNames = new string[] {  };
				string[] ignoreSuffixes = new string[] {  };
				string[] ignoreDirectories = new string[] {  };

				string errorMessage = null;
				foreach (string path in pathEnumList.SelectMany(pe => pe)) {
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
				}
				this.BeginInvoke(() => {
					Application.UseWaitCursor = false;
					panMain.Enabled = true;
					Activate();
					if (errorMessage != null) {
						MessageBox.Show(this, errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					else {
						MessageBox.Show(this, "Done!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				});
			}
			Application.UseWaitCursor = true;
			panMain.Enabled = false;
			new Thread(Run).Start();
		}
	}
}
