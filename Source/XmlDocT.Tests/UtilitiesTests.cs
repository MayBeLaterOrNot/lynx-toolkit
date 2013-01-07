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
            Assert.AreEqual("System.Int32", Utilities.GetXmlMemberParameterName(typeof(int)));
            Assert.AreEqual("System.Int32[]", Utilities.GetXmlMemberParameterName(typeof(int[])));
            Assert.AreEqual("System.Int32[0:,0:]", Utilities.GetXmlMemberParameterName(typeof(int[,])));
            Assert.AreEqual("System.Collections.Generic.List{System.Int32}@", Utilities.GetXmlMemberParameterName(typeof(List<int>)));
            Assert.AreEqual("System.Collections.Generic.IList{System.Int32}@", Utilities.GetXmlMemberParameterName(typeof(IList<int>)));
        }
    }
}
