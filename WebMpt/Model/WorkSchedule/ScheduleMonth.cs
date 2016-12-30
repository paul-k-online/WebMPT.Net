using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace WebMpt.Model.WorkSchedule
{
    /// <summary>
    /// табельный месяц
    /// </summary>
    public class ScheduleMonth
    {
        public DateTime FirstDay { get; private set; }
        public HashSet<DateTime> Holidays { get; private set; }
        public HashSet<DateTime> OverWorkdays { get; private set; }

        public bool IsHoliday(DateTime day)
        {
            return Holidays != null && Holidays.Contains(day);
        }
        public bool IsRestday(DateTime day)
        {
            return (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday) 
                && (
                        OverWorkdays == null || 
                        (OverWorkdays != null && !OverWorkdays.Contains(day))
                    );
        }
        public bool IsPreHoliday(DateTime day)
        {
            return IsHoliday(day.AddDays(1));
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
            return new ScheduleDay(day, IsHoliday(day), IsPreHoliday(day), IsRestday(day));
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
        
        public ScheduleMonth(DateTime firstDay, HashSet<DateTime> holidays = null, HashSet<DateTime> overWorkdays = null)
            : this(firstDay.Year, firstDay.Month, holidays, overWorkdays)
        {}
        
        public ScheduleMonth(int year, int month, HashSet<DateTime> holidays = null, HashSet<DateTime> overWorkdays = null)
        {
            this.Holidays = holidays;
            this.OverWorkdays = overWorkdays;

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
