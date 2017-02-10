using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MPT.Model;
using WebMpt.Model.WorkSchedule;

namespace WebMpt.Test.WorkSchedule
{
    [TestClass]
    public class WorkScheduleTest
    {
        Dictionary<DateTime, string> holidays = new Dictionary<DateTime, string>()
        {
                { new DateTime(2015, 03, 08), "день баб" }
        };

        [TestMethod]
        public void TestScheduleDay()
        {
            var month = new ScheduleMonth(2017, 02, holidays);
            var day = month.GetScheduleDay(10);

            Assert.IsFalse(day.IsRestday);
            Assert.IsFalse(day.IsHoliday);
            Assert.IsFalse(day.IsPreHoliday);

            var smenDay = day[SmenaName.Day];
            Assert.AreEqual((int)smenDay.WorkHours, 8);
            Assert.IsFalse(smenDay.IsNight);

            var smenA = day[SmenaName.A];
            Assert.AreEqual((int)smenA.WorkHours, 0);
            Assert.IsFalse(smenA.IsNight);

            var smenaB = day[SmenaName.B];
            Assert.AreEqual((int)smenaB.WorkHours, 12);
            Assert.IsTrue(smenaB.IsNight);

            var smenaC = day[SmenaName.V];
            Assert.AreEqual((int)smenaC.WorkHours, 12);
            Assert.IsFalse(smenaC.IsNight);

            var smenaD = day[SmenaName.G];
            Assert.AreEqual((int)smenaD.WorkHours, 0);
            Assert.IsFalse(smenaD.IsNight);
        }


        [TestMethod]
        public void TestPreHoliday()
        {
            var holidays = new Dictionary<DateTime, string> { { new DateTime(2015, 03, 08), "бабский день" } };

            var month = new ScheduleMonth(2015, 03, holidays);
            var day = month.GetScheduleDay(07);
            
            Assert.IsTrue(day.IsRestday);
            Assert.IsFalse(day.IsHoliday);
            Assert.IsTrue(day.IsPreHoliday);

            var smenDay = day[SmenaName.Day];
            Assert.AreEqual((int)smenDay.WorkHours, 0);
            Assert.IsFalse(smenDay.IsNight);
        }

        [TestMethod]
        public void OrderAttributeTest()
        {
            List<SmenaName> l = new List<SmenaName>
            {
                {SmenaName.A},
                {SmenaName.B},
                {SmenaName.V},
                {SmenaName.G},
                {SmenaName.Day},
            };

            var comparer = new SmenaNameByOrderAttribyteComparer();
            l.Sort(comparer);
        }
    }
}
