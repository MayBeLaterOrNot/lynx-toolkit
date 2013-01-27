// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CsSyntaxHighlighter.cs" company="Lynx Toolkit">
//   The MIT License (MIT)
//   
//   Copyright (c) 2012 Oystein Bjorke
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary>
//   Provides a simple C# syntax highlighter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LynxToolkit.Documents
{
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides a simple C# syntax highlighter.
    /// </summary>
    /// <remarks>
    /// This will not highlight exactly as in Visual Studio, but close... I think a real lexer/parser is necessary to
    /// do the job properly. There are probably libraries that does a better job than this, but I don't want to add
    /// a dependency, and currently this is 'good enough', I think.
    /// </remarks>
    public static class CsSyntaxHighlighter
    {
        /// <summary>
        /// The expression
        /// </summary>
        private static readonly Regex Expression;

        /// <summary>
        /// Initializes static members of the <see cref="CsSyntaxHighlighter"/> class. 
        /// </summary>
        static CsSyntaxHighlighter()
        {
            var keywords =
                "abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixe|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|async|await|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield";
            var b = new StringBuilder();
            b.Append("(?:");
            b.Append("  (?<string>  @?\"(?:\"\"|\\\\\"|[^\"])*\" )");
            b.Append("| (?<keyword> \\b(?:" + keywords + ")\\b )");
            b.Append("| (?<doc> ^\\s*(///.*?)$ )");
            b.Append("| (?<commentblock> /\\*.*?\\*/ )");
            b.Append("| (?<comment> //.*?$ )");
            b.Append(")");
            Expression = new Regex(
                b.ToString(),
                RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        }

        /// <summary>
        /// Applies C# syntax highlighting to the specified text.
        /// </summary>
        /// <param name="text">
        /// The encoded html.
        /// </param>
        /// <returns>
        /// Html with highlight elements.
        /// </returns>
        public static string Highlight(string text)
        {
            return Expression.Replace(text, ReplaceMatch);
        }

        /// <summary>
        /// Replaces the match.
        /// </summary>
        /// <param name="match">
        /// The match.
        /// </param>
        /// <returns>
        /// The replacement string.
        /// </returns>
        private static string ReplaceMatch(Match match)
        {
            var stringGroup = match.Groups["string"];
            if (stringGroup.Success)
            {
                return "<span class=\"string\">" + stringGroup.Value + "</span>";
            }

            var keywordGroup = match.Groups["keyword"];
            if (keywordGroup.Success)
            {
                return "<span class=\"keyword\">" + keywordGroup.Value + "</span>";
            }

            var commentBlockGroup = match.Groups["commentblock"];
            if (commentBlockGroup.Success)
            {
                return "<span class=\"comment\">" + commentBlockGroup.Value + "</span>";
            }

            var commentGroup = match.Groups["comment"];
            if (commentGroup.Success)
            {
                return "<span class=\"comment\">" + commentGroup.Value + "</span>";
            }

            var docGroup = match.Groups["doc"];
            if (docGroup.Success)
            {
                return "<span class=\"doc\">" + docGroup.Value + "</span>";
            }

            return match.Value;
        }
    }
}