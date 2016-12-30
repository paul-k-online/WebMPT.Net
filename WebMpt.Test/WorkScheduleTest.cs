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
        HashSet<DateTime> holidays = new HashSet<DateTime>()
            {
                new DateTime(2015, 03, 08)
            };



        [TestMethod]
        public void TestScheduleDay()
        {
            var month = new ScheduleMonth(2015, 03, holidays);
            var day = month.GetScheduleDay(08);

            Assert.AreEqual(day.IsRestday, true);
            Assert.AreEqual(day.IsHoliday, true);
            Assert.AreEqual(day.IsPreHoliday, false);

            var smenDay = day[SmenaName.Day];
            Assert.AreEqual(smenDay.WorkHours, 0);
            Assert.AreEqual(smenDay.IsNight, false);

            var smenA = day[SmenaName.A];
            Assert.AreEqual(smenA.WorkHours, 0);
            Assert.AreEqual(smenA.IsNight, false);

            var smenaB = day[SmenaName.B];
            Assert.AreEqual(smenaB.WorkHours, 12);
            Assert.AreEqual(smenaB.IsNight, true);

            var smenaC = day[SmenaName.V];
            Assert.AreEqual(smenaC.WorkHours, 12);
            Assert.AreEqual(smenaC.IsNight, false);

            var smenaD = day[SmenaName.G];
            Assert.AreEqual(smenaD.WorkHours, 0);
            Assert.AreEqual(smenaD.IsNight, false);
        }


        [TestMethod]
        public void TestPreHoliday()
        {
            var month = new ScheduleMonth(2015, 03, holidays);
            var day = new ScheduleDay(2015, 03, 07);
            
            Assert.AreEqual(day.IsRestday, true);
            Assert.AreEqual(day.IsPreHoliday, true);
            Assert.AreEqual(day.IsHoliday, false);

            var smenDay = day[SmenaName.Day];
            Assert.AreEqual(smenDay.WorkHours, 0);
            Assert.AreEqual(smenDay.IsNight, false);
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
