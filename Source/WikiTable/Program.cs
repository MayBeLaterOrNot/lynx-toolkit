//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="LynxToolkit">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace LynxToolkit
{
    using System;
    using System.Text;
    using System.Windows.Forms;

    class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Console.WriteLine(Application.Header);
            var input = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No text on clipboard.");
                return 1;
            }
            var output = Convert(input);
            Clipboard.SetText(output);
            return 0;
        }

        private static string Convert(string text, bool includeHeader = true)
        {
            var sb = new StringBuilder();
            foreach (var l in text.Split('\r'))
            {
                var line = l.Trim();
                if (line.Length == 0) continue;
                var fields = line.Split('\t');
                sb.Append(includeHeader ? "||" : "|");
                foreach (var field in fields)
                {
                    sb.Append(" ");
                    sb.Append(field.Trim());
                    sb.Append(includeHeader ? " ||" : " |");
                }
                sb.AppendLine();
                includeHeader = false;
            }
            return sb.ToString();
        }
    }
}
