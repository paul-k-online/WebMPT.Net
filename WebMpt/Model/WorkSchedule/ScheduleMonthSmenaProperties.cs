using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebMpt.Model.WorkSchedule
{
    public class ScheduleMonthSmenaProperties
    {
        [Display(Name = "Всего")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public long TotalHours { get; set; }

        [Display(Name = "Подм.")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public long OverHours { get; set; }

        [Display(Name = "Дни")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public long WorkDayCount { get; set; }

        [Display(Name = "Ноч.")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public long NightHours { get; set; }

        [Display(Name = "Празд.")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public long HolidayHours { get; set; }
    }
}