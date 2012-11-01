namespace LynxToolkit.Documents
{
    using System.Text.RegularExpressions;

    public abstract class DocumentParser
    {
        protected Document doc;

        public DocumentParser()
        {
            doc = new Document();
        }

        public Document Document
        {
            get { return doc; }
        }

        /// <summary>
        /// Creates a regular expression.
        /// </summary>
        /// <param name="s">The expression.</param>
        /// <param name="multiline">Multiline mode. Changes the meaning of ^ and $ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.</param>
        /// <param name="singleline">Specifies single-line mode. Changes the meaning of the dot (.) so it matches every character (instead of every character except \n).</param>
        /// <returns>The compiled regular expression.</returns>
        public static Regex CreateRegex(string s, bool multiline = true, bool singleline = true)
        {
            var o = RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.CultureInvariant;
            if (multiline)
                o |= RegexOptions.Multiline;
            if (singleline)
                o |= RegexOptions.Singleline;
            return new Regex(s, o);
        }

    }
}