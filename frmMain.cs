// --------------------------------------------------------------------------------
// Copyright (c) 2011-2017 J.D. Purcell
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
