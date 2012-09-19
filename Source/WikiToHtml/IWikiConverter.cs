namespace LynxToolkit
{
    public interface IWikiConverter
    {
        /// <summary>
        /// Transforms the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The transformed text.</returns>
        string Transform(string text);
    }
}