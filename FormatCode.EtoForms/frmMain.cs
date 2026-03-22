using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Drawing;
using Eto.Forms;
using FormatCode.Library;

namespace FormatCode.EtoForms;

public class frmMain : Form {
	private readonly StackLayout slMain;
	private readonly TextBox txtTabSize;
	private readonly RadioButtonList rblTabStyle;
	private readonly RadioButtonList rblOpenBraceStyle;
	private readonly RadioButtonList rblNewLineStyle;
	private readonly CheckBox chkRequireNewLineAtEnd;
	private readonly Label lblInstructions;
	private readonly Label lblStatus;

	static frmMain() {
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	}

	public frmMain() {
		Title = "FormatCode";
		Resizable = false;
		Maximizable = false;
		Topmost = true;
		ClientSize = new Size(420, 320);

		txtTabSize = new TextBox { Width = 40, Text = "4" };
		rblTabStyle = CreateRadioButtonList(
			(TabStyle.Detect.ToString(), "Detect"),
			(TabStyle.Tabs.ToString(), "Tabs"),
			(TabStyle.Spaces.ToString(), "Spaces")
		);
		rblOpenBraceStyle = CreateRadioButtonList(
			(OpenBraceStyle.Leave.ToString(), "Leave"),
			(OpenBraceStyle.MoveDown.ToString(), "Move Down"),
			(OpenBraceStyle.MoveUp.ToString(), "Move Up")
		);
		rblNewLineStyle = CreateRadioButtonList(
			(NewLineStyle.Detect.ToString(), "Detect"),
			(NewLineStyle.CRLF.ToString(), "CR/LF"),
			(NewLineStyle.LF.ToString(), "LF")
		);
		chkRequireNewLineAtEnd = new CheckBox { Text = "Require newline at end of file" };
		lblInstructions = new Label { Text = "Drop files/folders here.", TextAlignment = TextAlignment.Center };
		lblStatus = new Label { Visible = false, TextAlignment = TextAlignment.Center };

		slMain = new StackLayout {
			Orientation = Orientation.Vertical,
			Padding = 10,
			Spacing = 12,
			AllowDrop = true,
			Items = {
				CreateTabsGroup(),
				CreateOpenBracesGroup(),
				CreateNewLinesGroup(),
				new StackLayout {
					Orientation = Orientation.Vertical,
					Spacing = 8,
					Padding = new Padding(0, 4, 0, 0),
					Items = { chkRequireNewLineAtEnd }
				},
				new StackLayout {
					Orientation = Orientation.Vertical,
					HorizontalContentAlignment = HorizontalAlignment.Stretch,
					Items = { lblInstructions, lblStatus }
				}
			}
		};

		slMain.DragEnter += slMain_DragEnter;
		slMain.DragDrop += slMain_DragDrop;
		Content = slMain;

		if (Eto.Platform.Instance.IsMac) {
			Menu = new MenuBar { IncludeSystemItems = MenuBarSystemItems.Quit };
		}

		rblTabStyle.SelectedKey = TabStyle.Detect.ToString();
		rblOpenBraceStyle.SelectedKey = OpenBraceStyle.Leave.ToString();
		rblNewLineStyle.SelectedKey = NewLineStyle.Detect.ToString();
	}

	private Control CreateTabsGroup() =>
		new GroupBox {
			Text = "Tabs",
			Padding = 6,
			Content = new StackLayout {
				Orientation = Orientation.Vertical,
				Items = {
					new StackLayout {
						Orientation = Orientation.Horizontal,
						VerticalContentAlignment = VerticalAlignment.Center,
						Spacing = 10,
						Items = {
							new Label { Text = "Size:" },
							txtTabSize
						}
					},
					rblTabStyle
				}
			}
		};

	private Control CreateOpenBracesGroup() =>
		new GroupBox {
			Text = "Open Braces",
			Padding = 6,
			Content = rblOpenBraceStyle
		};

	private Control CreateNewLinesGroup() =>
		new GroupBox {
			Text = "Newlines",
			Padding = 6,
			Content = rblNewLineStyle
		};

	private static RadioButtonList CreateRadioButtonList(params (string Key, string Text)[] items) {
		RadioButtonList list = new() {
			Orientation = Orientation.Horizontal,
			Spacing = new Size(10, 8),
			Padding = new Padding(0, 8, 0, 0)
		};
		foreach ((string key, string text) in items) {
			list.Items.Add(new ListItem { Key = key, Text = text });
		}
		return list;
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
			FallbackEncoding = Encoding.GetEncoding(1252),
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
