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