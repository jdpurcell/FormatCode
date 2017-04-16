﻿// --------------------------------------------------------------------------------
// Copyright (c) J.D. Purcell
//
// Licensed under the MIT License (see LICENSE.txt)
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FormatCode {
	public class CodeFormatter {
		private static readonly string[] _autoGeneratedPreprocessorDirectives = new[] {
			"#region Component Designer generated code",
			"#region Windows Form Designer generated code"
		};
		private static readonly string[] _autoGeneratedComments = new[] {
			"// <auto-generated>"
		};

		public int TabSize { get; set; } = 4;

		public TabStyle TabStyle { get; set; } = TabStyle.Tabs;

		public bool MoveOpenBracesUp { get; set; }

		public bool RequireNewLineAtEnd { get; set; }

		public bool PreserveNewLineType { get; set; }

		public bool PreserveTrailingSpacesInComments { get; set; }

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
			List<OutputLine> lines = new List<OutputLine>();
			LineInfo prevLineInfo = null;
			Context currentContext = new NormalContext();
			Stack<Context> contexts = new Stack<Context>(new[] { currentContext });
			bool isInInterpolatedStringFormatSection = false;
			int tabbedIndentationCount = 0;
			int spacedIndentationCount = 0;

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
				LineInfo lineInfo = new LineInfo();

				while (Peek(0) == ' ' || Peek(0) == '\t') {
					bool isTab = Peek(0) == '\t';
					lineInfo.IndentationSize += isTab ? (TabSize - (lineInfo.IndentationSize % 4)) : 1;
					lineInfo.IndentationContainsTabs |= isTab;
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
						lineInfo.IsPreprocessorDirective = true;
					}
					else if (firstChar == '/' && Peek(1) == '/') { // Single line comment
						if (CodeStartsWithAnyIgnoreCase(_autoGeneratedComments)) return;
						lineInfo.EndsWithXmlDocComment = Peek(2) == '/' && Peek(3) != '/';
						lineInfo.EndsWithSingleLineComment = !lineInfo.EndsWithXmlDocComment;
						i += lineInfo.EndsWithXmlDocComment ? 3 : 2;
						while (Peek(0) != '\n') i++;
					}
					else if (firstChar == '/' && Peek(1) == '*') { // Multi line comment
						i += 2;
						while (!(Peek(0) == '*' && Peek(1) == '/')) i++;
						i += 2;
						lineInfo.EndsWithMultiLineComment = Enumerable.Range(0, Int32.MaxValue).Select(n => Peek(n)).TakeWhile(c => c != '\n').Any(c => c != '\t' && c != ' ');
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

				string lineSubstance = code.Substring(lineSubstanceStart, i - lineSubstanceStart);
				if (!PreserveTrailingSpacesInComments || (!lineInfo.EndsWithSingleLineComment && !lineInfo.EndsWithXmlDocComment)) {
					lineSubstance = lineSubstance.TrimEnd(' ', '\t');
				}
				lineInfo.EndsWithOpenBrace = !lineInfo.EndsWithComment && !lineInfo.IsPreprocessorDirective && lineSubstance.EndsWith("{");
				lineInfo.EndsWithCloseBrace = !lineInfo.EndsWithComment && !lineInfo.IsPreprocessorDirective && lineSubstance.EndsWith("}");
				lineInfo.EndsWithSemicolon = !lineInfo.EndsWithComment && !lineInfo.IsPreprocessorDirective && lineSubstance.EndsWith(";");
				lineInfo.EndsWithComma = !lineInfo.EndsWithComment && !lineInfo.IsPreprocessorDirective && lineSubstance.EndsWith(",");
				lineInfo.IsEmpty = lineSubstance.Length == 0;
				lineInfo.IsEmptyAfterEndingWithOpenBrace = prevLineInfo != null && lineInfo.IsEmpty && prevLineInfo.EndsWithOpenBrace;
				bool skippedLine = false;
				OutputLine line = new OutputLine {
					Substance = lineSubstance,
					IndentationSize = lineInfo.IsEmpty ? 0 : lineInfo.IndentationSize
				};

				if (MoveOpenBracesUp && prevLineInfo != null && lineSubstance == "{" && lineInfo.IndentationSize == prevLineInfo.IndentationSize &&
					!prevLineInfo.IsEmpty && !prevLineInfo.EndsWithComment && !prevLineInfo.IsPreprocessorDirective && !prevLineInfo.EndsWithCloseBrace &&
					!prevLineInfo.EndsWithSemicolon && !prevLineInfo.EndsWithComma)
				{
					lines[lines.Count - 1].Substance += " {";
				}
				else if (prevLineInfo != null && lineInfo.IsEmpty && prevLineInfo.IsEmpty) {
					skippedLine = true;
				}
				else if (prevLineInfo != null && lineInfo.IsEmpty && prevLineInfo.EndsWithXmlDocComment) {
					skippedLine = true;
				}
				else if (prevLineInfo != null && !lineSubstance.StartsWith("}") && prevLineInfo.IsEmptyAfterEndingWithOpenBrace) {
					lines[lines.Count - 1] = line;
				}
				else if (prevLineInfo != null && lineSubstance.StartsWith("}") && prevLineInfo.IsEmpty && !prevLineInfo.IsEmptyAfterEndingWithOpenBrace) {
					lines[lines.Count - 1] = line;
				}
				else {
					lines.Add(line);
				}

				if (lineInfo.IndentationSize != 0) {
					if (lineInfo.IndentationContainsTabs)
						tabbedIndentationCount++;
					else
						spacedIndentationCount++;
				}

				i++;
				if (!skippedLine) {
					prevLineInfo = lineInfo;
				}
			}

			if (contexts.Count != 1) {
				throw new Exception("Detected incomplete verbatim interpolated string.");
			}

			if (lines.Count != 0 && lines[lines.Count - 1].Substance.Length == 0) {
				lines.RemoveAt(lines.Count - 1);
			}

			StringBuilder newCodeSB = new StringBuilder(codeStrRaw.Length);
			bool tabsAsSpaces = TabStyle == TabStyle.Spaces || (TabStyle == TabStyle.Detect && spacedIndentationCount > tabbedIndentationCount);
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
				AppendIndentation(lines[iLine].IndentationSize);
				foreach (char c in lines[iLine].Substance) {
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
			if (HasUTF8Sequence(bytes))
				return new UTF8Encoding(false);
			return null;
		}

		private static BOMType GetBOMType(byte[] bytes) {
			if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) return BOMType.UTF8;
			if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) return BOMType.UTF16LE;
			if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) return BOMType.UTF16BE;
			return BOMType.None;
		}

		private static bool HasUTF8Sequence(byte[] bytes) {
			for (int i = 0; i < bytes.Length; i++) {
				if ((bytes[i] & 0x80) == 0) continue;
				int extra = bytes.Length - i - 1;
				bool IsContinuation(int offset) => (bytes[i + offset] & 0b11000000) == 0b10000000;
				if ((bytes[i] & 0b11100000) == 0b11000000 && extra >= 1 && IsContinuation(1) &&
					(bytes[i] & 0b00011110) != 0)
					return true;
				if ((bytes[i] & 0b11110000) == 0b11100000 && extra >= 2 && IsContinuation(1) && IsContinuation(2) &&
					(bytes[i] & 0b00001111 | bytes[i + 1] & 0b00100000) != 0)
					return true;
				if ((bytes[i] & 0b11111000) == 0b11110000 && extra >= 3 && IsContinuation(1) && IsContinuation(2) && IsContinuation(3) &&
					(bytes[i] & 0b00000111 | bytes[i + 1] & 0b00110000) != 0)
					return true;
				if ((bytes[i] & 0b11111100) == 0b11111000 && extra >= 4 && IsContinuation(1) && IsContinuation(2) && IsContinuation(3) && IsContinuation(4) &&
					(bytes[i] & 0b00000011 | bytes[i + 1] & 0b00111000) != 0)
					return true;
				if ((bytes[i] & 0b11111110) == 0b11111100 && extra >= 5 && IsContinuation(1) && IsContinuation(2) && IsContinuation(3) && IsContinuation(4) && IsContinuation(5) &&
					(bytes[i] & 0b00000001 | bytes[i + 1] & 0b00111100) != 0)
					return true;
			}
			return false;
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
				 select (NewLineType?)s.Type).FirstOrDefault() ?? NewLineType.None;
			return new string(dst, 0, iDst);
		}

		private class LineInfo {
			public int IndentationSize { get; set; }
			public bool IndentationContainsTabs { get; set; }
			public bool EndsWithSingleLineComment { get; set; }
			public bool EndsWithXmlDocComment { get; set; }
			public bool EndsWithMultiLineComment { get; set; }
			public bool IsPreprocessorDirective { get; set; }
			public bool EndsWithOpenBrace { get; set; }
			public bool EndsWithCloseBrace { get; set; }
			public bool EndsWithSemicolon { get; set; }
			public bool EndsWithComma { get; set; }
			public bool IsEmpty { get; set; }
			public bool IsEmptyAfterEndingWithOpenBrace { get; set; }

			public bool EndsWithComment =>
				EndsWithSingleLineComment ||
				EndsWithXmlDocComment ||
				EndsWithMultiLineComment;
		}

		private class OutputLine {
			public string Substance { get; set; }
			public int IndentationSize { get; set; }
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
}
