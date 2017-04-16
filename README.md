# FormatCode
This is for C# only, and focuses primarily on whitespace cleanup:
* Normalizes indentation (tabs or spaces).
* Trims trailing whitespace.
* Removes duplicate blank lines (i.e. 2 or more consecutive blank lines are squashed to 1 line).
* Removes blank lines between an opening brace and the line that follows.
* Removes blank lines between a line and the closing brace that follows.
* Removes blank lines between XML documentation comments and the code that follows.
* Forces a newline at the end of the file (optional, disabled by default).
* Normalizes all of the newlines (optional, enabled by default).

It understands C# syntax and will not touch anything inside string literals or comments, aside from normalizing the newlines, and trimming trailing whitespace from single-line comments.
