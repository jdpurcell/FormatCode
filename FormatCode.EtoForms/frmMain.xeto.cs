using Eto.Forms;
using Eto.Serialization.Xaml;
using FormatCode.Library;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FormatCode.EtoForms;

public class frmMain : Form {
	private StackLayout slMain;
	private TextBox txtTabSize;
	private RadioButtonList rblTabStyle;
	private RadioButtonList rblOpenBraceStyle;
	private RadioButtonList rblNewLineStyle;
	private CheckBox chkRequireNewLineAtEnd;
	private Label lblInstructions;
	private Label lblStatus;

	public frmMain() {
		XamlReader.Load(this);

		slMain.DragEnter += slMain_DragEnter;
		slMain.DragDrop += slMain_DragDrop;

		if (Eto.Platform.Instance.IsMac) {
			Menu = new MenuBar { IncludeSystemItems = MenuBarSystemItems.Quit };
		}

		txtTabSize.Text = "4";
		rblTabStyle.SelectedKey = TabStyle.Detect.ToString();
		rblOpenBraceStyle.SelectedKey = OpenBraceStyle.Leave.ToString();
		rblNewLineStyle.SelectedKey = NewLineStyle.Detect.ToString();
	}

	private void slMain_DragEnter(object sender, DragEventArgs e) {
		if (!slMain.Enabled || !IsFileDrop(e)) return;
		e.Effects = DragEffects.Copy;
	}

	private void slMain_DragDrop(object sender, DragEventArgs e) {
		if (!slMain.Enabled || !IsFileDrop(e)) return;
		Run(e.Data.Uris.Where(n => n.IsFile).Select(n => n.LocalPath).ToList());
	}

	private void Run(IList<string> dirsAndFiles) {
		CodeFormatter formatter = new() {
			TabSize = Int32.Parse(txtTabSize.Text),
			TabStyle = Enum.Parse<TabStyle>(rblTabStyle.SelectedKey),
			OpenBraceStyle = Enum.Parse<OpenBraceStyle>(rblOpenBraceStyle.SelectedKey),
			NewLineStyle = Enum.Parse<NewLineStyle>(rblNewLineStyle.SelectedKey),
			RequireNewLineAtEnd = chkRequireNewLineAtEnd.Checked.Value
		};
		slMain.Enabled = false;
		lblInstructions.Visible = false;
		lblStatus.Visible = true;
		lblStatus.Text = "";
		void OnProgress(int processedCount) {
			Application.Instance.AsyncInvoke(() => {
				lblStatus.Text = $"Processed {processedCount:#,##0} files.";
			});
		}
		void OnComplete(string errorMessage) {
			Application.Instance.AsyncInvoke(() => {
				if (errorMessage != null) {
					MessageBox.Show(this, errorMessage, "Error", MessageBoxButtons.OK, MessageBoxType.Error);
				}
				else {
					MessageBox.Show(this, "Done!", "Done", MessageBoxButtons.OK, MessageBoxType.Information);
				}
				slMain.Enabled = true;
				lblStatus.Visible = false;
				lblInstructions.Visible = true;
			});
		}
		BatchCodeFormatter.BeginRun(formatter, CodeFileFinder.Find(dirsAndFiles), OnProgress, OnComplete);
	}

	private static bool IsFileDrop(DragEventArgs e) =>
		e.Data.ContainsUris && e.Data.Uris.Any(n => n.IsFile);
}
