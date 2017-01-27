using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

using MPT.Model;
using MPT.PrimitiveType;

namespace WebMpt.Model
{
    public struct PlcEventDTO
    {
        [Display(Name = "����")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd  HH:mm:ss.fff}", ApplyFormatInEditMode = false)]
        public DateTime DateTime { get; set; }

        [Display(Name = "����")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
        public DateTime Date { get { return DateTime; } }

        [Display(Name = "�����")]
        [DisplayFormat(DataFormatString = "{0:HH:mm:ss.fff}", ApplyFormatInEditMode = false)]
        public DateTime Time { get { return DateTime; } }
        
        [Display(Name = "�����")]
        public int Number { get; set; }

        [Display(Name = "���������")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "��������� �� �������")]
        public string Message { get; set; }
        
        public int Code { get; set; }
        [Display(Name = "���")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public string CodeString { get; set; }

        public bool ShowValue { get; set; }
        [Display(Name = "��������")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public double? Value { get; set; }
        [Display(Name = "��������")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public string ValueString {
            get
            {
                if (!ShowValue || Value == null)
                    return "";
                return DoubleExtension.SoftRound(Value.Value).ToString(CultureInfo.InvariantCulture);
            }
        }

        public short SeverityNumber { get; set; }

        [Display(Name = "������")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "&nbsp;", HtmlEncode = false)]
        public short? Group { get; set; }


        private static Dictionary<string, int> CodeNumbers = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"L",               1},
            {"LL",              2},
            {"H",               4},
            {"HH",              8},
            {"�����",           16},
            {"BREAK",           16},
            {"L OUT",           33},
            {"LL OUT",          34},
            {"H OUT",           36},
            {"HH OUT",          40},
            {"BREAK OUT",       48},
            {"����� �� ������", 48},
            {"�����",           64},
            {"���������",       65},
            {"����",            70},
            {"info",            70},
        };


        private static Regex removeSpacesRegex = new Regex("[ ]{2,}", RegexOptions.None);
        public static PlcEventDTO MapPlcEventOld(PlcEventOld plcEvent, Dictionary<int, PlcEventCode> eventCodeDict)
        {
            var plcEventDTO = new PlcEventDTO()
            {
                DateTime = plcEvent.DateTime.AddMilliseconds(plcEvent.Msec),
                Number = plcEvent.n,
                Message = plcEvent.Message,

                Value = plcEvent.Value,
                ShowValue = true,

                //FactoryNumber = plcEvent.PLC.Factory.Number.Value,
                //PlcName = plcEvent.PLC.Description,
                Group = null,
            };

            //P2659     L     OUT   Pelletizer Die flange melt press
            //Z-1/2     OFF         TWS valve to bypass (LCP switch)
            //Z-1/2     ON          Horn reset (LCP switch)
            //          OFF         Coc�.��o����o�o�o ��a�a�a 215/2D
            //          AUTOMAT     P���� ���a��e��� ��a�a��� 215/1�
            //H215A     OPEN        �e���.��o����o�o�o ��a�a�a 215/1� 
            //������������ ������������ ���-�� ���-��� �� ������ ������

            var Message = plcEventDTO.Message;
            if (!string.IsNullOrEmpty(Message) && Message.Length >= 22)
            {
                var reason1 = Message.Substring(10, 12).Trim();
                plcEventDTO.CodeString = removeSpacesRegex.Replace(reason1, " ").Trim();
                var pos = Message.Substring(0, 10).Trim();
                var mes = Message.Substring(22, Message.Length - 22).Trim();
                plcEventDTO.Message = string.Format("{0} {1}", pos, mes);

                bool hasCode = CodeNumbers.ContainsKey(plcEventDTO.CodeString);
                if (hasCode)
                    plcEventDTO.Code = CodeNumbers[plcEventDTO.CodeString];

                if (hasCode && eventCodeDict.ContainsKey(plcEventDTO.Code))
                {
                    var eventCode = eventCodeDict[plcEventDTO.Code];
                    plcEventDTO.CodeString = eventCode.Text;
                    plcEventDTO.SeverityNumber = eventCode.SeverityNumber;
                }
            }
            return plcEventDTO;
        }

        public static PlcEventDTO MapPlcEvent(PlcEvent plcEvent)
        {
            var plcEventDTO = new PlcEventDTO()
            {
                DateTime = plcEvent.DateTime.AddMilliseconds(plcEvent.Msec),
                Number = plcEvent.MessageNumber,
                Message = plcEvent.PlcMessage.Text,
                Code = plcEvent.CodeId,

                CodeString = plcEvent.PlcEventCode.Text,
                SeverityNumber = plcEvent.PlcEventCode.SeverityNumber,
                //Severity = plcEvent.PlcEventCode.Severity.Name,

                ShowValue = plcEvent.PlcEventCode.ShowValue == true,
                Value = plcEvent.Value,

                Group = plcEvent.PlcMessage.Group,
                //FactoryNumber = plcEvent.PLC.Factory.Number.Value,
                //PlcName = plcEvent.PLC.Description,
            };
            return plcEventDTO;
        }
    }
}