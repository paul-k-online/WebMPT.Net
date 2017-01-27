using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MPT.Model;

namespace WebMpt.Model.WorkSchedule
{
    /// <summary>
    /// табельный месяц
    /// </summary>
    public class ScheduleMonth
    {
        public DateTime FirstDay { get; private set; }
        private Dictionary<DateTime, string> Holidays;
        private Dictionary<DateTime, DateTime> OverWorkdays;
        
        public bool IsHoliday(DateTime day)
        {
            return Holidays != null && Holidays.ContainsKey(day);
        }

        public bool IsMove(DateTime day)
        {
            return OverWorkdays != null && OverWorkdays.ContainsKey(day.Date);
        }

        public DateTime GetMoveDate(DateTime day)
        {
            if (!IsMove(day))
                return day;
            return OverWorkdays[day.Date];
        }
        public bool IsSaturSunDay(DateTime day)
        {
            return (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday);
        }
        public bool IsRestday(DateTime day)
        {
            var day1 = GetMoveDate(day);
            return IsSaturSunDay(day1);
        }
        public bool IsPreHoliday(DateTime day)
        {
            var day1 = GetMoveDate(day);
            return IsHoliday(day1.AddDays(1));
        }

        public string GetTooltip(DateTime day)
        {
            if (Holidays != null && Holidays.ContainsKey(day))
                return Holidays[day];

            if (OverWorkdays != null && OverWorkdays.ContainsKey(day.Date))
                return string.Format("перенос: {0:d MMMM}", OverWorkdays[day.Date]);

            return null;
        }

        public string MonthName
        {
            get { return FirstDay.ToString("MMMM"); }
        }
        public string FullName
        {
            get { return FirstDay.ToString("yyyy MMMM"); }

        }
        public override string ToString()
        {
            return FullName;
        }


        public ScheduleDay GetScheduleDay(int dayNumber)
        {
            var day = FirstDay.AddDays(dayNumber-1);
            if (day.Month != FirstDay.Month)
                return null;
            return new ScheduleDay(day, IsHoliday(day), IsPreHoliday(day), IsRestday(day), IsMove(day),  GetTooltip(day));
        }
        
        public IEnumerable<ScheduleDay> Days { get; private set; }

        public SortedDictionary<SmenaName, ScheduleMonthSmenaProperties> SmenaMonthPropertieses
                    = new SortedDictionary<SmenaName, ScheduleMonthSmenaProperties>(new SmenaNameByOrderAttribyteComparer())
                                                {
                                                    {SmenaName.Day, new ScheduleMonthSmenaProperties()},
                                                    {SmenaName.B, new ScheduleMonthSmenaProperties()},
                                                    {SmenaName.A, new ScheduleMonthSmenaProperties()},
                                                    {SmenaName.V, new ScheduleMonthSmenaProperties()},
                                                    {SmenaName.G, new ScheduleMonthSmenaProperties()},
                                                };
        public ScheduleMonthSmenaProperties this[SmenaName smenaName]
        {
            get { return SmenaMonthPropertieses[smenaName]; }
        }
        
        public ScheduleMonth(DateTime firstDay, Dictionary<DateTime,string> holidays = null, Dictionary<DateTime, DateTime> overWorkdays = null)
            : this(firstDay.Year, firstDay.Month, holidays, overWorkdays)
        {}
        
        public ScheduleMonth(int year, int month, Dictionary<DateTime, string> holidays = null, Dictionary<DateTime, DateTime> overWorkdays = null)
        {
            Holidays = holidays;
            OverWorkdays = overWorkdays;
            

            FirstDay = new DateTime(year, month, 1);
            Days = Enumerable.Range(1, 31).Select(GetScheduleDay).Where(day => (day != null));

            var smenaMonthDay = SmenaMonthPropertieses[(int)SmenaName.Day];
            foreach (var smenaMonthKv in SmenaMonthPropertieses.OrderBy(x=>x.Key))
            {
                var smenaName = smenaMonthKv.Key;
                var smenaMonth = smenaMonthKv.Value;
                smenaMonth.TotalHours = Days.Sum(day => day[smenaName].WorkHours);
                smenaMonth.WorkDayCount = Days.Count(day => day[smenaName].WorkHours != 0);
                smenaMonth.NightHours = Days.Where(day => day[smenaName].IsNight).Sum(day => day[smenaName].WorkHours);
                smenaMonth.HolidayHours = Days.Where(day => day[smenaName].IsHoliday).Sum(day => day[smenaName].WorkHours);
                smenaMonth.OverHours = smenaMonth.TotalHours - smenaMonthDay.TotalHours;
            }
        }
    }
}
