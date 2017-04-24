using System;
using System.Globalization;
using System.Windows.Forms;

namespace FormatCode {
	public static class ExtensionMethods {
		public static object Invoke(this Control control, Action action) {
			return control.Invoke(action);
		}

		public static IAsyncResult BeginInvoke(this Control control, Action action) {
			return control.BeginInvoke(action);
		}
	}

	public static class UnicodeHelper {
		public static bool IsIDStart(char c) {
			UnicodeCategory uc = Char.GetUnicodeCategory(c);
			// NOTE: Does not include Other_ID_Start characters
			return IsIDStartCategory(uc) && !IsPatternWhiteSpace(c) && !IsPatternSyntax(c);
		}

		public static bool IsIDContinue(char c) {
			UnicodeCategory uc = Char.GetUnicodeCategory(c);
			// NOTE: Does not include Other_ID_Continue characters
			return IsIDContinueCategory(uc) && !IsPatternWhiteSpace(c) && !IsPatternSyntax(c);
		}

		private static bool IsIDStartCategory(UnicodeCategory uc) =>
			uc == UnicodeCategory.UppercaseLetter || uc == UnicodeCategory.LowercaseLetter || uc == UnicodeCategory.TitlecaseLetter ||
			uc == UnicodeCategory.ModifierLetter || uc == UnicodeCategory.OtherLetter || uc == UnicodeCategory.LetterNumber;

		private static bool IsIDContinueCategory(UnicodeCategory uc) =>
			IsIDStartCategory(uc) || uc == UnicodeCategory.NonSpacingMark || uc == UnicodeCategory.SpacingCombiningMark ||
			uc == UnicodeCategory.DecimalDigitNumber || uc == UnicodeCategory.ConnectorPunctuation;

		private static bool IsPatternWhiteSpace(char c) =>
			(c >= '\u0009' && c <= '\u000D') || c == '\u0020' || c == '\u0085' || c == '\u200E' || c == '\u200F' || c == '\u2028' || c == '\u2029';

		private static bool IsPatternSyntax(char c) =>
			(c >= '\u0021' && c <= '\u002F') || (c >= '\u003A' && c <= '\u0040') || (c >= '\u005B' && c <= '\u005E') || (c >= '\u007B' && c <= '\u007E') ||
			(c >= '\u00A1' && c <= '\u00A7') || (c >= '\u2010' && c <= '\u2027') || (c >= '\u2030' && c <= '\u203E') || (c >= '\u2041' && c <= '\u2053') ||
			(c >= '\u2055' && c <= '\u205E') || (c >= '\u2190' && c <= '\u245F') || (c >= '\u2500' && c <= '\u2775') || (c >= '\u2794' && c <= '\u2BFF') ||
			(c >= '\u2E00' && c <= '\u2E7F') || (c >= '\u3001' && c <= '\u3003') || (c >= '\u3008' && c <= '\u3020') || c == '\u0060' || c == '\u00A9' ||
			c == '\u00AB' || c == '\u00AC' || c == '\u00AE' || c == '\u00B0' || c == '\u00B1' || c == '\u00B6' || c == '\u00BB' || c == '\u00BF' ||
			c == '\u00D7' || c == '\u00F7' || c == '\u3030' || c == '\uFD3E' || c == '\uFD3F' || c == '\uFE45' || c == '\uFE46';
	}
}
