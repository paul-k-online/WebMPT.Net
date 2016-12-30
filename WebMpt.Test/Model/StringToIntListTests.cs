using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WebMpt.Model;

namespace WebMpt.Model.Tests
{
    [TestClass()]
    public class StringToIntListTests
    {
        [TestMethod()]
        public void SplitToIntListTest()
        {
            var a1 = "10 9 8 7";
            var b1 = StringToIntList.SplitToIntList(a1).ToArray();
            Assert.AreEqual(b1[0], 10);


            var a2 = "-10-9-8 7";
            var b2 = StringToIntList.SplitToIntList(a1).ToArray();
            Assert.AreEqual(b1[0], 10);

        }
    }
}