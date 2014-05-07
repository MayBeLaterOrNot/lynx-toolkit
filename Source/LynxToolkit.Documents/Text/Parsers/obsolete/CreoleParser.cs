// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreoleParser.cs" company="Lynx Toolkit">
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
    public class CreoleParser : WikiParserBase
    {
        private CreoleParser()
        {
            this.TableRowExpression = CreateRegex(@"(\|{1,2})(.+?)(?=\|)");
            this.ListItemExpression = CreateRegex(@"(?<=^)(\s*)([-+*#])\s.*?");

            this.BlocksExpression = CreateRegex(@"(
(?: ^(?<hlevel>\={1,5})\s+(?<hcontent>[^\n]+)\s*\=*$)|
(?: ^(?<table>\|.+?)(?:\n\n|\z))|
(?: ^(?<quote>\>.+?)(?:\n\n|\z))|
(?: ^(?<ul>\s*[-+*]+\s.+?)(?:\n\n|\z))|
(?: ^(?<ol>\s*\#+\s.+?)(?:\n\n|\z))|
(?: ^```(?<codelanguage>.*?)\n(?<code>.+?)```)|
(?<hr>  ^----\n)
)");

            this.InlineExpression = CreateRegex(@"(?<=[^\\]|^)     # Not escaped
(?:
 (?: \*\*(?<strong>.+?[^\\])\*\*)
|(?: //(?<em>.+?[^\\])//)
|(?: `(?<code>.+?)`)
|(?: \#(?<anchor>[^\s]+))
|(?: \[ ((?<atext>.+?)\|)? (?<ahref>.+?) (\|(?<atitle>.+?))?\])
|(?: \{(?<imgsrc>.+?)(\|(?<imgalt>.+?))?(\|(?<imglink>.+?))?\})
|(?<br>\s\s\n)
|(?<n>\n)
)", false);
        }

        public static Document Parse(string text, string documentFolder)
        {
            var p = new CreoleParser { BaseDirectory = documentFolder };
            p.ParseCore(text);
            return p.Document;
        }
    }
}