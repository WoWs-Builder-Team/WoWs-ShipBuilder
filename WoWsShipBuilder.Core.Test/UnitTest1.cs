using NUnit.Framework;
using System.Diagnostics;

namespace WoWsShipBuilder.Core.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            //Assert.Pass();
            Debug.Assert(false, "test");
        }
    }
}
