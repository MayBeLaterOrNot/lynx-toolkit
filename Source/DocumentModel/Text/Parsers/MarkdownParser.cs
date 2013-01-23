namespace LynxToolkit.Documents
{
    public class MarkdownParser : WikiParserBase
    {
        private MarkdownParser()
        {
            TableRowExpression = CreateRegex(@"(\|{1,2})(.+?)(?=\|)");
            ListItemExpression = CreateRegex(@"(?<=^)(\s*)([-+*]|\d+\.|\#)\s.*?");

            BlocksExpression = CreateRegex(@"(
(?: ^(?<h1>[^\n]+?)\n^={2,})|
(?: ^(?<h2>[^\n]+?)\n^-{2,})|
(?: ^(?<hlevel>\#{1,5})\s(?<hcontent>[^\n]+)$)|
(?: ^(?<table>\|.+?)(?:\n\n|\z))|
(?: ^(?<quote>\>.+?)(?:\n\n|\z))|
(?: ^(?<ul>\s*[-+*]\s.+?)(?:\n\n|\z))|
(?: ^(?<ol>\s*\d+\.\s.+?)(?:\n\n|\z))|
(?: ^(?<code>\s{4}.+?)(?:\n[^\s]|\z))|
)");

            InlineExpression = CreateRegex(@"(?<=[^\\]|^)     # Not escaped
(?:
 (?: \*\*(?<strong>.+?[^\\])\*\*)
|(?: \*(?<em>.+?[^\\])\*)
|(?: `(?<code>.+?)`)
|(?: ¤(?<anchor>.+?)¤)
|(?: !\[ (?<imgalt>.+?)  \] \( (?<imgsrc> .+?) (\""(?<imgtitle>.+?) \"")? \))
|(?: \[ (?<atext>.+?) \] \( (?<ahref>.+?) (\""(?<atitle>.+?) \"")? \))
|(?<br>\s\s\n)
|(?<n>\n)
)");
        }

        public static Document Parse(string text, string documentFolder)
        {
            var p = new MarkdownParser { BaseDirectory = documentFolder };
            p.ParseCore(text);
            return p.Document;
        }

    }
}