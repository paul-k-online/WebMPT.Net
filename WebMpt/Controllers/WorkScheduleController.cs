using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPT.Model;
using WebMpt.Model.WorkSchedule;

namespace WebMpt.Controllers
{
    public class WorkScheduleController : Controller
    {
        //
        // GET: /ScheduleWork/
        public ActionResult Index(int? year = null, int? month = null)
        {
            /*
            var selectList = new List<SelectListItem>();
            var yy = DateTime.Now.Year;
            for (var i = yy; i < yy + 2; i++)
            {
                selectList.Add(new SelectListItem()
                               {
                                   Selected = i==yy,
                                   Text = i.ToString(),
                                   Value = i.ToString(),
                               });
            }
            ViewBag.SelectList = selectList;
            */

            if (year == null)
                year = DateTime.Now.Year;
            if (month == null)
                month = DateTime.Now.Month;
            var requestDate = new DateTime(year.Value, month.Value, 1);

            var sheduleMonthDates = new List<DateTime>();
            sheduleMonthDates.Add(requestDate.AddMonths(-1));
            sheduleMonthDates.Add(requestDate.AddMonths(0));
            sheduleMonthDates.Add(requestDate.AddMonths(1));
            sheduleMonthDates.Add(requestDate.AddMonths(2));
            sheduleMonthDates.Add(requestDate.AddMonths(3));

            var years = sheduleMonthDates.Select(x => x.Year).Distinct();

            Dictionary<DateTime, string> holidays = null;
            Dictionary<DateTime, DateTime> overWorkdays = null;

            using (var db = new MPTEntities())
            {
                holidays = db.GetHolidays(years.ToArray());
                overWorkdays = db.GetOverWorkdays(years.ToArray());
            }

            var data = sheduleMonthDates.Select(date => new ScheduleMonth(date, holidays, overWorkdays));
            return View(data);
        }
    }
}
