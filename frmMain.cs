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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormatCode {
	public partial class frmMain : Form {
		private static readonly string[] _ignoreNames = {  };
		private static readonly string[] _ignoreSuffixes = {  };
		private static readonly string[] _ignoreDirectories = {  };

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
				TabStyle = rbTabStyleTabs.Checked ? TabStyle.Tabs : rbTabStyleSpaces.Checked ? TabStyle.Spaces : TabStyle.Detect,
				MoveOpenBracesUp = chkMoveOpenBracesUp.Checked,
				RequireNewLineAtEnd = chkRequireNewLineAtEnd.Checked,
				PreserveNewLineType = chkPreserveNewLineType.Checked
			};
			var dirsAndFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
			FormatCode(formatter, EnumerateCodeFiles(dirsAndFiles));
		}

		private static IEnumerable<string> EnumerateCodeFiles(string[] dirsAndFiles) {
			bool IsExcluded(string path) =>
				!Path.GetExtension(path).Equals(".cs", StringComparison.OrdinalIgnoreCase) ||
				_ignoreNames.Any(n => Path.GetFileName(path).Equals(n, StringComparison.OrdinalIgnoreCase)) ||
				_ignoreSuffixes.Any(s => path.EndsWith(s, StringComparison.OrdinalIgnoreCase)) ||
				_ignoreDirectories.Any(d => path.IndexOf($@"\{d}\", StringComparison.OrdinalIgnoreCase) != -1);

			foreach (string path in dirsAndFiles) {
				if (File.Exists(path) && !IsExcluded(path)) {
					yield return path;
				}
				else if (Directory.Exists(path)) {
					foreach (string filePath in Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories)) {
						if (!IsExcluded(filePath)) {
							yield return filePath;
						}
					}
				}
			}
		}

		private void FormatCode(CodeFormatter formatter, IEnumerable<string> paths) {
			void Run() {
				object syncObj = new object();
				int processedCount = 0;
				TimeSpan uiUpdateInterval = TimeSpan.FromMilliseconds(15);
				DateTime nextUIUpdateTime = DateTime.UtcNow;
				void Process(string path) {
					try {
						formatter.Format(path);
					}
					catch (Exception ex) {
						throw new Exception($"{path}\r\n\r\n{ex.Message}");
					}
					string newStatus = null;
					lock (syncObj) {
						processedCount++;
						DateTime timeNow = DateTime.UtcNow;
						if (timeNow >= nextUIUpdateTime) {
							newStatus = $"Processed {processedCount:#,##0} files.";
							nextUIUpdateTime = timeNow + uiUpdateInterval;
						}
					}
					if (newStatus != null) {
						this.Invoke(() => {
							lblStatus.Text = newStatus;
						});
					}
				}

				string errorMessage = null;
				try {
					Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, Process);
				}
				catch (AggregateException ae) {
					errorMessage = ae.InnerException?.Message ?? "(Empty AggregateException)";
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
