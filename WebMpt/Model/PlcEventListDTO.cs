using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using System.Data.Entity;

using MPT.Model;
using MPT.PrimitiveType;

namespace WebMpt.Model
{
    public static partial class MPTEntitiesExt
    {
        public static IEnumerable<PlcEventDTO> GetPlcEventDTOList(this MPTEntities _db, PLC plc, 
            DateTime fromDateTime, DateTime toDateTime,
            IEnumerable<int> numbers = null, string message = null, 
            bool hideBreak = false,  bool sortOrderDesc = false)
        {
            if (plc == null)
                throw new ArgumentNullException("plc is null");

            if (plc.ProtocolType == 1)
            {
                var events = _db.PlcEvents.AsNoTracking()
                                .Where(x => x.PlcId == plc.Id)
                                //.Include(e => e.PLC)
                                //.Include(e => e.PLC.Factory)
                                .Include(e => e.PlcMessage)
                                .Include(e => e.PlcEventCode)
                                .Include(e => e.PlcEventCode.Severity);

                events = events.Where(x => x.DateTime >= fromDateTime && x.DateTime < toDateTime);

                if (numbers != null && numbers.Any())
                    events = events.Where(x => numbers.Contains(x.MessageNumber));

                if (!string.IsNullOrEmpty(message))
                    events = events.Where(e => e.PlcMessage.Text.ToUpper().Contains(message.ToUpper()));

                if (hideBreak == true)
                    events = events.Where(x => x.CodeId != 16 && x.CodeId != 48);

                events = sortOrderDesc == true ?
                    events.OrderByDescending(x => x.DateTime).ThenByDescending(x => x.Msec).ThenBy(x => x.Id) :
                    events.OrderBy(x => x.DateTime).ThenBy(x => x.Msec).ThenByDescending(x => x.Id);

                return events.ToList().Select(x => PlcEventDTO.MapPlcEvent(x));
            }


            if (plc.ProtocolType == 2)
            {
                var events = _db.PlcEventsOld.AsNoTracking()
                                .Where(x => x.PlcId == plc.Id)
                                //.Include(e => e.PLC)
                                //.Include(e => e.PLC.Factory)
                                ;

                events = events.Where(x => x.DateTime >= fromDateTime && x.DateTime < toDateTime);

                if (numbers != null && numbers.Any())
                    events = events.Where(x => numbers.Contains(x.n));

                events = sortOrderDesc == true ?
                    events.OrderByDescending(x => x.DateTime).ThenByDescending(x => x.Msec).ThenBy(x => x.Id) :
                    events.OrderBy(x => x.DateTime).ThenBy(x => x.Msec).ThenByDescending(x => x.Id);

                var codes = _db.PlcEventCodes.Include(x => x.Severity).ToDictionary(x => x.Id, y => y);

                var plcEventDTOList = events.ToList().Select(x => PlcEventDTO.MapPlcEventOld(x, codes));

                if (!string.IsNullOrEmpty(message))
                    plcEventDTOList = plcEventDTOList.Where(e => e.Message.IndexOf(message, StringComparison.InvariantCultureIgnoreCase) >= 0);
                if (hideBreak == true)
                    plcEventDTOList = plcEventDTOList.Where(x => x.Code != 16 && x.Code != 48);

                return plcEventDTOList;
            }
            return null;
        }
    }

    public class PlcEventListDTO
    {
        public PLC Plc { get; set; }
        public IEnumerable<PlcEventDTO> PlcEventDTOList { get; set; }

        [Display(Name = "Дата по")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [HiddenInput(DisplayValue = true)]
        public DateTime To { get; set; }

        [Display(Name = "Дата c")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [HiddenInput(DisplayValue = true)]
        public DateTime From { get; set; }

        private void MoveDate(int days = 1)
        {
            To = To.AddDays(days);
            From = From.AddDays(days);
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
        public bool Group { get; set; }

        [Display(Name = "Обратная хронология")]
        public bool Desc { get; set; }

        [Display(Name = "Найти номера")]
        public string Numbers { get; set; }
        public IEnumerable<int> NumberList { get { return StringToIntList.SplitToIntList(Numbers); } }

        [Display(Name = "Найти текст")]
        public string Message { get; set; }
    }
}