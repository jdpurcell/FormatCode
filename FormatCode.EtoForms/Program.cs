using Eto.Forms;
using System;

namespace FormatCode.EtoForms;

internal class Program {
	[STAThread]
	static void Main(string[] args) {
		new Application(Eto.Platform.Detect).Run(new frmMain());
	}
}
