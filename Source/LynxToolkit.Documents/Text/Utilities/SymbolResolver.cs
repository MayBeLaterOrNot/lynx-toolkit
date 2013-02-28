// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolResolver.cs" company="Lynx Toolkit">
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
    using System.Linq;

    public static class SymbolResolver
    {
        private static readonly Dictionary<string, string> symbolDictionary;

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
        
        public static IEnumerable<string> GetSymbolNames()
        {
            return symbolDictionary.Keys;
        }

        public static string Decode(string input)
        {
            string file;
            if (!symbolDictionary.TryGetValue(input, out file))
            {
                return null;
            }

            return file;
        }

        public static string Encode(string file)
        {
            var keyValuePair = symbolDictionary.FirstOrDefault(kvp => kvp.Value == file);
            return keyValuePair.Key;
        }
    }
}