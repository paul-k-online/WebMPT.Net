using System.Collections.Generic;
using System.ComponentModel;
using MPT.PrimitiveType;

namespace WebMpt.Model.WorkSchedule
{
    public enum SmenaName
    {
        [Description("Дн")]
        [Order(0)]
        Day = 0,
        [Description("Б")]
        [Order(1)]
        B = 5,
        [Description("А")]
        [Order(2)]
        A = 3,
        [Description("В")]
        [Order(3)]
        V = 1,
        [Description("Г")]
        [Order(4)]
        G = 7,
    }

    public class SmenaNameByOrderAttribyteComparer : IComparer<SmenaName>
    {
        public int Compare(SmenaName x, SmenaName y)
        {
            var xOrderValue = x.GetOrderValueFromEnumValue() ?? int.MaxValue;
            var yOrderValue = y.GetOrderValueFromEnumValue() ?? int.MaxValue;
            return xOrderValue.CompareTo(yOrderValue);
        }
    }



    public class ScheduleSmena
    {
        public SmenaName Smena { get; private set; }
        public ScheduleDay Day { get; private set; }
        public uint WorkHours { get; private set; }
        public bool IsNight { get; private set; }
        public bool IsRestday { get { return Day.IsRestday; } }
        public bool IsHoliday { get { return Day.IsHoliday; } }
        public bool IsPreholyday { get { return Day.IsPreHoliday; } }

        public ScheduleSmena(ScheduleDay day, uint workHours = 8, bool isNight = false, SmenaName smenaName = SmenaName.Day)
        {
            Day = day;
            WorkHours = workHours;
            IsNight = isNight;
            Smena = smenaName;
        }

        public string GetCssClass()
        {
            var l = new List<string>();
            if (Smena != SmenaName.Day)
                l.Add("Smena");
            if (IsRestday)
                l.Add("IsRestday");
            if (IsHoliday)
                l.Add("IsHolyday");
            if (IsPreholyday)
                l.Add("IsPreholyday");
            if (IsNight)
                l.Add("IsNight");
            return string.Join(" ", l);
        }
    }
}