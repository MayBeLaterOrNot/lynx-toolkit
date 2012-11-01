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
            foreach (var kvp in Replacements)
            {
                input = input.Replace(kvp.Key, kvp.Value);
            }

            return DecodeExpressions.Replace(input, Decode);
        }

        public static string Encode(string input)
        {
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