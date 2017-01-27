using System;
using System.Collections.Generic;

namespace WebMpt.Model.WorkSchedule
{
    /// <summary>
    /// день в табеле
    /// </summary>
    public class ScheduleDay
    {
        public DateTime DateTime { get; private set; }
        public bool IsHoliday { get; private set; }
        public bool IsRestday { get; private set; }
        public bool IsPreHoliday { get; private set; }
        public bool IsMove { get; private set; }
        public string ToolTip{ get; private set; }


        private readonly static ScheduleSmena[] SmenaDiff =
        {
            new ScheduleSmena(null, workHours: 12,  isNight: false),
            new ScheduleSmena(null, workHours: 12,  isNight: false),
            new ScheduleSmena(null, workHours: 0,   isNight: false),
            new ScheduleSmena(null, workHours: 4,   isNight: true),
            new ScheduleSmena(null, workHours: 12,  isNight: true),
            new ScheduleSmena(null, workHours: 8,   isNight: true),
            new ScheduleSmena(null, workHours: 0,   isNight: false),
            new ScheduleSmena(null, workHours: 0,   isNight: false),
        };

        private static int GetDiffSh(DateTime day, SmenaName smena)
        {
            // начало отсчета смен
            // А: 2011.01.(3)
            // Б: 2011.01.(5)
            // В: 2011.01.(1)
            // Г: 2011.01.(7)
            var totalDays = (day.Date.Subtract(new DateTime(2011, 01, (int)smena))).TotalDays;
            return (int)(totalDays % 8);
        }

        private readonly Dictionary<SmenaName, ScheduleSmena> _smenaList = new Dictionary<SmenaName, ScheduleSmena>();
        public ScheduleSmena this[SmenaName key]
        {
            get { return _smenaList.ContainsKey(key) ? _smenaList[key] : null; }
        }
        

        public ScheduleDay(DateTime day, bool isHoliday = false, bool isPreHoliday = false, bool isRestday = false, bool isMove=false, string tooltip = "")
        {
            DateTime = day;
            IsHoliday = isHoliday;
            IsPreHoliday = isPreHoliday;
            IsRestday = isRestday;
            IsMove = isMove;
            ToolTip = tooltip;

            foreach (SmenaName smenaName in Enum.GetValues(typeof(SmenaName)))
            {
                if (smenaName == SmenaName.Day)
                {
                    uint h = 8;
                    if (IsPreHoliday) h = 7;
                    if (IsRestday || IsHoliday) h = 0;
                    _smenaList.Add(smenaName, new ScheduleSmena(this, h));
                    continue;
                }
                var diff = GetDiffSh(DateTime, smenaName);
                var smena = new ScheduleSmena(this, SmenaDiff[diff].WorkHours, SmenaDiff[diff].IsNight, smenaName);
                _smenaList.Add(smenaName, smena);
            }
        }

        public ScheduleDay(int year, int month, int day) :
            this(new DateTime(year, month, day))
        {}

        public int Day
        {
            get { return DateTime.Day; }
        }
        public int Month
        {
            get { return DateTime.Month; }
        }
        public string DayName
        {
            get { return DateTime.ToString("ddd"); }
        }
        public override string ToString()
        {
            return DateTime.ToShortDateString();
        }
    }
}
