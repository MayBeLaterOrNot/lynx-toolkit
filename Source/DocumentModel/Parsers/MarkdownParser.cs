namespace LynxToolkit.Documents
{
    public class MarkdownParser : WikiParserBase
    {
        private MarkdownParser()
        {
            TableRowExpression = CreateRegex(@"(\|{1,2})(.+?)(?=\|)");
            ListItemExpression = CreateRegex(@"(?<=^)(\s*)([-+*]|\d+\.|\#)\s.*?");

            BlocksExpression = CreateRegex(@"(
(?<h1> ^(?<h1content>[^\n]+?)\n^={2,})|
(?<h2> ^(?<h2content>[^\n]+?)\n^-{2,})|
(?<h> ^(?<hlevel>\#{1,5})\s(?<hcontent>[^\n]+)$)|
(?<table>  ^(?<tablecontent>\|.+?)(?:\n\n|\z))|
(?<quote>  ^(?<quotecontent>\>.+?)(?:\n\n|\z))|
(?<ul>  ^(?<ulcontent>\s*[-+*]\s.+?)(?:\n\n|\z))|
(?<ol>  ^(?<olcontent>\s*\d+\.\s.+?)(?:\n\n|\z))|
(?<code>  ^(?<codecontent>\s{4}.+?)(?:\n[^\s]|\z))|
)");

            InlineExpression = CreateRegex(@"(?<=[^\\]|^)     # Not escaped
(?:
(?<strong>\*\*(?<strongContent>.+?[^\\])\*\*)
|(?<em>\*(?<emContent>.+?[^\\])\*)
|(?<code>`(?<codeContent>.+?)`)
|(?<anchor>¤(?<anchorName>.+?)¤)
|(?<img>  !\[ (?<alt>.+?)  \] \( (?<src> .+?) (\""(?<title>.+?) \"")? \))
|(?<a>     \[ (?<text>.+?) \] \( (?<href>.+?) (\""(?<title>.+?) \"")? \))
|(?<br>\s\s\n)
|(?<n>\n)
)");
        }

        public static Document Parse(string text)
        {
            var p = new MarkdownParser();
            p.ParseCore(text);
            return p.Document;
        }

    }
}