using System;
using System.ComponentModel.DataAnnotations;

namespace WebMpt.Model
{
    public class PlcEventDTO
    {
        public int Id { get; set; }
        public int LogPos { get; set; }

        [Display(Name = "Цех")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public int FactoryNumber { get; set; }

        [Display(Name = "Котроллер")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public string PlcName { get; set; }
        


        [Display(Name = "Дата")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd  HH:mm:ss.fff}", ApplyFormatInEditMode = false)]
        public DateTime DateTime { get; set; }

        [Display(Name = "Дата")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
        public DateTime Date { get { return DateTime; } }

        [Display(Name = "Время")]
        [DisplayFormat(DataFormatString = "{0:HH:mm:ss.fff}", ApplyFormatInEditMode = false)]
        public DateTime Time { get { return DateTime; } }
        
        [Display(Name = "Номер")]
        public int Number { get; set; }

        [Display(Name = "Сообщение")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "Сообщение не найдено")]
        public string Message { get; set; }
        
        
        public int Code { get; set; }

        [Display(Name = "Код")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public string StringCode { get; set; }
        
        [Display(Name = "Значение")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public string StringValue { get; set; }

        [Display(Name = "Значение")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public double Value { get; set; }

        public short SeverityNumber { get; set; }
        public string Severity { get; set; }

        public enum SeverityBootstarpEnum
        {
            Alarm = 2,
            Warning = 3,
            Normal = 4,
            Info = 6,
            Trace = 7,
            Debug = 8
        }

        [Display(Name = "Группа")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public short? Group { get; set; }

    }
}