using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FormatCode.Library;

public static class CodeFileFinder {
	private static readonly string[] _ignoreNames = { };
	private static readonly string[] _ignoreSuffixes = { };
	private static readonly string[] _ignoreDirectories = { "obj" };

	public static IEnumerable<string> Find(IList<string> dirsAndFiles) {
		char dirSep = Path.DirectorySeparatorChar;
		bool IsExcluded(string path) =>
			!Path.GetExtension(path).Equals(".cs", StringComparison.OrdinalIgnoreCase) ||
			_ignoreNames.Any(n => Path.GetFileName(path).Equals(n, StringComparison.OrdinalIgnoreCase)) ||
			_ignoreSuffixes.Any(s => path.EndsWith(s, StringComparison.OrdinalIgnoreCase)) ||
			_ignoreDirectories.Any(d => path.IndexOf(dirSep + d + dirSep, StringComparison.OrdinalIgnoreCase) != -1);

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
}
