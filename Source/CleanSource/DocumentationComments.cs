namespace CleanSource
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public class DocumentationComments
    {
        public DocumentationComments(string xml, string followingLine = "")
        {
            this.Summary = this.GetElement(xml, "summary");
            this.Value = this.GetElement(xml, "value");
            this.Returns = this.GetElement(xml, "returns");
            this.Remarks = this.GetElement(xml, "remarks");
            this.Example = this.GetElement(xml, "example");

            this.Parameters = new List<ParameterComment>();
            foreach (Match match in Regex.Matches(xml, "<param\\s+name\\s*=\\s*\"(\\w+?)\"\\s*>(.*?)</param>", RegexOptions.Singleline))
            {
                this.Parameters.Add(new ParameterComment(match.Groups[1].Value, this.Clean(match.Groups[2].Value.Trim())));
            }

            this.TypeParameters = new List<ParameterComment>();
            foreach (Match match in Regex.Matches(xml, "<typeparam\\s+name\\s*=\\s*\"(\\w+?)\"\\s*>(.*?)</typeparam>", RegexOptions.Singleline))
            {
                this.TypeParameters.Add(new ParameterComment(match.Groups[1].Value, this.Clean(match.Groups[2].Value.Trim())));
            }

            this.Exceptions = new List<ReferenceComment>();
            foreach (Match match in Regex.Matches(xml, "<exception\\s+cref\\s*=\\s*\"(.+?)\"\\s*>(.*?)</exception>", RegexOptions.Singleline))
            {
                this.Exceptions.Add(new ReferenceComment(match.Groups[1].Value, this.Clean(match.Groups[2].Value.Trim())));
            }

            this.SeeAlsoComments = new List<ReferenceComment>();
            foreach (Match match in Regex.Matches(xml, "<seealso\\s+cref\\s*=\\s*\"(.+?)\"\\s*>(.*?)</seealso>", RegexOptions.Singleline))
            {
                this.SeeAlsoComments.Add(new ReferenceComment(match.Groups[1].Value, this.Clean(match.Groups[2].Value.Trim())));
            }

            foreach (Match match in Regex.Matches(xml, "<seealso\\s+cref\\s*=\\s*\"(.+?)\"\\s*/>", RegexOptions.Singleline))
            {
                this.SeeAlsoComments.Add(new ReferenceComment(match.Groups[1].Value, null));
            }

            this.Permissions = new List<ReferenceComment>();

            // dependency properties
            var dependencyPropertyMatch = Regex.Match(
                followingLine,
                @"\s*public static readonly DependencyProperty (?<name>.*?)Property");
            if (dependencyPropertyMatch.Success)
            {
                this.Summary = "Identifies the <see cref=\"" + dependencyPropertyMatch.Groups[1].Value
                               + "\"/> dependency property.";
            }

            // routed events
            var routedEventMatch = Regex.Match(
                followingLine,
                @"\s*public static readonly RoutedEvent (?<name>.*?)Event");
            if (routedEventMatch.Success)
            {
                this.Summary = "Identifies the <see cref=\"" + routedEventMatch.Groups[1].Value + "\"/> routed event.";
            }
        }

        public string Summary { get; private set; }
        public List<ParameterComment> TypeParameters { get; private set; }
        public List<ParameterComment> Parameters { get; private set; }
        public List<ReferenceComment> Exceptions { get; private set; }
        public List<ReferenceComment> Permissions { get; private set; }
        public List<ReferenceComment> SeeAlsoComments { get; private set; }
        public string Value { get; private set; }
        public string Returns { get; private set; }
        public string Remarks { get; private set; }
        public string Example { get; private set; }
        public IncludeComment Include { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (this.Summary != null)
            {
                sb.AppendLine("<summary>");
                sb.AppendLine(this.Summary);
                sb.AppendLine("</summary>");
            }

            if (this.Value != null)
            {
                sb.AppendLine("<value>" + this.Value + "</value>");
            }

            foreach (var p in this.TypeParameters)
            {
                sb.AppendLine("<typeparam name=\"" + p.Name + "\">" + p.Description + "</typeparam>");
            }

            foreach (var p in this.Parameters)
            {
                sb.AppendLine("<param name=\"" + p.Name + "\">" + p.Description + "</param>");
            }

            if (this.Returns != null)
            {
                sb.AppendLine("<returns>");
                sb.AppendLine(this.Returns);
                sb.AppendLine("</returns>");
            }

            foreach (var e in this.SeeAlsoComments)
            {
                if (string.IsNullOrEmpty(e.Description))
                {
                    sb.AppendLine("<seealso cref=\"" + e.Reference + "\" />");
                }
                else
                {
                    sb.AppendLine("<seealso cref=\"" + e.Reference + "\">" + e.Description + "</seealso>");
                }
            }

            foreach (var e in this.Exceptions)
            {
                sb.AppendLine("<exception cref=\"" + e.Reference + "\">" + e.Description + "</exception>");
            }

            foreach (var e in this.Permissions)
            {
                sb.AppendLine("<permission cref=\"" + e.Reference + "\">" + e.Description + "</permission>");
            }

            if (this.Example != null)
            {
                sb.AppendLine("<example>" + this.Example + "</example>");
            }

            if (this.Remarks != null)
            {
                sb.AppendLine("<remarks>" + this.Remarks + "</remarks>");
            }

            return sb.ToString().Trim();
        }

        private string GetElement(string input, string name)
        {
            var match = Regex.Match(input, "<" + name + ">(.*?)</" + name + ">", RegexOptions.Singleline);
            if (match.Success)
            {
                return this.Clean(match.Groups[1].Value.Trim());
            }

            return null;
        }

        private string Clean(string c)
        {
            c = c.Replace("\"/>", "\" />");
            c = Regex.Replace(c, @"(<c>\s*)?\b(true|false|null)\b(\s*</c>)?", "<c>$2</c>");
            return c;
        }

        public class IncludeComment
        {
            public string FileName { get; set; }
            public string Path { get; set; }
        }

        public class ParameterComment
        {
            public ParameterComment(string name, string description)
            {
                this.Name = name;
                this.Description = description;
            }

            public string Name { get; private set; }

            public string Description { get; private set; }
        }

        public class ReferenceComment
        {
            public ReferenceComment(string reference, string description)
            {
                this.Reference = reference;
                this.Description = description;
            }

            public string Reference { get; private set; }
            public string Description { get; private set; }
        }
    }
}