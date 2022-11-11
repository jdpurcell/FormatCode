using System;
using System.Windows.Forms;

namespace FormatCode;

public static class ExtensionMethods {
	public static object Invoke(this Control control, Action action) {
		return control.Invoke(action);
	}

	public static IAsyncResult BeginInvoke(this Control control, Action action) {
		return control.BeginInvoke(action);
	}
}
