namespace ExtractText
{
    using System.IO;
    using System.Text.RegularExpressions;

    class Program
    {
        private const string DefaultPattern = @"\n\#\#.*?\n(.*?)\n\#\#[^#]";

        static void Main(string[] args)
        {
            var inputFile = args[0];
            var outputFile = args[1];
            var pattern = args.Length > 2 ? args[2] : DefaultPattern;
            var input = File.ReadAllText(inputFile);
            var match = Regex.Match(input, pattern, RegexOptions.Singleline);
            if (match.Success)
            {
                File.WriteAllText(outputFile, match.Groups[1].Value.Trim());
            }
        }
    }
}
