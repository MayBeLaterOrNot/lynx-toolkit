// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CharacterReplacements.cs" company="Lynx Toolkit">
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
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit.Documents
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public static class CharacterReplacements
    {
        static readonly Regex EncodeExpressions;
        static readonly Regex DecodeExpressions;

        public static Dictionary<string, string> Replacements { get; private set; }

        static CharacterReplacements()
        {
            Replacements = new Dictionary<string, string>
                {
                    { "(C)", "©" },
                    { "(R)", "®" },
                    { "(TM)", "™" },
                    { "(c)", "©" },
                    { "(r)", "®" },
                    { "(tm)", "™" },
                    { "...", "…" },
                    { "<->", "↔" },
                    { "->", "→" },
                    { "<-", "←" },
                    { "<=>", "⇔" },
                    { "=>", "⇒" },
                    { "<=", "⇐" },
                    { "---", "—" },
                    { "--", "–" }
                };

            DecodeExpressions = CreateRegex(@"(?<=[^\\]|^)     # Not escaped
(?:
(?<x>(?<=\d+\s*)x(?=\s*\d+))
| (?<esc>\\)
)");
            EncodeExpressions = CreateRegex(@"(?<=[^\\]|^)     # Not escaped
(?:
(?<x>(?<=\d+\s*)×(?=\s*\d+))
)");

        }

        public static string Decode(string input)
        {
            if (input == null)
            {
                return null;
            }

            foreach (var kvp in Replacements)
            {
                input = input.Replace(kvp.Key, kvp.Value);
            }

            return DecodeExpressions.Replace(input, Decode);
        }

        public static string Encode(string input)
        {
            if (input == null)
            {
                return null;
            }

            foreach (var kvp in Replacements)
            {
                input = input.Replace(kvp.Value, kvp.Key);
            }

            return EncodeExpressions.Replace(input, Encode);
        }

        private static string Decode(Match match)
        {
            // http://www.w3schools.com/tags/ref_symbols.asp

            if (match.Groups["x"].Success) return "×";
            if (match.Groups["esc"].Success) return "";
            return null;
        }

        private static string Encode(Match match)
        {
            // http://www.w3schools.com/tags/ref_symbols.asp

            if (match.Groups["x"].Success) return "x";
            if (match.Groups["esc"].Success) return "\\";
            return null;
        }

        /// <summary>
        /// Creates a regular expression.
        /// </summary>
        /// <param name="s">The expression.</param>
        /// <param name="multiline">Multiline mode. Changes the meaning of ^ and$ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.</param>
        /// <param name="singleline">Specifies single-line mode. Changes the meaning of the dot (.) so it matches every character (instead of every character except \n).</param>
        /// <returns>The compiled regular expression.</returns>
        private static Regex CreateRegex(string s, bool multiline = true, bool singleline = true)
        {
            var o = RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant;
            if (multiline)
            {
                o |= RegexOptions.Multiline;
            }

            if (singleline)
            {
                o |= RegexOptions.Singleline;
            }

            return new Regex(s, o);
        }
    }
}