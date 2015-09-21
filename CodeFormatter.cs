﻿// ---------------------------------------------------------------------------
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
using System.Text;

namespace FormatCode {
	public class CodeFormatter {
		public CodeFormatter() {
			TabSize = 4;
			MoveOpenBracesUp = false;
			RequireNewLineAtEnd = false;
		}

		public int TabSize { get; set; }

		public bool MoveOpenBracesUp { get; set; }

		public bool RequireNewLineAtEnd { get; set; }

		public void Format(string path) {
			byte[] codeBytes = File.ReadAllBytes(path);
			Encoding encoding = DetectEncoding(codeBytes) ?? Encoding.Default;
			string codeStrRaw = encoding.GetString(codeBytes);
			if (!AreByteArraysEqual(codeBytes, encoding.GetBytes(codeStrRaw))) {
				throw new Exception("Misdetected character encoding!");
			}
			string code = new StringBuilder(codeStrRaw).Replace("\r\n", "\n").Replace("\r", "\n").ToString();
			bool fileEndsWithLineEnd = code.Length >= 1 && code[code.Length - 1] == '\n';
			int i = 0;
			List<string> lines = new List<string>();
			LineInfo prevLineInfo = null;

			if (code.IndexOf("#region Component Designer generated code", StringComparison.OrdinalIgnoreCase) != -1 ||
				code.IndexOf("#region Windows Form Designer generated code", StringComparison.OrdinalIgnoreCase) != -1 ||
				code.IndexOf("// <auto-generated>", StringComparison.OrdinalIgnoreCase) != -1)
			{
				return;
			}

			Func<int, char> peek = (offset) => {
				int index = i + offset;
				return index >= 0 && index < code.Length ? code[index] : '\n';
			};

			while (i < code.Length) {
				LineInfo lineInfo = new LineInfo();

				while (peek(0) == ' ' || peek(0) == '\t') {
					lineInfo.LeadingWhitespaceCount += peek(0) == '\t' ? (TabSize - (lineInfo.LeadingWhitespaceCount % 4)) : 1;
					i++;
				}

				int lineBeefStart = i;

				while (peek(0) != '\n') {
					if (i == lineBeefStart && peek(0) == '#') { // Preprocessor directive
						i++;
						while (peek(0) != '\n') i++;
						lineInfo.IsPreprocessorDirective = true;
					}
					else if (peek(0) == '/' && peek(1) == '/') { // Single line comment
						lineInfo.EndsWithXmlDocComment = peek(2) == '/' && peek(3) != '/';
						i += lineInfo.EndsWithXmlDocComment ? 3 : 2;
						while (peek(0) != '\n') i++;
						lineInfo.EndsWithComment = true;
					}
					else if (peek(0) == '/' && peek(1) == '*') { // Multi line comment
						i += 2;
						while (!(peek(0) == '*' && peek(1) == '/')) i++;
						i += 2;
						lineInfo.EndsWithComment = true;
					}
					else if (peek(0) == '@' && peek(1) == '"') { // Verbatim string literal
						i += 2;
						while (!(peek(0) == '"' && peek(1) != '"')) i += peek(0) == '"' ? 2 : 1;
						i++;
					}
					else if (peek(0) == '"') { // String literal
						i++;
						while (peek(0) != '"') i += peek(0) == '\\' ? 2 : 1;
						i++;
					}
					else if (peek(0) == '\'') { // Character literal
						i++;
						while (peek(0) != '\'') i += peek(0) == '\\' ? 2 : 1;
						i++;
					}
					else if (peek(0) == '\t' || peek(0) == ' ') {
						i++;
					}
					else {
						i++;
						lineInfo.EndsWithComment = false;
					}
				}

				string lineSubstance = code.Substring(lineBeefStart, i - lineBeefStart).TrimEnd(' ', '\t');
				lineInfo.EndsWithOpenBrace = !lineInfo.EndsWithComment && !lineInfo.IsPreprocessorDirective && lineSubstance.EndsWith("{");
				lineInfo.EndsWithCloseBrace = !lineInfo.EndsWithComment && !lineInfo.IsPreprocessorDirective && lineSubstance.EndsWith("}");
				lineInfo.EndsWithSemicolon = !lineInfo.EndsWithComment && !lineInfo.IsPreprocessorDirective && lineSubstance.EndsWith(";");
				lineInfo.EndsWithComma = !lineInfo.EndsWithComment && !lineInfo.IsPreprocessorDirective && lineSubstance.EndsWith(",");
				lineInfo.IsEmpty = lineSubstance.Length == 0;
				lineInfo.IsEmptyAfterEndingWithOpenBrace = prevLineInfo != null && lineInfo.IsEmpty && prevLineInfo.EndsWithOpenBrace;
				bool skippedLine = false;
				string line = lineInfo.IsEmpty ? "" :
					new string('\t', lineInfo.LeadingWhitespaceCount / 4) +
					new string(' ', lineInfo.LeadingWhitespaceCount % 4) +
					lineSubstance;

				if (MoveOpenBracesUp && prevLineInfo != null && lineSubstance == "{" && lineInfo.LeadingWhitespaceCount == prevLineInfo.LeadingWhitespaceCount &&
					!prevLineInfo.IsEmpty && !prevLineInfo.EndsWithComment && !prevLineInfo.IsPreprocessorDirective && !prevLineInfo.EndsWithCloseBrace &&
					!prevLineInfo.EndsWithSemicolon && !prevLineInfo.EndsWithComma)
				{
					lines[lines.Count - 1] += " {";
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

				i++;
				if (!skippedLine) {
					prevLineInfo = lineInfo;
				}
			}

			if (lines.Count != 0 && lines[lines.Count - 1].Length == 0) {
				lines.RemoveAt(lines.Count - 1);
			}

			StringBuilder newCodeSB = new StringBuilder(String.Join("\n", lines));
			if (RequireNewLineAtEnd || fileEndsWithLineEnd) {
				newCodeSB.Append("\n");
			}
			newCodeSB = newCodeSB.Replace("\n", Environment.NewLine);
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

			while (iA < strA.Length && Char.IsWhiteSpace(strA, iA)) iA++;
			while (iB < strB.Length && Char.IsWhiteSpace(strB, iB)) iB++;

			while (iA < strA.Length && iB < strB.Length) {
				if (strA[iA++] != strB[iB++]) return false;

				while (iA < strA.Length && Char.IsWhiteSpace(strA, iA)) iA++;
				while (iB < strB.Length && Char.IsWhiteSpace(strB, iB)) iB++;
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
				int rem = bytes.Length - i - 1;
				if ((bytes[i] & 0xE0) == 0xC0 && rem >= 1 && (bytes[i + 1] & 0xC0) == 0x80 &&
					(bytes[i] & 0x1E) != 0)
					return true;
				if ((bytes[i] & 0xF0) == 0xE0 && rem >= 2 && (bytes[i + 1] & 0xC0) == 0x80 && (bytes[i + 2] & 0xC0) == 0x80 &&
					(bytes[i] & 0x0F | bytes[i + 1] & 0x20) != 0)
					return true;
				if ((bytes[i] & 0xF8) == 0xF0 && rem >= 3 && (bytes[i + 1] & 0xC0) == 0x80 && (bytes[i + 2] & 0xC0) == 0x80 && (bytes[i + 3] & 0xC0) == 0x80 &&
					(bytes[i] & 0x07 | bytes[i + 1] & 0x30) != 0)
					return true;
				if ((bytes[i] & 0xFC) == 0xF8 && rem >= 4 && (bytes[i + 1] & 0xC0) == 0x80 && (bytes[i + 2] & 0xC0) == 0x80 && (bytes[i + 3] & 0xC0) == 0x80 && (bytes[i + 4] & 0xC0) == 0x80 &&
					(bytes[i] & 0x03 | bytes[i + 1] & 0x38) != 0)
					return true;
				if ((bytes[i] & 0xFE) == 0xFC && rem >= 5 && (bytes[i + 1] & 0xC0) == 0x80 && (bytes[i + 2] & 0xC0) == 0x80 && (bytes[i + 3] & 0xC0) == 0x80 && (bytes[i + 4] & 0xC0) == 0x80 && (bytes[i + 5] & 0xC0) == 0x80 &&
					(bytes[i] & 0x01 | bytes[i + 1] & 0x3C) != 0)
					return true;
			}
			return false;
		}

		private class LineInfo {
			public int LeadingWhitespaceCount;
			public bool EndsWithComment;
			public bool EndsWithXmlDocComment;
			public bool IsPreprocessorDirective;
			public bool EndsWithOpenBrace;
			public bool EndsWithCloseBrace;
			public bool EndsWithSemicolon;
			public bool EndsWithComma;
			public bool IsEmpty;
			public bool IsEmptyAfterEndingWithOpenBrace;
		}

		private enum BOMType {
			None = 0,
			UTF8 = 1,
			UTF16LE = 2,
			UTF16BE = 3
		}
	}
}
