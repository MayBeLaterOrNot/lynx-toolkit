namespace PropertyCG
{
    using System.Text;

    public class StringBuilderEx
    {
        private StringBuilder sb;

        public StringBuilderEx(int capacity = 1000)
        {
            this.sb = new StringBuilder(capacity);
        }

        private string indent = "";
        public void Indent()
        {
            this.indent += "    ";
        }
        public void Unindent()
        {
            this.indent = this.indent.Substring(4);
        }
        public void Append(object s)
        {
            this.sb.Append(s);
        }
        public void AppendLine(string format = null, params object[] args)
        {
            if (format != null)
            {
                this.sb.Append(this.indent);
                if (args.Length > 0)
                    this.sb.AppendFormat(format, args);
                else this.sb.Append(format);
            }
            this.sb.AppendLine();
        }
        public override string ToString()
        {
            return this.sb.ToString();
        }
    }
}