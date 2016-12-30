using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using MPT.Model;

namespace WebMpt.Model
{
    public static partial class MPTEntitiesExt
    {
        private static int? GetTotalCountByPlcId(IDictionary<int, GetPlcEventsCount_Result> dict, int plcId)
        {
            if (dict.ContainsKey(plcId))
                return dict[plcId].TotalCount;
            return null;
        }

        private static bool HasAlarmEventByPlcId(IDictionary<int, GetPlcEventsCount_Result> dict, int plcId)
        {
            if (dict.ContainsKey(plcId))
                return dict[plcId].AlarmCount != null && dict[plcId].AlarmCount > 0;
            return false;
        }

        public static IEnumerable<PlcDTO> GetPlcDTOList(this MPTEntities db)
        {
            var plcs = db.GetPLCs().ToList();
            
            var plcEventsCountList = db.GetPlcEventsCount(DateTime.Now.Date, DateTime.Now.AddDays(1).Date, severity: 2).ToList();
            
            var plcEventsCountDict = plcEventsCountList.ToDictionary(x => x.PlcId, y => y);
            
            Mapper.CreateMap<PLC, PlcDTO>()
                .ForMember(x => x.Id, opt => opt.MapFrom(y => y.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(y => y.Name))
                .ForMember(x => x.FullName, opt => opt.MapFrom(y => y.FullName))
                .ForMember(x => x.Description, opt => opt.MapFrom(y => y.Description))
                .ForMember(x => x.LastEventDateTime, opt => opt.MapFrom(y => y.LastEventDateTime))
                .ForMember(x => x.Factory, opt => opt.MapFrom(y => y.Factory))
                
                .ForMember(x => x.OrderIndex, opt => opt.MapFrom(y => y.OrderIndex))
                .ForMember(x => x.ProtocolType, opt => opt.MapFrom(y => y.ProtocolType ?? 0))
                
                .ForMember(x => x.TodayCount,      opt => opt.ResolveUsing(b => GetTotalCountByPlcId(plcEventsCountDict, b.Id)))
                .ForMember(x => x.HasWarningToday, opt => opt.ResolveUsing(b => HasAlarmEventByPlcId(plcEventsCountDict, b.Id)))
                ;
            //Mapper.AssertConfigurationIsValid();

            
            var plcViewList = Mapper.Map<IEnumerable<PLC>, IEnumerable<PlcDTO>>(plcs);
            return plcViewList;
        }
        
        public static IEnumerable<PlcEventDTO> MapPlcEventList(IEnumerable<PlcEvent> events)
        {
            Mapper.CreateMap<PlcEvent, PlcEventDTO>()
                .ForMember(name => name.Id, opt => opt.MapFrom(plcEvent => plcEvent.Id))
                .ForMember(name => name.LogPos, opt => opt.MapFrom(plcEvent => plcEvent.LogPos))
                
                .ForMember(name => name.DateTime, opt => opt.MapFrom(plcEvent => plcEvent.DateTime.AddMilliseconds(plcEvent.Msec)))
                .ForMember(name => name.Number, opt => opt.MapFrom(plcEvent => plcEvent.MessageNumber))
                
                .ForMember(name => name.Message, opt => opt.MapFrom(plcEvent => plcEvent.PlcMessage.Text))
                .ForMember(name => name.Code, opt => opt.MapFrom(plcEvent => plcEvent.CodeId))
                .ForMember(name => name.StringCode, opt => opt.MapFrom(plcEvent => plcEvent.PlcEventCode.Text))
                
                .ForMember(name => name.SeverityNumber, opt => opt.MapFrom(plcEvent => plcEvent.PlcEventCode.SeverityNumber))
                .ForMember(name => name.Severity, opt => opt.MapFrom(plcEvent => plcEvent.PlcEventCode.Severity.Name))

                .ForMember(name => name.Value, opt => opt.MapFrom(plcEvent => plcEvent.Value))
                .ForMember(name => name.StringValue, opt => opt.ResolveUsing(x => GetPlcEventValueString(x.Value, x.PlcEventCode.ShowValue)))

                .ForMember(name => name.Group, opt => opt.MapFrom(plcEvent => plcEvent.PlcMessage.Group))

                                .ForMember(name => name.FactoryNumber, opt => opt.MapFrom(plcEvent => plcEvent.PLC.Factory.Number))
                .ForMember(name => name.PlcName, opt => opt.MapFrom(plcEvent => plcEvent.PLC.Description))
                ;

            var eventsModel = Mapper.Map<IEnumerable<PlcEvent>, IEnumerable<PlcEventDTO>>(events);
            return eventsModel;
        }
        
        public static string GetPlcEventValueString(double? value, bool? showValue = true)
        {
            var show = showValue == null || showValue.Value;
            if (!show || value == null)
                return string.Empty;
            var v = value.Value;
            
            return SoftRound(v).ToString(CultureInfo.InvariantCulture);
        }

        public static double SoftRound(double v, int softDigits = 3)
        {
            try
            {
                var log10 = Math.Log10(Math.Abs(v));
                var fl = Math.Floor(log10);
                var exp = Convert.ToInt32(log10);
                //var val = plcEventDTO.Value.ToString(exp > 0 ? "F1" : "F3", CultureInfo.InvariantCulture);

                var digits = 0;

                if (exp <= -(softDigits))
                    digits = Math.Abs(exp) + 1;
                else if (exp < 0)
                    digits = Math.Abs(exp) + softDigits-1;
                else if (exp <= softDigits)
                    digits = softDigits - exp;
                else if (exp > softDigits)
                    digits = 0;

                return Math.Round(v, digits);
            }
            catch (Exception)
            {
                return v;
            }
        }

        private static string GetMessage(string message, PlcMessage plcMessage)
        {
            if (plcMessage != null && !string.IsNullOrWhiteSpace(plcMessage.Text))
                return plcMessage.Text;

            return string.Format("{0} {1}", 
                message.Substring(0, 10).Trim(),
                message.Substring(22, message.Length - 22).Trim());
        }

        private static string GetReason(string message)
        {
            if (message.Length < 22)
                return "";
            var s = message.Substring(10, 12).Trim();
            return s;
        }
        
        private static int GetCodeNumber(string codeText)
        {
            switch (codeText.Trim().ToUpper())
            {
                case "L":
                    return 1;
                case "LL":
                    return 2;
                case "H":
                    return 4;
                case "HH":
                    return 8;
                case "ОБРЫВ":
                    return 16;
                case "L OUT":
                    return 33;
                case "LL OUT":
                    return 33;
                case "H OUT":
                    return 33;
                case "HH OUT":
                    return 33;
            }
            return -1;
        }

        private static short GetSeverity(int code, IDictionary<int, PlcEventCode> severiryDict)
        {
            if (severiryDict.ContainsKey(code))
            {
                var r = severiryDict[code].SeverityNumber; 
                return r;
            }
            return 0;
        }

        private static string GetCodeStringNormalize(int code, IDictionary<int, PlcEventCode> eventCodeDict)
        {
            if (eventCodeDict.ContainsKey(code))
            {
                var s = eventCodeDict[code].Text; 
                return s;
            }
            return string.Empty;
        }

        private static string GetCodeStringNormalize(string codeString, IDictionary<int, PlcEventCode> eventCodeDict)
        {
            var codeNumber = GetCodeNumber(codeString);
            if (codeNumber == 0)
                return codeString.Trim();

            return GetCodeStringNormalize(codeNumber, eventCodeDict);
        }

        public static IEnumerable<PlcEventDTO> MapPlcOldEventList(IEnumerable<PlcOldEvent> events, IEnumerable<PlcEventCode> eventCodeList)
        {
            
            var eventCodeDict = eventCodeList.ToDictionary(x => x.Id, y => y);

            Mapper.CreateMap<PlcOldEvent, PlcEventDTO>()
                .ForMember(name => name.Id, opt => opt.MapFrom(plcEvent => plcEvent.Id))
                .ForMember(name => name.LogPos, opt => opt.MapFrom(plcEvent => plcEvent.LogPos))
                .ForMember(name => name.DateTime, opt => opt.MapFrom(plcEvent => plcEvent.DateTime.AddMilliseconds(plcEvent.Msec)))
                .ForMember(name => name.Number, opt => opt.MapFrom(plcEvent => plcEvent.MessageNumber))

                .ForMember(name => name.Message, opt => opt.ResolveUsing(x => GetMessage(x.Message, x.PlcMessage)))
                
                .ForMember(name => name.Code, opt => opt.ResolveUsing(x => GetCodeNumber(GetReason(x.Message))))
                
                //.ForMember(name => name.StringCode, opt => opt.ResolveUsing(x => GetReason(x.Message)))
                .ForMember(name => name.StringCode, opt => opt.ResolveUsing(x => GetCodeStringNormalize(GetReason(x.Message), eventCodeDict)))
                //.ForMember(name => name.StringCode, opt => opt.ResolveUsing(x => GetCodeStringNormalize(GetCodeNumber(GetReason(x.Message)), eventCodeDict)))

                .ForMember(name => name.SeverityNumber, opt => opt.ResolveUsing(x => GetSeverity(GetCodeNumber(GetReason(x.Message)), eventCodeDict)))
                
                //.ForMember(name => name.Severity, opt => opt.MapFrom(plcEvent => plcEvent.PlcEventCode.Severity.Name))
                //.ForMember(name => name.Value, opt => opt.MapFrom(plcEvent => plcEvent.Value ))
                
                .ForMember(name => name.Value, opt => opt.MapFrom(x => x.Value))
                .ForMember(name => name.StringValue, opt => opt.ResolveUsing(x => GetPlcEventValueString(x.Value)))

                //.ForMember(name => name.Group, opt => opt.MapFrom(x => null))
                .ForMember(name => name.FactoryNumber, opt => opt.MapFrom(plcEvent => plcEvent.PLC.Factory.Number))
                .ForMember(name => name.PlcName, opt => opt.MapFrom(plcEvent => plcEvent.PLC.Description))
                ;
                
            var eventsModel = Mapper.Map<IEnumerable<PlcOldEvent>, IEnumerable<PlcEventDTO>>(events);
            return eventsModel;
        }
    }
}