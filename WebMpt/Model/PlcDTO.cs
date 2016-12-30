using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using System.Text.RegularExpressions;

using MPT.Model;

namespace WebMpt.Model
{
    public static class StringToIntList
    {
        private static Regex SplitNumbersRegex = new Regex(@"\D+", RegexOptions.Compiled);
        public static IEnumerable<int> SplitToIntList(string str)
        {
            if (str == null) throw new ArgumentNullException("list");
            var numberList = SplitNumbersRegex.Split(str);
            return numberList.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Convert.ToInt32(x)).ToList();
        }
    }


    public class PlcDTO
    {
        public int Id { get; set; }

        [Display(Name = "Проект")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string FullName { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Дата последнего")]
        public DateTime? LastEventDateTime { get; set; }

        public Factory Factory { get; set; }

        public int? OrderIndex { get; set; }

        [Display(Name = "Сегодня")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public int? TodayCount { get; set; }

        public bool HasWarningToday { get; set; }

        public int ProtocolType { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Id, Name);
        }
    }


    public class PlcEventListDTO
    {
        //public const string DateFormat = "yyyy-MM-dd";

        public PLC Plc { get; set; }

        public IEnumerable<PlcEventDTO> EventList { get; set; }

        [Display(Name="Дата по")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [HiddenInput(DisplayValue = true)]
        public DateTime DateEnd { get; set; }

        [Display(Name = "Дата c")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [HiddenInput(DisplayValue = true)]
        public DateTime DateBegin { get; set; }
        
        private void MoveDate(int days=1)
        {
            DateEnd = DateEnd.AddDays(days);
            DateBegin = DateBegin.AddDays(days);
        }

        public void NextDate()
        {
            MoveDate(1);
        }

        public void PrevDate()
        {
            MoveDate(-1);
        }


        [Display(Name = "Спрятать обрывы")]
        public bool HideBreak { get; set; }

        [Display(Name = "Показать группы")]
        public bool ShowGroup{ get; set; }

        [Display(Name = "Обратная хронология")]
        public bool SortOrderDesc { get; set; }

        [Display(Name = "Найти по номерам")]
        public string Numbers { get; set; }

        public IEnumerable<int> NumberList
        {
            get { return StringToIntList.SplitToIntList(Numbers); }
        }

        [Display(Name = "Найти текст")]
        public string Message { get; set; }
    }
}
