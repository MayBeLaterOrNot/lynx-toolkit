using System.Collections.Generic;

namespace XmlDocT.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class UtilitiesTests
    {
        [Test]
        public void GetNiceParameterName()
        {
            Assert.AreEqual("System.Int32", XmlUtilities.GetXmlMemberParameterName(typeof(int)));
            Assert.AreEqual("System.Int32[]", XmlUtilities.GetXmlMemberParameterName(typeof(int[])));
            Assert.AreEqual("System.Int32[0:,0:]", XmlUtilities.GetXmlMemberParameterName(typeof(int[,])));
            Assert.AreEqual("System.Collections.Generic.List{System.Int32}@", XmlUtilities.GetXmlMemberParameterName(typeof(List<int>)));
            Assert.AreEqual("System.Collections.Generic.IList{System.Int32}@", XmlUtilities.GetXmlMemberParameterName(typeof(IList<int>)));
        }
    }
}
