namespace LynxToolkit.Documents
{
    public class CreoleParser : WikiParserBase
    {
        private CreoleParser()
        {
            TableRowExpression = CreateRegex(@"(\|{1,2})(.+?)(?=\|)");
            ListItemExpression = CreateRegex(@"(?<=^)(\s*)([-+*#])\s.*?");

            BlocksExpression = CreateRegex(@"(
(?<h> ^(?<hlevel>\={1,5})\s+(?<hcontent>[^\n]+)\s*\=*$)|
(?<table>  ^(?<tablecontent>\|.+?)(?:\n\n|\z))|
(?<quote>  ^(?<quotecontent>\>.+?)(?:\n\n|\z))|
(?<ul>  ^(?<ulcontent>\s*[-+*]+\s.+?)(?:\n\n|\z))|
(?<ol>  ^(?<olcontent>\s*\#+\s.+?)(?:\n\n|\z))|
(?<code>  ^```(?<codelanguage>.*?)\n(?<codecontent>.+?)```)|
(?<hr>  ^----\n)
)");

            InlineExpression = CreateRegex(@"(?<=[^\\]|^)     # Not escaped
(?:
(?<strong>\*\*(.+?[^\\])\*\*)
|(?<em>//(.+?[^\\])//)
|(?<code>`(.+?)`)
|(?<anchor>\#(?<anchorName>[^\s]+))
|(?<a>\[ ((?<text>.+?)\|)? (?<href>.+?) (\|(?<title>.+?))?\])
|(?<img>\{(?<src>.+?)(\|(?<alt>.+?))?(\|(?<imglink>.+?))?\})
|(?<br>\s\s\n)
|(?<n>\n)
)", false);
        }

        public static Document Parse(string text)
        {
            var p = new CreoleParser();
            p.ParseCore(text);
            return p.Document;
        }
    }
}