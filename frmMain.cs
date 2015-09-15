// ---------------------------------------------------------------------------
// Copyright 2011 J.D. Purcell
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ---------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FormatCode {
	public partial class frmMain : Form {
		public frmMain() {
			InitializeComponent();
		}

		private void frmMain_DragEnter(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void frmMain_DragDrop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				CodeFormatter formatter = new CodeFormatter();
				formatter.TabSize = Int32.Parse(txtTabSize.Text);
				formatter.MoveOpenBracesUp = chkMoveOpenBracesUp.Checked;
				formatter.RequireNewLineAtEnd = chkRequireNewLineAtEnd.Checked;
				string[] ignoreNames = new string[] {  };
				string[] ignoreSuffixes = new string[] {  };
				string[] ignoreDirectories = new string[] {  };
				string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
				List<IEnumerable<string>> pathEnumList = new List<IEnumerable<string>>();
				foreach (string path in paths) {
					if (File.Exists(path)) {
						pathEnumList.Add(new[] { path });
					}
					else if (Directory.Exists(path)) {
						pathEnumList.Add(Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories));
					}
				}
				foreach (string path in pathEnumList.SelectMany(pe => pe)) {
					if (!Path.GetExtension(path).Equals(".cs", StringComparison.OrdinalIgnoreCase)) continue;
					if (ignoreNames.Any(n => Path.GetFileName(path).Equals(n, StringComparison.OrdinalIgnoreCase))) continue;
					if (ignoreSuffixes.Any(s => path.EndsWith(s, StringComparison.OrdinalIgnoreCase))) continue;
					if (ignoreDirectories.Any(d => path.IndexOf(@"\" + d + @"\", StringComparison.OrdinalIgnoreCase) != -1)) continue;
					formatter.Format(path);
				}
				Activate();
				MessageBox.Show(this, "Done!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}
}
