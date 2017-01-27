using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using MPT.Model;

namespace WebMpt.Model
{
    public static partial class MPTEntitiesExt
    {
        public static IEnumerable<PlcDTO> GetPlcDTOList(this MPTEntities db, DateTime? fromDT = null, DateTime? toDT = null)
        {
            if (fromDT == null)
                fromDT = DateTime.Now.Date;
            if (toDT == null)
                toDT = DateTime.Now.AddDays(1).Date;
            if (fromDT > toDT)
                fromDT = toDT;

            var plcEventsCountDict = db.GetPlcEventsCount(fromDT.Value, toDT.Value, severity: 2).ToDictionary(x => x.PlcId, y => y);
            return db.GetPLCs(protocolOnly:true).ToList().Select(x=>PlcDTO.MapPlc(x, plcEventsCountDict));
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
        [DataType(DataType.DateTime)]
        public DateTime? LastEventDateTime { get; set; }

        public int FactoryId { get; set; }
        public int? FactoryNumber { get; set; }
        public string FactoryName { get; set; }
        public string FactoryFullName { get; set; }

        public int? OrderIndex { get; set; }

        [Display(Name = "Сегодня")]
        public int? EventCount { get; set; }
        public int? AlarmCount { get; set; }
        public bool HasAlarm { get { return AlarmCount > 0; } }
        public int ProtocolType { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Id, Name);
        }

        public static PlcDTO MapPlc(PLC plc, int? totalCount = null, int? alarmCount = null)
        {
            if (plc == null)
                return null;

            var plcDTO = new PlcDTO();
            plcDTO.Id = plc.Id;
            plcDTO.Name = plc.Name;
            plcDTO.FullName = plc.FullName;
            plcDTO.Description = plc.Description;
            plcDTO.LastEventDateTime = plc.LastEventDateTime;

            plcDTO.FactoryId = plc.FactoryId;
            if (plc.Factory != null)
            {
                plcDTO.FactoryNumber = plc.Factory.Number;
                plcDTO.FactoryName = plc.Factory.Description;
                plcDTO.FactoryFullName = plc.Factory.FullName;
            }
            plcDTO.OrderIndex = plc.OrderIndex;
            plcDTO.ProtocolType = plc.ProtocolType != null ? plc.ProtocolType.Value : 0;

            plcDTO.EventCount = totalCount;
            plcDTO.AlarmCount = alarmCount;

            return plcDTO;
        }

        public static PlcDTO MapPlc(PLC plc, Dictionary<int, GetPlcEventsCount_Result> plcEventsCountDict)
        {
            if (plc == null)
                return null;

            int? totalCount = null;
            int? alarmCount = null;

            if (plcEventsCountDict != null && plcEventsCountDict.ContainsKey(plc.Id))
            {
                var count = plcEventsCountDict[plc.Id];
                totalCount = count.TotalCount;
                alarmCount = count.AlarmCount;
            }
            return MapPlc(plc, totalCount, alarmCount);
        }
    }
}