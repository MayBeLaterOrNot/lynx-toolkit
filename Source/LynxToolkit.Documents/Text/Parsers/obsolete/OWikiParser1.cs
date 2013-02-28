// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OWikiParser.cs" company="Lynx Toolkit">
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
    public class OWikiParser1 : WikiParserBase
    {
        protected OWikiParser1()
        {
            TableRowExpression = CreateRegex(@"(\|{1,2})(.+?)(?=\|)");
            ListItemExpression = CreateRegex(@"(?<=^)(\s*)((?<unordered>[-+*])|\d+\.|\#)\s.*?");

            BlocksExpression = CreateRegex(@"(
(?:  ^(?<hlevel>={1,5})\s(?<h>[^\n]+))
|(?: ^(----)$)
)");

            BlocksExpression = CreateRegex(@"(
(?:  ^(?<hlevel>\={1,5})\s(?<h>[^\n]+)$)|
(?:  ^(?<table>\|.+?)(?:\n\n|\z))|
(?:  ^(?<quote>\>.+?)(?:\n\n|\z))|
(?<hr>  ^\s*-\s-\s-\s*$)|
(?:  ^(?<ul>\s*[-+*]\s.+?)(?:\n\n|\z))|
(?:  ^(?<ol>\s*(\d+\.|\#)\s.+?)(?:\n\n|\z))|
(?:  ^```(?<codelanguage>.*?)\n(?<code>.+?)```)|
(?<p> \n\n)
)");
            InlineExpression = CreateRegex(@"(?<=[^\\]|^)     # Not escaped
(?:
 (?: \*\*(?<strong>.+?[^\\])\*\*)
|(?: \*(?<em>.+?[^\\])\*)
|(<smiley>  (?: :-?\) | :-?\( | ;-?\) | :P | :D  ) )
|(?: (<symbol> \( (?:[*i/x!+\-?_ynh]|on|off) \)) )
|(?: `(?<code>.+?)`)
|(?: \#(?<anchor>[a-zA-Z0-9_]+))
|(?: \[ (?<ahref>.+?) (\|(?<atext>.+?))? (\|(?<atitle>.+?))?   \])
|(?: \{ (?<imgsrc>.+?)  (\|(?<imgalt>.+?))?  (\|(?<imglink>.+?))? \})
|(?: (<br>\s\s\n))
|(?: (<n>\n))
)", false);
//            InlineExpression = CreateRegex(@"(?<=[^\\]|^)     # Not escaped
//(?:
//(?: \*\*(?<strong>.+?[^\\])\*\*)
//|(?:\*(?<em>.+?[^\\])\*)
//|(?<smiley> (?: :-?\) | :-?\( | ;-?\) | :P | :D  ))
//|(?<symbol> \( (?:[*i/x!+\-?_ynh]|on|off) \) )
//|(?: `(?<code>.+?)`)
//|(?: \#(?<anchor>[a-zA-Z0-9_]+))
//|(?: \[ (?<ahref>.+?) (\|(?<atext>.+?))? (\|(?<atitle>.+?))?   \])
//|(?: \{ (?<imgsrc>.+?)  (\|(?<imgalt>.+?))?  (\|(?<imglink>.+?))? \})
//|(?<br>\s\s\n)
//|(?<n>\n)
//)", false);
        }

        public static Document Parse(string text, string documentFolder)
        {
            var p = new OWikiParser1 { BaseDirectory = documentFolder };
            p.ParseCore(text);
            return p.Document;
        }
    }
}