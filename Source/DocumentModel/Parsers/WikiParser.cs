namespace LynxToolkit.Documents
{
    using System;
    using System.Text.RegularExpressions;

    public class WikiParser
    {
        public Document Document { get; set; }
        public string Syntax { get; set; }

        private static readonly Regex DirectivesExpression;

        static WikiParser()
        {
            DirectivesExpression = new Regex(
@"(?<=[^\\]|^)     # Not escaped
@(?:
(?<title>title \s (?<titleContent>.+?) \r?\n)|
(?<description>description \s (?<descriptionContent>.+?) \r?\n)|
(?<keywords>keywords \s (?<keywordsContent>.+?) \r?\n)|
(?<syntax>syntax \s (?<syntaxContent>.+?) \r?\n)
)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        }

        public static Document Parse(string text)
        {
            string title = null, description = null, keywords = null, syntax = null;

            text = DirectivesExpression.Replace(text,
                m =>
                {
                    if (m.Groups["titleContent"].Success)
                    {
                        title = m.Groups["titleContent"].Value;
                    }

                    if (m.Groups["descriptionContent"].Success)
                    {
                        description = m.Groups["descriptionContent"].Value;
                    }

                    if (m.Groups["keywordsContent"].Success)
                    {
                        keywords = m.Groups["keywordsContent"].Value;
                    }

                    if (m.Groups["syntaxContent"].Success)
                    {
                        syntax = m.Groups["syntaxContent"].Value;
                    }

                    return string.Empty;
                });

            Document doc;
            switch (syntax)
            {
                case "creole":
                    doc = CreoleParser.Parse(text);
                    break;
                case "markdown":
                case "md":
                    doc = MarkdownParser.Parse(text);
                    break;
                case "confluence":
                    throw new Exception("Confluence wiki parsing not supported.");
                    // doc = ConfluenceParser.Parse(text);
                case "codeplex":
                    throw new Exception("Codeplex wiki parsing not supported.");
                    // doc = CodeplexParser.Parse(text);
                default:
                    doc = OWikiParser.Parse(text);
                    break;
            }

            doc.Title = title;
            doc.Description = description;
            doc.Keywords = keywords;
            return doc;
        }
    }
}