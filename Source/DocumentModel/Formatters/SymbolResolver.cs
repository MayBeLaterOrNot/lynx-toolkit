namespace LynxToolkit.Documents
{
    using System.Collections.Generic;
    using System.Linq;

    public static class SymbolResolver
    {
        private static Dictionary<string, string> symbolDictionary;

        static SymbolResolver()
        {
            symbolDictionary = new Dictionary<string, string>
                {
                    { ":-)", "smile.png" },
                    { ":-(", "sad.png" },
                    { ":P", "tongue.png" },
                    { ":D", "biggrin.png" },
                    { ";-)", "wink.png" },
                    { ":)", "smile.png" },
                    { ":(", "sad.png" },
                    { ";)", "wink.png" },
                    { "(*)", "star_yellow.png" },
                    { "(*r)", "star_red.png" },
                    { "(*g)", "star_green.png" },
                    { "(*b)", "star_blue.png" },
                    { "(*y)", "star_yellow.png" },
                    { "(i)", "information.png" },
                    { "(/)", "check.png" },
                    { "(x)", "error.png" },
                    { "(!)", "warning.png" },
                    { "(+)", "add.png" },
                    { "(-)", "forbidden.png" },
                    { "(?)", "help.png" },
                    { "(on)", "lightbulb_on.png" },
                    { "(off)", "lightbulb.png" },
                    { "(_)", "under_construction.png" },
                    { "(y)", "thumbs_up.png" },
                    { "(n)", "thumbs_down.png" },
                    { "(h)", "home.png" }
                };
        }

        public static string Decode(string input)
        {
            string file;
            if (!symbolDictionary.TryGetValue(input, out file))
                return null;
            return file;
        }

        public static string Encode(string file)
        {
            var keyValuePair = symbolDictionary.FirstOrDefault(kvp => kvp.Value == file);
            return keyValuePair.Key;
        }
    }
}