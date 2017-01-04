﻿using System;
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
        private IEnumerable<DateTime> Holidays;
        private HashSet<DateTime> HolidaysCache;
        private IEnumerable<WorkScheduleMove> OverWorkdays;
        private HashSet<DateTime> OverWorkdaysCache;
        
        private HashSet<DateTime> GetOverWorkdaysCache(IEnumerable<WorkScheduleMove> overWorkdays)
        {
            if (overWorkdays == null)
                return null;
            var dtHS = new HashSet<DateTime>(overWorkdays.Select(x => x.DateFrom.Date).Distinct());
            dtHS.UnionWith(overWorkdays.Select(x=>x.DateTo.Date).Distinct());
            return dtHS;
        }

        private HashSet<DateTime> GetHolidaysCache(IEnumerable<DateTime> holidays)
        {
            if (holidays == null)
                return null;
            return new HashSet<DateTime>(holidays.Distinct());
        }

        public bool IsHoliday(DateTime day)
        {
            return Holidays != null && Holidays.Contains(day);
        }

        public DateTime TestMove(DateTime day)
        {
            if (OverWorkdays == null || OverWorkdaysCache == null)
                return day;

            if (!OverWorkdaysCache.Contains(day.Date))
                return day;

            var from = OverWorkdays.Where(x => x.DateTo.Date == day.Date);
            if (from.Any())
                return from.First().DateTo;

            var to = OverWorkdays.Where(x => x.DateTo.Date == day.Date);
            if (to.Any())
                return to.First().DateTo;

            return day;
        }

        public bool IsSaturSunDay(DateTime day)
        {
            return (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday);
        }

        public bool IsRestday(DateTime day)
        {
            var d = TestMove(day);
            return IsSaturSunDay(d);
        }

        public bool IsPreHoliday(DateTime day)
        {
            var d = TestMove(day);
            return IsHoliday(d.AddDays(1));
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
        
        public ScheduleMonth(DateTime firstDay, IEnumerable<DateTime> holidays = null, IEnumerable<WorkScheduleMove> overWorkdays = null)
            : this(firstDay.Year, firstDay.Month, holidays, overWorkdays)
        {}
        
        public ScheduleMonth(int year, int month, IEnumerable<DateTime> holidays = null, IEnumerable<WorkScheduleMove> overWorkdays = null)
        {
            Holidays = holidays;
            HolidaysCache = GetHolidaysCache(holidays);
            OverWorkdays = overWorkdays;
            OverWorkdaysCache = GetOverWorkdaysCache(overWorkdays);
            

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
