using Microsoft.VisualStudio.TestTools.UnitTesting;

using WebMpt.Model;

namespace WebMpt.Test
{
    [TestClass]
    public class MPTEntitiesExtTest
    {
        [TestMethod]
        public void SoftRoundTest()
        {
            var privateType = new PrivateType(typeof(MPTEntitiesExt));

            Assert.AreEqual(MPTEntitiesExt.SoftRound(0), 0);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(1), 1);

            Assert.AreEqual(MPTEntitiesExt.SoftRound(1234.1234), 1234);

            Assert.AreEqual(MPTEntitiesExt.SoftRound(123.1934), 123.2);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(123.123), 123.1);

            Assert.AreEqual(MPTEntitiesExt.SoftRound(1.123, 2), 1.12);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(1.123), 1.123);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(1.1234), 1.123);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(1.12345), 1.123);

            Assert.AreEqual(MPTEntitiesExt.SoftRound(12.123), 12.12);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(12.789), 12.79);

            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.1), 0.1);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.123), 0.123);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.12378), 0.124);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.012378), 0.0124);

            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.01), 0.01);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.012), 0.012);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.01278), 0.0128);

            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.001), 0.001);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.0012), 0.0012);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.001278), 0.0013);

            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.0001), 0.0001);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.00012), 0.00012);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.0001278), 0.00013);


            Assert.AreEqual(MPTEntitiesExt.SoftRound(-1.54877), -1.549);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(-0.54877), -0.549);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(-0.056789), -0.057);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.056789), 0.057);

            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.56789, 2), 0.57);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.056789, 2), 0.06);
            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.0056789, 2), 0.006);

            Assert.AreEqual(MPTEntitiesExt.SoftRound(0.0056789, 0), 0.006);
        }
    }
}
