// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreoleConverter.cs" company="Lynx">
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
// <summary>
//   Converts Creole 1.0 formatted wiki text to xhtml.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CreoleWikiEngine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;

    using LynxToolkit;

    /// <summary>
    /// Converts Creole 1.0 formatted wiki text to xhtml.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    public class CreoleConverter : IWikiConverter
    {
        private const string LineBreakReplacement = "¶";

        private const string TildeReplacement = @"\u0016";

        private static readonly Regex NewLine = CreateRegex(@"(\r?\n)");

        private static readonly Regex LessThanExpression = CreateRegex(@"<");

        private static readonly Regex GreaterThanExpression = CreateRegex(@">");

        private static readonly Regex HeaderExpression = CreateRegex(@"(=+) (.*)");

        private static readonly Regex TableRowExpression = CreateRegex(@"((^|¶)\|[^¶]+)");

        private static readonly Regex TableCellExpression = CreateRegex(@"\|(=)?(.*?)(?=[¶|]|$)");

        private static readonly Regex Definition = CreateRegex(@";\s*(.*?[^~]):\s*(.*?)(?=[^~];|$)");

        private static readonly Regex Indentation = CreateRegex(@"(?<=^|¶)([:>]+)(.*?)(?=¶(\1)|¶<|¶¶|$)");

        private Options options;

        private IReplacement substituteLineBreaks;

        private IReplacement substituteTilde;

        private IReplacement trimLines;

        private IReplacement alternateLink;

        private IReplacement arrows;

        private IReplacement lessGreaterThan;

        private IReplacement escapedLessGreaterThan;

        private IReplacement horizontalRuler;

        private IReplacement freelinks;

        private IReplacement strong;

        private IReplacement emphasized;

        private IReplacement strikeout;

        private IReplacement underline;

        private IReplacement superscript;

        private IReplacement subscript;

        private IReplacement monospace;

        private IReplacement paragraphs;

        private IReplacement definitionList;

        private IReplacement indentation;

        private IReplacement list;

        private IReplacement lineBreak;

        private IReplacement dashes;

        private IReplacement copyright;

        private IReplacement icons;

        private IReplacement smileys;

        private IReplacement box;

        private IReplacement quote;

        private IReplacement image;

        private IReplacement link;

        private IReplacement anchor;

        private IReplacement table;

        private IReplacement escaped;

        private IReplacement removeLinebreaks;

        private IReplacement replaceLinebreaks;

        private IReplacement replaceTilde;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreoleConverter"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public CreoleConverter(Options options = null)
        {
            this.options = options ?? new Options();

            // Replace line breaks
            this.substituteLineBreaks = new StringReplacement(@"(\r?\n)", LineBreakReplacement);

            // Replace tilde "~~"
            this.substituteTilde = new StringReplacement(@"~~", TildeReplacement);

            // Trim all lines and remove double empty lines
            this.trimLines = new ReplacementGroup(
                new StringReplacement(@"\s*¶", "¶"),
                new StringReplacement(@"¶\s*", "¶"),
                new StringReplacement(@"\s*¶", "¶"),
                new StringReplacement(@"¶¶+", "¶¶"));

            const string NotEscaped = @"(?<!~)";

            // alternate link syntax (Creole 2.0)
            this.alternateLink = new StringReplacement(@"(?<!~)\[\[([^¶]*)->([^¶]+?)\]\]", "[[$2|$1]]");

            // Arrows / special characters
            this.arrows = new ReplacementGroup(
                new StringReplacement(NotEscaped + @"[\.]{3}", "&hellip;"),
                new StringReplacement(NotEscaped + @"<->", "&harr;"),
                new StringReplacement(NotEscaped + @"<-", "&larr;"),
                new StringReplacement(NotEscaped + @"->", "&rarr;"),
                new StringReplacement(NotEscaped + @"<=>", "&hArr;"),
                new StringReplacement(NotEscaped + @"<=", "&lArr;"),
                new StringReplacement(NotEscaped + @"=>", "&rArr;"));

            // Less/greater than
            this.lessGreaterThan = new ReplacementGroup(
                new StringReplacement(NotEscaped + @">", "&gt;"),
                new StringReplacement(NotEscaped + @"<", "&lt;"),
                new StringReplacement(NotEscaped + @">=", "&ge;"),
                new StringReplacement(NotEscaped + @"<=", "&le;"));

            // Escaped less/greater than
            this.escapedLessGreaterThan = new ReplacementGroup(
                new StringReplacement(@"~>", ">"), new StringReplacement(@"~<", "<"));

            // horizontal ruler (Creole 1.0)
            this.horizontalRuler = new StringReplacement(@"----", "<hr/>");

            // convert free (raw) links to [[...]] syntax (Creole 1.0)
            this.freelinks =
                new StringReplacement(
                    @"(?<!\[)\b((https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]*[-A-Za-z0-9+&@#/%=~_|])", "[[$1]]");

            // strong (Creole 1.0)
            this.strong = new StringReplacement(NotEscaped + @"\*\*([^\s*].+?)(?<!~)(\*\*|(¶¶))", "<strong>$1</strong>$3");

            // emphasized (Creole 1.0)
            this.emphasized =
                new StringReplacement(
                    NotEscaped + @"(?<!https?:|ftp:|file:)//(.+?)(?<!~|https?:|ftp:|file:)(//|(¶¶))", "<em>$1</em>$3");

            // strike (Creole 2.0)
            this.strikeout = new StringReplacement(NotEscaped + @"--([^¶]+?)(?<!~)--", "<del>$1</del>");

            // underline (Creole 2.0)
            this.underline = new StringReplacement(@"(?<!~)__([^¶]+?)(?<!~)__", "<u>$1</u>");

            // superscript (Creole 2.0)
            this.superscript = new StringReplacement(@"(?<!~)\^\^([^¶]+?)(?<!~)\^\^", "<sup>$1</sup>");

            // subscript (Creole 2.0)
            this.subscript = new StringReplacement(@"(?<!~),,([^¶]+?)(?<!~),,", "<sub>$1</sub>");

            // monospace (Creole 2.0)
            this.monospace = new StringReplacement(@"(?<!~)##([^¶]+?)(?<!~)##", "<tt>$1</tt>");

            // paragraphs (Creole 1.0)
            this.paragraphs = new StringReplacement(@"(?<=^|¶)([^=#*|¶].+?)(?=¶¶|$|¶[=#*|])", "<p>¶$1¶</p>");

            // definition list (Creole 2.0)
            this.definitionList = new EvaluatorReplacement(@"(?<=^|¶)\s*(; .*?)(?=¶¶|$)", this.ReplaceDefinitionList);

            // indentation (Creole 2.0)
            this.indentation = new EvaluatorReplacement(@"(?<=^|¶)([:>]+)(.*?)(?=¶(\1)|¶¶|$)", this.ReplaceIndentation);

            // unordered lists (Creole 2.0)
            this.list = new EvaluatorReplacement(@"(?<=^|¶)(([#\*]+)\s+(.*?))(?=¶[^#\*]|$)", this.ReplaceUnorderedList);

            // forced line break
            this.lineBreak = new StringReplacement(@"\\\\", "<br />");

            this.dashes = new ReplacementGroup(
                new StringReplacement(@"---", "&mdash;"), new StringReplacement(@"--", "&ndash;"));

            this.copyright = new ReplacementGroup(
                new StringReplacement(@"\(C\)", "&copy;"),
                new StringReplacement(@"\(R\)", "&reg;"),
                new StringReplacement(@"\(TM\)", "&trade;"));

            this.icons = new ReplacementGroup(
                new IconReplacement(@"\(\*\)", "star_yellow.png", this.options),
                new IconReplacement(@"\(\*r\)", "star_red.png", this.options),
                new IconReplacement(@"\(\*g\)", "star_green.png", this.options),
                new IconReplacement(@"\(\*b\)", "star_blue.png", this.options),
                new IconReplacement(@"\(\*y\)", "star_yellow.png", this.options),
                new IconReplacement(@"\(i\)", "information.png", this.options),
                new IconReplacement(@"\(/\)", "check.png", this.options),
                new IconReplacement(@"\(x\)", "error.png", this.options),
                new IconReplacement(@"\(\!\)", "warning.png", this.options),
                new IconReplacement(@"\(\+\)", "add.png", this.options),
                new IconReplacement(@"\(\-\)", "forbidden.png", this.options),
                new IconReplacement(@"\(\?\)", "help.png", this.options),
                new IconReplacement(@"\(on\)", "lightbulb_on.png", this.options),
                new IconReplacement(@"\(off\)", "lightbulb.png", this.options),
                new IconReplacement(@"\(h\)", "home.png", this.options),
                new IconReplacement(@"\(_\)", "UnderConstruction.png", this.options));

            this.smileys = new ReplacementGroup(
                new IconReplacement(@":\)", "smile.png", this.options),
                new IconReplacement(@":\(", "sad.png", this.options),
                new IconReplacement(@":P", "tongue.png", this.options),
                new IconReplacement(@":D", "biggrin.png", this.options),
                new IconReplacement(@";\)", "wink.png", this.options),
                new IconReplacement(@"\(y\)", "thumbs_up.png", this.options),
                new IconReplacement(@"\(n\)", "thumbs_down.png", this.options),
                new IconReplacement(@"\(on\)", "lightbulb_on.png", this.options),
                new IconReplacement(@"\(off\)", "lightbulb.png", this.options),
                new IconReplacement(@"\(h\)", "home.png", this.options));

            // Box
            this.box = new EvaluatorReplacement(@"\[\[\[([i!?+-])(¶.+?¶)?\]\]\]", this.ReplaceBox);

            // Quote
            this.quote = new EvaluatorReplacement(@"'''(.*?)(\|(.*?))?'''", this.ReplaceQuote);

            // Image (Creole 1.0)
            this.image = new EvaluatorReplacement(@"(?<!~){{([^¶]*?)(\|([^¶]+?))?}}", this.ReplaceImage);

            // Link (Creole 1.0)
            this.link = new EvaluatorReplacement(@"(?<!~)\[\[([^¶]*?)(\|([^¶]+?))?\]\]", this.ReplaceLink);

            // Anchor name definition
            this.anchor = new StringReplacement(NotEscaped + @"\[#([^]]+?)\]", @"<a name=""$1"" />");

            // table (Creole 1.0)
            this.table = new EvaluatorReplacement(@"(?<=^|¶)(\|.*?)(?=¶[^|]|$)", this.ReplaceTable);

            // escaped characters (Creole 1.0)
            this.escaped = new StringReplacement(@"\~([|*#/-_={[<>)])", "$1");

            // remove linebreaks at beginning and end of string
            this.removeLinebreaks = new StringReplacement(@"(^¶)|(¶$)", string.Empty);

            // replace '¶' with line breaks
            this.replaceLinebreaks = new StringReplacement(LineBreakReplacement, "\r\n");

            // replace escape ~
            this.replaceTilde = new StringReplacement(TildeReplacement, "~");
        }

        /// <summary>
        /// Transforms the specified input text.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>The output html.</returns>
        public string Transform(string text)
        {
            var codeblocks = new Substitution(@"(?<=^|\n){{{((([^\n]*?):)?\r?\n.*?)(?<=^|\n)}}}", "\u0017");
            var inlineCodeblocks = new Substitution(@"{{{(.*?[^¶])}}}", "\u0018");
            var headerblocks = new Substitution(@"(?<=^|¶)((=+) (.*?))=*(?=¶|$)", "= \u0019");

            var result = codeblocks.Substitute(text);
            result = NewLine.Replace(result, @"¶");
            result = inlineCodeblocks.Substitute(result);
            result = headerblocks.Substitute(result);

            result = this.substituteLineBreaks.Apply(result);
            result = this.substituteTilde.Apply(result);
            result = this.trimLines.Apply(result);

            if (this.options.SupportCreole2)
            {
                result = this.alternateLink.Apply(result);
            }

            if (this.options.SupportCreoleX)
            {
                result = this.arrows.Apply(result);
            }

            result = this.lessGreaterThan.Apply(result);
            result = this.escapedLessGreaterThan.Apply(result);

            result = this.horizontalRuler.Apply(result);
            result = this.freelinks.Apply(result);
            result = this.strong.Apply(result);
            result = this.emphasized.Apply(result);

            if (this.options.SupportCreole2)
            {
                result = this.strikeout.Apply(result);
                result = this.underline.Apply(result);
                result = this.superscript.Apply(result);
                result = this.subscript.Apply(result);
                result = this.monospace.Apply(result);
            }

            result = this.paragraphs.Apply(result);
            result = this.definitionList.Apply(result);
            result = this.indentation.Apply(result);
            result = this.list.Apply(result);
            result = this.lineBreak.Apply(result);

            if (this.options.SupportCreoleX)
            {
                result = this.dashes.Apply(result);
                result = this.copyright.Apply(result);
                result = this.icons.Apply(result);
                result = this.smileys.Apply(result);
                result = this.box.Apply(result);
            }

            if (this.options.SupportCreole2)
            {
                result = this.quote.Apply(result);
            }

            result = this.image.Apply(result);
            result = this.link.Apply(result);
            if (this.options.SupportCreoleX)
            {
                result = this.anchor.Apply(result);
            }

            result = this.table.Apply(result);
            result = this.escaped.Apply(result);
            result = this.removeLinebreaks.Apply(result);
            result = this.replaceLinebreaks.Apply(result);
            result = this.replaceTilde.Apply(result);

            result = codeblocks.Revert(result, s => string.Format("<code><pre>{0}</pre></code>", this.FormatCode(s)));
            result = inlineCodeblocks.Revert(result, s => string.Format("<code>{0}</code>", this.FormatCode(s)));
            result = headerblocks.Revert(result, this.ReplaceHeader);

            return result;
        }

        private class Substitution
        {
            private Regex expression;

            private Regex revertExpression;

            private string code;

            private IList<string> codeBlocks = new List<string>();

            public Substitution(string expression, string substitutionCode)
            {
                this.expression = CreateRegex(expression);
                this.code = substitutionCode;
                this.revertExpression = CreateRegex(this.code + @"(\d+)" + this.code);
            }

            public string Substitute(string input)
            {
                input = this.expression.Replace(
                    input,
                    match =>
                    {
                        codeBlocks.Add(match.Groups[1].Value);
                        var r = string.Format(@"{0}{1}{0}", code, codeBlocks.Count - 1);
                        return r;
                    });
                return input;
            }

            public string Revert(string input, Func<string, string> f)
            {
                return this.revertExpression.Replace(input, match => f(this.codeBlocks[int.Parse(match.Groups[1].Value)]));
            }
        }

        private object FormatCode(string input)
        {
            var output = LessThanExpression.Replace(input, "&lt;");
            output = GreaterThanExpression.Replace(output, "&gt;");
            return output;
        }

        private string ReplaceHeader(string s)
        {
            var match = HeaderExpression.Match(s);
            int level = match.Groups[1].Value.Length;
            return string.Format("<h{0}>{1}</h{0}>", level, match.Groups[2].Value);
        }

        private static Regex CreateRegex(string expression)
        {
            return new Regex(expression, RegexOptions.Singleline | RegexOptions.Compiled);
        }

        private string ReplaceDefinitionList(Match match)
        {
            var content = match.Groups[1].Value;
            content = Definition.Replace(content, "<dt>$1</dt><dd>$2</dd>");
            return string.Format("<dl>¶{0}¶</dl>", content);
        }

        private string ReplaceUnorderedList(Match match)
        {
            var isOrdered = match.Groups[2].Value[0] == '#';
            var content = match.Groups[1].Value;
            int x = match.Groups[2].Length;
            var r = CreateRegex(@"(?<=^|¶)([#\*]{" + x + @"})\s+(.*?)(?=¶\1\s|¶¶|$)");
            content = r.Replace(content, this.ReplaceItem);
            return string.Format("<{0}>¶{1}¶</{0}>", isOrdered ? "ol" : "ul", content);
        }

        private string ReplaceItem(Match match)
        {
            var content = match.Groups[2].Value;
            content = this.list.Apply(content);
            return string.Format("<li>{0}</li>", content);
        }

        private string ReplaceIndentation(Match match)
        {
            var content = match.Groups[2].Value;
            content = Indentation.Replace(content, ReplaceIndentation);
            return string.Format("<div class=\"indented\">¶{0}¶</div>", content);
        }

        private string ReplaceTable(Match match)
        {
            var content = match.Groups[1].Value.Trim();
            content = TableRowExpression.Replace(content, this.ReplaceTableRow);
            return string.Format("<table>¶{0}¶</table>", content);
        }

        private string ReplaceTableRow(Match match)
        {
            var content = match.Groups[1].Value.TrimEnd('|');
            content = TableCellExpression.Replace(content, this.ReplaceTableCell);
            return string.Format("<tr>{0}¶</tr>", content);
        }

        private string ReplaceTableCell(Match match)
        {
            var prefix = match.Groups[1].Value;
            var element = prefix == "=" ? "th" : "td";
            var content = match.Groups[2].Value.Trim();
            var css = this.GetAlignment(ref content);
            return string.Format("<{0}{1}>{2}</{0}>", element, css, content);
        }

        private static string GetAlignment(string left, string right)
        {
            if (!string.IsNullOrEmpty(left) && !string.IsNullOrEmpty(right))
                return " class=\"center\"";
            if (!string.IsNullOrEmpty(left))
                return " class=\"left\"";
            if (!string.IsNullOrEmpty(right))
                return " class=\"right\"";
            return string.Empty;
        }

        private string ReplaceLink(Match match)
        {
            var href = match.Groups[1].Value.Trim();
            var text = match.Groups[3].Value.Trim();
            var linkimage = string.Empty;
            var css = string.Empty;
            var target = string.Empty;

            if (!href.StartsWith("http"))
            {
                // internal link
                var match2 = Regex.Match(href, "((.*):)?(.*)");
                var space = match2.Groups[2].Value.Trim();
                var id = match2.Groups[3].Value.Trim();
                string format;
                if (this.options.InternalLinkSpaces.TryGetValue(space, out format))
                {
                    // space defined
                    href = string.Format(format, id);
                }
                else
                {
                    // no space
                    href = this.options.LocalLinkFunction(space, id);
                }
            }

            if (href.StartsWith("http"))
            {
                css = " class=\"external\"";
                linkimage = this.options.ExternalLinkImage;
                target = string.Format(" target=\"{0}\"", this.options.ExternalLinkTarget);
            }

            if (href.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase)
                && !string.IsNullOrEmpty(this.options.DocumentLinkImage))
            {
                linkimage = this.options.DocumentLinkImage;
            }
            if (href.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase)
                && !string.IsNullOrEmpty(this.options.ArchiveLinkImage))
            {
                linkimage = this.options.ArchiveLinkImage;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                text = href;
            }

            if (!string.IsNullOrEmpty(linkimage))
            {
                linkimage = string.Format("<img src=\"{0}{1}\"{2} alt/>", options.ImagePath, linkimage, css);
            }

            return string.Format("<a href=\"{0}\"{1}{2}>{3}{4}</a>", href, css, target, text, linkimage);
        }

        private string ReplaceBox(Match match)
        {
            var type = match.Groups[1].Value.Trim();
            var content = match.Groups[2].Value.Trim();
            var style = "infoBox";
            switch (type)
            {
                case "i":
                    style = "infoBox";
                    break;
                case "+":
                    style = "noteBox";
                    break;
                case "!":
                    style = "warningBox";
                    break;
                case "?":
                    style = "tipBox";
                    break;
            }

            return string.Format("<div class=\"{0}\">{1}</div>", style, content);
        }

        private string ReplaceQuote(Match match)
        {
            var quote = match.Groups[1].Value.Trim();
            var cite = match.Groups[3].Value.Trim();
            if (string.IsNullOrEmpty(cite))
            {
                return string.Format("<blockquote>{0}</blockquote>", quote);
            }

            // Use cite attribute
            // return string.Format("<blockquote cite=\"{1}\">{0}</blockquote>", quote, cite);

            // Use cite element
            return string.Format("<blockquote>{0}<br/><cite>{1}</cite></blockquote>", quote, cite);
        }

        private string GetAlignment(ref string input)
        {
            var m = Regex.Match(input, @"^(\^)?(.*?)(\^)?$");
            input = m.Groups[2].Value;
            return GetAlignment(m.Groups[1].Value, m.Groups[3].Value);
        }

        private string ReplaceImage(Match match)
        {
            var source = match.Groups[1].Value.Trim();
            var alt = match.Groups[3].Value.Trim();
            var css = this.GetAlignment(ref source);

            if (!Regex.IsMatch(source, "http[s]?://"))
            {
                // local page
                source = this.options.LocalImageFunction(source);
            }

            if (string.IsNullOrEmpty(alt))
            {
                return string.Format("<img src=\"{0}\"{1} />", source, css);
            }

            return string.Format("<img src=\"{0}\" alt=\"{1}\"{2}/>", source, alt, css);
        }

        public interface IReplacement
        {
            string Apply(string input);
        }

        public abstract class Replacement : IReplacement
        {
            public Regex Expression { get; set; }

            public Replacement(string expression)
            {
                Expression = new Regex(expression, RegexOptions.Singleline | RegexOptions.Compiled);
            }

            public abstract string Apply(string input);
        }

        public class ReplacementGroup : IReplacement
        {
            public IEnumerable<Replacement> Rules { get; set; }

            public ReplacementGroup(params Replacement[] replacements)
            {
                this.Rules = replacements;
            }

            public ReplacementGroup(IEnumerable<Replacement> replacements)
            {
                this.Rules = replacements;
            }

            public string Apply(string input)
            {
                foreach (var r in Rules)
                {
                    input = r.Apply(input);
                }
                return input;
            }
        }

        private class EvaluatorReplacement : Replacement
        {
            private MatchEvaluator MatchEvaluator { get; set; }

            public EvaluatorReplacement(string expression, MatchEvaluator evaluator)
                : base(expression)
            {
                this.MatchEvaluator = evaluator;
            }

            public override string Apply(string result)
            {
                return this.Expression.Replace(result, this.MatchEvaluator);
            }
        }

        private class StringReplacement : Replacement
        {
            private string Replacement { get; set; }

            public StringReplacement(string expression, string replacement)
                : base(expression)
            {
                this.Replacement = replacement;
            }

            public override string Apply(string result)
            {
                return this.Expression.Replace(result, this.Replacement);
            }
        }

        private class IconReplacement : Replacement
        {
            private string Image { get; set; }

            private Options options;

            public IconReplacement(string expression, string image, Options options)
                : base(expression)
            {
                this.Image = image;
                this.options = options;
            }

            public override string Apply(string result)
            {
                var replacement = string.Format(@"<img src=""{0}{1}"" />", this.options.ImagePath, this.Image);
                return this.Expression.Replace(result, replacement);
            }
        }

        /// <summary>
        /// Specifies parsing options.
        /// </summary>
        public class Options
        {
            public bool EnableIcons { get; set; }

            public bool EnableSmileys { get; set; }

            /// <summary>
            /// Gets or sets the icon path.
            /// </summary>
            /// <value>
            /// The icon path.
            /// </value>
            public string ImagePath { get; set; }

            /// <summary>
            /// Gets or sets the local link function.
            /// </summary>
            /// <value>
            /// The local link function.
            /// </value>
            public Func<string, string, string> LocalLinkFunction { get; set; }

            /// <summary>
            /// Gets or sets the local image function.
            /// </summary>
            /// <value>
            /// The local image function.
            /// </value>
            public Func<string, string> LocalImageFunction { get; set; }

            public Dictionary<string, string> InternalLinkSpaces { get; set; }

            public bool SupportCreoleX { get; set; }

            public string ExternalLinkImage { get; set; }

            public string DocumentLinkImage { get; set; }

            public string ArchiveLinkImage { get; set; }

            public string ExternalLinkTarget { get; set; }

            public bool EnableArrows { get; set; }

            public bool SupportCreole2 { get; set; }

            public Options()
            {
                this.SupportCreole2 = true;
                this.SupportCreoleX = true;

                this.EnableArrows = true;
                this.EnableIcons = true;
                this.EnableSmileys = true;

                this.ImagePath = @"../img/";

                this.ExternalLinkImage = "out.png";
                this.DocumentLinkImage = "pdf.png";
                this.ArchiveLinkImage = "zip.png";

                this.ExternalLinkTarget = "_blank";
                this.LocalLinkFunction =
                    (space, link) => (string.IsNullOrEmpty(space) ? string.Empty : space + "/") + link;
                this.LocalImageFunction = source => source;

                this.InternalLinkSpaces = new Dictionary<string, string>
                    {
                        { "youtube", @"http://www.youtube.com/watch?v={0}" },
                        { "vimeo", @"http://vimeo.com/{0}" },
                        { "google", @"http://www.google.com/?q={0}" }
                    };
            }
        }

    }
}