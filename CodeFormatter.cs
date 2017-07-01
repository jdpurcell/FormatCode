﻿// --------------------------------------------------------------------------------
// Copyright (c) J.D. Purcell
//
// Licensed under the MIT License (see LICENSE.txt)
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FormatCode {
	public class CodeFormatter {
		private static readonly string[] _autoGeneratedPreprocessorDirectives = {
			"#region Component Designer generated code",
			"#region Windows Form Designer generated code"
		};
		private static readonly string[] _autoGeneratedComments = {
			"// <auto-generated>"
		};

		public int TabSize { get; set; } = 4;

		public TabStyle TabStyle { get; set; } = TabStyle.Tabs;

		public OpenBraceStyle OpenBraceStyle { get; set; } = OpenBraceStyle.LeaveAlone;

		public bool RequireNewLineAtEnd { get; set; }

		public bool PreserveNewLineType { get; set; }

		public bool LeaveTrailingWhitespaceInCode { get; set; }

		public bool LeaveTrailingWhitespaceInComments { get; set; }

		public bool LeaveEmptyLines { get; set; }

		public void Format(string path) {
			const char bomChar = '\uFEFF';
			byte[] codeBytes = File.ReadAllBytes(path);
			Encoding encoding = DetectEncoding(codeBytes) ?? Encoding.Default;
			string codeStrRaw = encoding.GetString(codeBytes);
			if (!AreByteArraysEqual(codeBytes, encoding.GetBytes(codeStrRaw))) {
				throw new Exception("Misdetected character encoding!");
			}
			string code = NormalizeNewLines(codeStrRaw, out NewLineType inputNewLineType);
			string outputNewLine = !PreserveNewLineType ? Environment.NewLine :
				inputNewLineType == NewLineType.CRLF ? "\r\n" :
				inputNewLineType == NewLineType.LF ? "\n" :
				inputNewLineType == NewLineType.CR ? "\r" :
				Environment.NewLine;
			bool fileEndsWithLineEnd = code.Length >= 1 && code[code.Length - 1] == '\n';
			int i = 0;
			List<Line> lines = new List<Line>();
			Context currentContext = new NormalContext();
			Stack<Context> contexts = new Stack<Context>(new[] { currentContext });
			bool isInInterpolatedStringFormatSection = false;

			char Peek(int offset) {
				const int maxSpill = 1024;
				int index = i + offset;
				if (index >= 0 && index < code.Length) {
					return code[index];
				}
				if (index >= -maxSpill && index < code.Length + maxSpill) {
					return '\n';
				}
				throw new Exception("Detected unterminated sequence.");
			}
			bool CodeStartsWithAnyIgnoreCase(string[] values) => values.Any(v => String.Compare(code, i, v, 0, v.Length, StringComparison.OrdinalIgnoreCase) == 0);

			void PushContext(Context context) {
				currentContext = context;
				contexts.Push(context);
			}
			void PopContext() {
				contexts.Pop();
				currentContext = contexts.Peek();
			}
			Context PreviousContext() => contexts.ElementAtOrDefault(1);

			bool hasBOM = Peek(0) == bomChar;
			if (hasBOM) {
				i++;
			}

			while (i < code.Length) {
				Line line = new Line(this);

				while (Peek(0) == ' ' || Peek(0) == '\t') {
					bool isTab = Peek(0) == '\t';
					line.IndentationSize += isTab ? (TabSize - (line.IndentationSize % 4)) : 1;
					line.IndentationContainsTabs |= isTab;
					i++;
				}

				int lineSubstanceStart = i;

				char firstChar;
				while ((firstChar = Peek(0)) != '\n' || !(currentContext is NormalContext)) {
					if (currentContext is VerbatimInterpolatedStringContext || (firstChar == '$' && Peek(1) == '@' && Peek(2) == '"')) { // Verbatim interpolated string
						if (isInInterpolatedStringFormatSection) {
							while (!(Peek(0) == '}' && Peek(1) != '}')) i += Peek(0) == '}' ? 2 : 1;
							i++;
							isInInterpolatedStringFormatSection = false;
						}
						else {
							if (currentContext is NormalContext) {
								PushContext(new VerbatimInterpolatedStringContext());
								i += 3;
							}
							bool IsEndQuote() => Peek(0) == '"' && Peek(1) != '"';
							bool IsCodeSection() => Peek(0) == '{' && Peek(1) != '{';
							while (!IsEndQuote() && !IsCodeSection()) i += Peek(0) == '"' || Peek(0) == '{' ? 2 : 1;
							if (IsCodeSection()) {
								PushContext(new NormalContext());
							}
							else if (IsEndQuote()) {
								PopContext();
							}
							i++;
						}
					}
					else if (currentContext is InterpolatedStringContext || (firstChar == '$' && Peek(1) == '"')) { // Interpolated string
						if (isInInterpolatedStringFormatSection) {
							while (!(Peek(0) == '}' && Peek(1) != '}')) i += Peek(0) == '}' ? 2 : 1;
							i++;
							isInInterpolatedStringFormatSection = false;
						}
						else {
							if (currentContext is NormalContext) {
								PushContext(new InterpolatedStringContext());
								i += 2;
							}
							bool IsEndQuote() => Peek(0) == '"';
							bool IsCodeSection() => Peek(0) == '{' && Peek(1) != '{';
							while (!IsEndQuote() && !IsCodeSection()) i += Peek(0) == '\\' || Peek(0) == '{' ? 2 : 1;
							if (IsCodeSection()) {
								PushContext(new NormalContext());
							}
							else if (IsEndQuote()) {
								PopContext();
							}
							i++;
						}
					}
					else if (firstChar == '#' && i == lineSubstanceStart && contexts.Count == 1) { // Preprocessor directive
						if (CodeStartsWithAnyIgnoreCase(_autoGeneratedPreprocessorDirectives)) return;
						i++;
						while (Peek(0) != '\n') i++;
						line.IsPreprocessorDirective = true;
					}
					else if (firstChar == '/' && Peek(1) == '/') { // Single line comment
						if (CodeStartsWithAnyIgnoreCase(_autoGeneratedComments)) return;
						line.EndsWithXmlDocComment = Peek(2) == '/' && Peek(3) != '/';
						line.EndsWithSingleLineComment = !line.EndsWithXmlDocComment;
						i += line.EndsWithXmlDocComment ? 3 : 2;
						while (Peek(0) != '\n') i++;
					}
					else if (firstChar == '/' && Peek(1) == '*') { // Multi line comment
						i += 2;
						while (!(Peek(0) == '*' && Peek(1) == '/')) i++;
						i += 2;
						line.EndsWithMultiLineComment = Enumerable.Range(0, Int32.MaxValue).Select(n => Peek(n)).TakeWhile(c => c != '\n').Any(c => c != '\t' && c != ' ');
					}
					else if (firstChar == '@' && Peek(1) == '"') { // Verbatim string literal
						i += 2;
						while (!(Peek(0) == '"' && Peek(1) != '"')) i += Peek(0) == '"' ? 2 : 1;
						i++;
					}
					else if (firstChar == '"') { // String literal
						i++;
						while (Peek(0) != '"') i += Peek(0) == '\\' ? 2 : 1;
						i++;
					}
					else if (firstChar == '\'') { // Character literal
						i++;
						while (Peek(0) != '\'') i += Peek(0) == '\\' ? 2 : 1;
						i++;
					}
					else if (firstChar == '{' && contexts.Count > 1) {
						i++;
						currentContext.BlockDepth++;
					}
					else if (firstChar == '}' && contexts.Count > 1) {
						i++;
						if (--currentContext.BlockDepth == -1) {
							if (currentContext.ParenDepth != 0) {
								throw new Exception("Detected parentheses mismatch.");
							}
							PopContext();
						}
					}
					else if (firstChar == '(' && contexts.Count > 1) {
						i++;
						currentContext.ParenDepth++;
					}
					else if (firstChar == ')' && contexts.Count > 1) {
						i++;
						if (--currentContext.ParenDepth == -1) {
							throw new Exception("Detected parentheses mismatch.");
						}
					}
					else if (firstChar == ':' && currentContext.ParenDepth == 0 && (PreviousContext() is InterpolatedStringContext || PreviousContext() is VerbatimInterpolatedStringContext)) {
						i++;
						PopContext();
						isInInterpolatedStringFormatSection = true;
					}
					else {
						i++;
					}
				}

				if (contexts.Count > 1 && contexts.OfType<InterpolatedStringContext>().Any()) {
					throw new Exception("Detected incomplete interpolated string.");
				}

				line.SetSubstanceRaw(code.Substring(lineSubstanceStart, i - lineSubstanceStart));
				lines.Add(line);
				i++;
			}

			if (contexts.Count != 1) {
				throw new Exception("Detected incomplete verbatim interpolated string.");
			}

			ProcessLines(lines);

			StringBuilder newCodeSB = new StringBuilder(codeStrRaw.Length);
			IEnumerable<Line> indentedLines = lines.Where(l => l.IndentationSize != 0);
			bool tabsAsSpaces = TabStyle == TabStyle.Spaces ||
				(TabStyle == TabStyle.Detect && indentedLines.Count(l => !l.IndentationContainsTabs) > indentedLines.Count(l => l.IndentationContainsTabs));
			void AppendIndentation(int size) {
				if (tabsAsSpaces) {
					newCodeSB.Append(' ', size);
				}
				else {
					newCodeSB.Append('\t', size / 4);
					newCodeSB.Append(' ',  size % 4);
				}
			}
			if (hasBOM) {
				newCodeSB.Append(bomChar);
			}
			for (int iLine = 0; iLine < lines.Count; iLine++) {
				if (iLine != 0) newCodeSB.Append(outputNewLine);
				AppendIndentation(lines[iLine].OutputIndentationSize);
				foreach (char c in lines[iLine].OutputSubstance) {
					if (c != '\n')
						newCodeSB.Append(c);
					else
						newCodeSB.Append(outputNewLine);
				}
			}
			if (RequireNewLineAtEnd || fileEndsWithLineEnd) {
				newCodeSB.Append(outputNewLine);
			}
			string newCode = newCodeSB.ToString();
			if (!StringEqualsIgnoreWhitespace(code, newCode)) {
				throw new Exception("Code contents changed!");
			}
			byte[] newCodeBytes = encoding.GetBytes(newCode);
			if (!AreByteArraysEqual(codeBytes, newCodeBytes)) {
				File.WriteAllBytes(path, newCodeBytes);
			}
		}

		private void ProcessLines(List<Line> lines) {
			int iLine = 0;
			bool shouldRemoveCurrentLine;

			Line PeekLine(int offset) {
				int idx = iLine + offset;
				return idx >= 0 && idx < lines.Count ? lines[idx] : null;
			}
			void FlagCurrentLineForRemoval() {
				shouldRemoveCurrentLine = true;
			}
			void InsertLineAfterCurrent(Line line) {
				lines.Insert(iLine + 1, line);
			}

			while (iLine < lines.Count) {
				shouldRemoveCurrentLine = false;
				ProcessLine(lines[iLine], PeekLine, FlagCurrentLineForRemoval, InsertLineAfterCurrent);
				if (shouldRemoveCurrentLine) {
					lines.RemoveAt(iLine);
				}
				else {
					iLine++;
				}
			}
		}

		private void ProcessLine(Line line, Func<int, Line> peekLine, Action flagCurrentLineForRemoval, Action<Line> insertLineAfterCurrent) {
			Line prevLine = peekLine(-1);
			Line nextLine = peekLine(1);

			int nextLineOffset = 1;
			void GetNextLine() { nextLine = peekLine(++nextLineOffset); }

			if (!LeaveEmptyLines && line.IsEmpty) {
				while (nextLine != null && nextLine.IsEmpty) {
					GetNextLine();
				}
				bool isDuplicate = prevLine?.IsEmpty ?? false;
				bool isAfterXmlDocComment = prevLine?.EndsWithXmlDocComment ?? false;
				bool isAfterOpenBrace = prevLine?.CodeEndsWith('{') ?? false;
				bool isBeforeCloseBrace = nextLine?.CodeStartsWith('}') ?? false;
				bool isLastLine = nextLine == null;
				if (isAfterOpenBrace && isBeforeCloseBrace) {
					// An empty line is okay here
				}
				else if (isDuplicate || isAfterXmlDocComment || isAfterOpenBrace || isBeforeCloseBrace || isLastLine) {
					flagCurrentLineForRemoval();
				}
			}
			else if (OpenBraceStyle == OpenBraceStyle.MoveUp && line.Substance == "{" && prevLine != null && line.IndentationSize == prevLine.IndentationSize &&
				!prevLine.IsEmpty && !prevLine.EndsWithComment && !prevLine.IsPreprocessorDirective && !prevLine.CodeEndsWith('}') &&
				!prevLine.CodeEndsWith(';') && !prevLine.CodeEndsWith(','))
			{
				prevLine.SetSubstanceRaw(prevLine.SubstanceRaw + " {");
				flagCurrentLineForRemoval();
			}
			else if (OpenBraceStyle == OpenBraceStyle.MoveDown && line.Substance.Length > 1 && line.CodeEndsWith('{')) {
				while (nextLine != null && (nextLine.IsEmpty || nextLine.IsPreprocessorDirective)) {
					GetNextLine();
				}
				if (nextLine != null) {
					line.SetSubstanceRaw(line.Substance.Substring(0, line.Substance.Length - 1).TrimEnd(' ', '\t'));

					bool matchIndentation = nextLine.CodeStartsWith('}') || StartsWithLabel(nextLine.Substance);
					Line insertLine = new Line(this);
					insertLine.IndentationSize = Math.Max(matchIndentation ? nextLine.IndentationSize : nextLine.IndentationSize - TabSize, line.IndentationSize);
					insertLine.IndentationContainsTabs = nextLine.IndentationContainsTabs;
					insertLine.SetSubstanceRaw("{");
					insertLineAfterCurrent(insertLine);
				}
			}
		}

		private static bool AreByteArraysEqual(byte[] a, byte[] b) {
			if (a.Length != b.Length) return false;
			for (int i = 0; i < a.Length; i++) {
				if (a[i] != b[i]) return false;
			}
			return true;
		}

		private static bool StringEqualsIgnoreWhitespace(string strA, string strB) {
			int iA = 0;
			int iB = 0;

			bool IsWhitespace(char c) => c == ' ' || c == '\t' || c == '\r' || c == '\n';

			while (iA < strA.Length && IsWhitespace(strA[iA])) iA++;
			while (iB < strB.Length && IsWhitespace(strB[iB])) iB++;

			while (iA < strA.Length && iB < strB.Length) {
				if (strA[iA++] != strB[iB++]) return false;

				while (iA < strA.Length && IsWhitespace(strA[iA])) iA++;
				while (iB < strB.Length && IsWhitespace(strB[iB])) iB++;
			}

			return iA == strA.Length && iB == strB.Length;
		}

		private static Encoding DetectEncoding(byte[] bytes) {
			BOMType bomType = GetBOMType(bytes);
			if (bomType == BOMType.UTF8)
				return new UTF8Encoding(true);
			if (bomType == BOMType.UTF16LE)
				return new UnicodeEncoding(false, true);
			if (bomType == BOMType.UTF16BE)
				return new UnicodeEncoding(true, true);
			if (IsStronglyUTF8(bytes))
				return new UTF8Encoding(false);
			return null;
		}

		private static BOMType GetBOMType(byte[] bytes) {
			if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) return BOMType.UTF8;
			if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) return BOMType.UTF16LE;
			if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) return BOMType.UTF16BE;
			return BOMType.None;
		}

		private static bool IsStronglyUTF8(byte[] bytes) {
			bool sawUTF8Sequence = false;
			int i = 0;
			while (i < bytes.Length) {
				if (bytes[i] <= 0x7F)
				{ i++; continue; }
				int extra = bytes.Length - i - 1;
				bool IsContinuation(int offset) => (bytes[i + offset] & 0b11000000) == 0b10000000;
				if ((bytes[i] & 0b11100000) == 0b11000000 && extra >= 1 && IsContinuation(1) &&
					(bytes[i] & 0b00011110) != 0)
				{ i += 2; sawUTF8Sequence = true; continue; }
				if ((bytes[i] & 0b11110000) == 0b11100000 && extra >= 2 && IsContinuation(1) && IsContinuation(2) &&
					(bytes[i] & 0b00001111 | bytes[i + 1] & 0b00100000) != 0)
				{ i += 3; sawUTF8Sequence = true; continue; }
				if ((bytes[i] & 0b11111000) == 0b11110000 && extra >= 3 && IsContinuation(1) && IsContinuation(2) && IsContinuation(3) &&
					(bytes[i] & 0b00000111 | bytes[i + 1] & 0b00110000) != 0)
				{ i += 4; sawUTF8Sequence = true; continue; }
				return false;
			}
			return sawUTF8Sequence;
		}

		private static string NormalizeNewLines(string src, out NewLineType detectedNewLineType) {
			char[] dst = new char[src.Length];
			int iDst = 0;
			int crlfCount = 0;
			int lfCount = 0;
			int crCount = 0;
			for (int iSrc = 0; iSrc < src.Length; iSrc++) {
				if (src[iSrc] == '\r') {
					if (iSrc < src.Length - 1 && src[iSrc + 1] == '\n') {
						crlfCount++;
						iSrc++;
					}
					else {
						crCount++;
					}
					dst[iDst++] = '\n';
				}
				else if (src[iSrc] == '\n') {
					lfCount++;
					dst[iDst++] = '\n';
				}
				else {
					dst[iDst++] = src[iSrc];
				}
			}
			var stats = new[] {
				new { Type = NewLineType.CRLF, Count = crlfCount },
				new { Type = NewLineType.LF, Count = lfCount },
				new { Type = NewLineType.CR, Count = crCount }
			};
			detectedNewLineType =
				(from s in stats
				 where s.Count != 0
				 orderby s.Count descending
				 select s.Type).DefaultIfEmpty(NewLineType.None).First();
			return new string(dst, 0, iDst);
		}

		private static bool StartsWithLabel(string s) {
			int p = s.IndexOf(':');
			return p != -1 && IsIdentifier(s.Substring(0, p));
		}

		private static bool IsIdentifier(string s) {
			bool IsStartCategory(UnicodeCategory uc) =>
				uc == UnicodeCategory.UppercaseLetter || uc == UnicodeCategory.LowercaseLetter || uc == UnicodeCategory.TitlecaseLetter ||
				uc == UnicodeCategory.ModifierLetter || uc == UnicodeCategory.OtherLetter || uc == UnicodeCategory.LetterNumber;

			bool IsPartCategory(UnicodeCategory uc) =>
				IsStartCategory(uc) || uc == UnicodeCategory.NonSpacingMark || uc == UnicodeCategory.SpacingCombiningMark ||
				uc == UnicodeCategory.DecimalDigitNumber || uc == UnicodeCategory.ConnectorPunctuation || uc == UnicodeCategory.Format;

			bool IsStartCharacter(char c) => c == '_' || IsStartCategory(Char.GetUnicodeCategory(c));

			bool IsPartCharacter(char c) => IsPartCategory(Char.GetUnicodeCategory(c));

			int i = 0;
			if (s.Length >= 1 && s[0] == '@') i++;
			if (i == s.Length) return false;
			if (!IsStartCharacter(s[i])) return false;
			i++;
			while (i < s.Length) {
				if (!IsPartCharacter(s[i])) return false;
				i++;
			}
			return true;
		}

		private class Line {
			private CodeFormatter Owner { get; }

			public Line(CodeFormatter owner) {
				Owner = owner;
			}

			public int IndentationSize { get; set; }
			public bool IndentationContainsTabs { get; set; }
			public bool EndsWithSingleLineComment { get; set; }
			public bool EndsWithXmlDocComment { get; set; }
			public bool EndsWithMultiLineComment { get; set; }
			public bool IsPreprocessorDirective { get; set; }
			public string Substance { get; private set; }
			public string SubstanceRaw { get; private set; }

			public void SetSubstanceRaw(string value) {
				SubstanceRaw = value;
				Substance = value.TrimEnd(' ', '\t');
			}

			public bool CodeStartsWith(char c) =>
				Substance.Length >= 1 &&
				Substance[0] == c;

			public bool CodeEndsWith(char c) =>
				!EndsWithComment &&
				!IsPreprocessorDirective &&
				Substance.Length >= 1 &&
				Substance[Substance.Length - 1] == c;

			public bool IsEmpty => Substance.Length == 0;

			public bool EndsWithComment =>
				EndsWithSingleLineComment ||
				EndsWithXmlDocComment ||
				EndsWithMultiLineComment;

			public string OutputSubstance => !LeaveTrailingWhitespace ? Substance : SubstanceRaw;

			public int OutputIndentationSize => !LeaveTrailingWhitespace && IsEmpty ? 0 : IndentationSize;

			private bool LeaveTrailingWhitespace =>
				EndsWithSingleLineComment || EndsWithXmlDocComment ? Owner.LeaveTrailingWhitespaceInComments : Owner.LeaveTrailingWhitespaceInCode;
		}

		private abstract class Context {
			public int BlockDepth { get; set; }
			public int ParenDepth { get; set; }
		}

		private class NormalContext : Context { }

		private class InterpolatedStringContext : Context { }

		private class VerbatimInterpolatedStringContext : Context { }

		private enum BOMType {
			None,
			UTF8,
			UTF16LE,
			UTF16BE
		}

		private enum NewLineType {
			None,
			CRLF,
			LF,
			CR
		}
	}

	public enum TabStyle {
		Tabs,
		Spaces,
		Detect
	}

	public enum OpenBraceStyle {
		LeaveAlone,
		MoveUp,
		MoveDown
	}
}
