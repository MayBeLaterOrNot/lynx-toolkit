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

        static readonly Dictionary<string, string> Replacements;

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

            DecodeExpressions = DocumentParser.CreateRegex(@"(?<=[^\\]|^)     # Not escaped
(?:
(?<x>(?<=\d+\s*)x(?=\s*\d+))
| (?<esc>\\)
)");
            EncodeExpressions = DocumentParser.CreateRegex(@"(?<=[^\\]|^)     # Not escaped
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

    }
}