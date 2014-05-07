namespace CleanSource.Tests
{
    using NUnit.Framework;

    public class DocumentationCommentsTests
    {
        [Test]
        public void SummaryTests()
        {
            Assert.AreEqual("<summary>\r\nabc\r\n</summary>", new DocumentationComments("<summary>abc</summary>").ToString());
            Assert.AreEqual("<summary>\r\nabc\r\n</summary>", new DocumentationComments("<summary> abc </summary>").ToString());
            Assert.AreEqual("<summary>\r\nabc\r\n</summary>", new DocumentationComments("<summary>\r\nabc\r\n</summary>").ToString());
        }

        [Test]
        public void ParamTests()
        {
            Assert.AreEqual("<param name=\"a\">ABC</param>", new DocumentationComments("<param name=\"a\">ABC</param>").ToString());
            Assert.AreEqual("<param name=\"a\">ABC</param>", new DocumentationComments("<param name=\"a\"> ABC </param>").ToString());
            Assert.AreEqual("<param name=\"a\">ABC</param>", new DocumentationComments("<param name=\"a\">\r\nABC\r\n</param>").ToString());
            Assert.AreEqual("<param name=\"a\">ABC</param>", new DocumentationComments("<param name=\"a\" >ABC</param>").ToString());
            Assert.AreEqual("<param name=\"a\">ABC</param>", new DocumentationComments("<param  name = \"a\" >ABC</param>").ToString());
        }

        [Test]
        public void TypeParamTests()
        {
            Assert.AreEqual("<typeparam name=\"T\">Type</typeparam>", new DocumentationComments("<typeparam name=\"T\">Type</typeparam>").ToString());
        }

        [Test]
        public void ExceptionTests()
        {
            Assert.AreEqual("<exception cref=\"System.Exception\">An exception</exception>", new DocumentationComments("<exception cref=\"System.Exception\">An exception</exception>").ToString());
            Assert.AreEqual("<exception cref=\"System.Exception\">An exception</exception>", new DocumentationComments("<exception  cref = \"System.Exception\" > An exception  </exception>").ToString());
        }

        [Test]
        public void SeeAlsoTests()
        {
            Assert.AreEqual("<seealso cref=\"x\">y</seealso>", new DocumentationComments("<seealso cref=\"x\">y</seealso>").ToString());
            Assert.AreEqual("<seealso cref=\"x\">y</seealso>", new DocumentationComments("<seealso  cref = \"x\" > y </seealso>").ToString());
            Assert.AreEqual("<seealso cref=\"x\" />", new DocumentationComments("<seealso  cref = \"x\"/>").ToString());
            Assert.AreEqual("<seealso cref=\"x\" />", new DocumentationComments("<seealso  cref = \"x\"></seealso>").ToString());
        }
    }
}
