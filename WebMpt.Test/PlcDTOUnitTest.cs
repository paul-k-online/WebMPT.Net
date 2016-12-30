using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

using WebMpt.Model;

namespace WebMpt.Test
{
    [TestClass]
    public class PlcDTOUnitTest
    {
        [TestMethod]
        public void TestStringSplit()
        {
            CollectionAssert.AreEqual((new PlcEventListDTO {Numbers = "1, 2, 5 77"}).NumberList.ToList(), new[] { 1, 2, 5, 77 });
            
            CollectionAssert.AreEqual((new PlcEventListDTO {}).NumberList.ToList(), new int[] { });

            CollectionAssert.AreEqual((new PlcEventListDTO {Numbers = "as asd asd asd asd asd "}).NumberList.ToList(), new int[] { });
        }
    }
}