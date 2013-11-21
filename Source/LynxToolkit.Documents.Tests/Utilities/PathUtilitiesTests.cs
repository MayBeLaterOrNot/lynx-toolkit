namespace LynxToolkit.Documents.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class PathUtilitiesTests
    {
        [Test]
        [TestCase("a.txt", ".csv", "a.csv")]
        [TestCase(".txt", ".csv", ".csv")]
        [TestCase("a.txt", "", "a.")]
        [TestCase("a.txt", ".", "a.")]
        [TestCase("a", ".csv", "a.csv")]
        [TestCase("a.", ".csv", "a.csv")]
        public void ChangeExtension(string input, string newExtension, string expectedOutput)
        {
            var result = PathUtilities.ChangeExtension(input, newExtension);
            Assert.AreEqual(expectedOutput, result);
            var result2 = System.IO.Path.ChangeExtension(input, newExtension);
            Assert.AreEqual(expectedOutput, result2);
        }

        [Test]
        [TestCase("a", "b", @"a\b")]
        public void Combine(string input, string newExtension, string expectedOutput)
        {
            var result = PathUtilities.Combine(input, newExtension);
            Assert.AreEqual(expectedOutput, result);
            var result2 = System.IO.Path.Combine(input, newExtension);
            Assert.AreEqual(expectedOutput, result2);
        }

        [Test]
        [TestCase(@"a\b", @"a")]
        public void GetDirectoryName(string input, string expectedOutput)
        {
            var result = PathUtilities.GetDirectoryName(input);
            Assert.AreEqual(expectedOutput, result);
            var result2 = System.IO.Path.GetDirectoryName(input);
            Assert.AreEqual(expectedOutput, result2);
        }
        [Test]
        [TestCase(@"a\b.txt", ".txt")]
        public void GetExtension(string input, string expectedOutput)
        {
            var result = PathUtilities.GetExtension(input);
            Assert.AreEqual(expectedOutput, result);
            var result2 = System.IO.Path.GetExtension(input);
            Assert.AreEqual(expectedOutput, result2);
        }

        [Test]
        [TestCase(@"\a", true)]
        public void IsPathRooted(string input, bool expectedOutput)
        {
            var result = PathUtilities.IsPathRooted(input);
            Assert.AreEqual(expectedOutput, result);
            var result2 = System.IO.Path.IsPathRooted(input);
            Assert.AreEqual(expectedOutput, result2);
        }
    }
}