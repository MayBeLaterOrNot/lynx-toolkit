namespace LynxToolkit.Documents
{
    public class OWikiParser : WikiParserBase
    {
        protected OWikiParser()
        {
            TableRowExpression = CreateRegex(@"(\|{1,2})(.+?)(?=\|)");
            ListItemExpression = CreateRegex(@"(?<=^)(\s*)((?<unordered>[-+*])|\d+\.|\#)\s.*?");

            BlocksExpression = CreateRegex(@"(
(?:  ^(?<h1>[^\n]+?)\n^={2,})|
(?:  ^(?<h2>[^\n]+?)\n^-{2,})|
(?:  ^(?<hlevel>\#{1,5})\s(?<h>[^\n]+)$)|
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
|(?:\*(?<em>.+?[^\\])\*)
|(?<smiley> (?: :-?\) | :-?\( | ;-?\) | :P | :D  ))
|(?<symbol> \( (?:[*i/x!+\-?_ynh]|on|off) \) )
|(?: `(?<code>.+?)`)
|(?: \#(?<anchor>[a-zA-Z0-9_]+))
|(?: \[ (?<ahref>.+?) (\|(?<atext>.+?))? (\|(?<atitle>.+?))?   \])
|(?: \{ (?<imgsrc>.+?)  (\|(?<imgalt>.+?))?  (\|(?<imglink>.+?))? \})
|(?<br>\s\s\n)
|(?<n>\n)
)", false);
        }

        public static Document Parse(string text, string documentFolder)
        {
            var p = new OWikiParser { BaseDirectory = documentFolder };
            p.ParseCore(text);
            return p.Document;
        }
    }
}