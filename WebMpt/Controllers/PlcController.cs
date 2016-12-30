using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMpt.Model;
using MPT.Model;
using MPT.Positions;

using MPTEntitiesExt = WebMpt.Model.MPTEntitiesExt;

namespace WebMpt.Controllers
{
    public class PlcController : Controller
    {
        private readonly MPTEntities _db = new MPTEntities();
        

        /// <summary>
        /// GET: /Plc/
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var plcDtoList = _db.GetPlcDTOList();
            return View(plcDtoList);
        }

        //
        // GET: /Plc/Events/5
        public ActionResult Events(int id,
            string dateEnd = null, string dateBegin = null,
            string numbers = null, string message = null,
            bool hideBreak = false, bool sortOrderDesc = false, bool showGroup = false)
        {
            
            var plc = _db.GetPlc(id);
            ViewBag.PlcFullName = plc.FullName;

            var plcEventListDTO = new PlcEventListDTO
            {
                Plc = plc,
                Numbers = numbers, 
                Message = message,
                DateEnd = DateTime.Now.Date,
                DateBegin = DateTime.Now.Date,
                HideBreak = hideBreak, 
                SortOrderDesc = sortOrderDesc,
                ShowGroup = showGroup,
            };


            if (!string.IsNullOrWhiteSpace(dateEnd))
            {
                try
                {
                    plcEventListDTO.DateEnd = DateTime.Parse(dateEnd).Date;
                }
                catch { }
            }

            if (!string.IsNullOrWhiteSpace(dateBegin))
            {
                try
                {
                    plcEventListDTO.DateBegin = DateTime.Parse(dateBegin).Date;
                }
                catch{}
            }
            
            if (plcEventListDTO.DateBegin > plcEventListDTO.DateEnd)
            {
                plcEventListDTO.DateBegin = plcEventListDTO.DateEnd;
            }


            var filterDateFrom = plcEventListDTO.DateBegin.Date;
            var filterDateTo = plcEventListDTO.DateEnd.AddDays(1).Date;
            
            
            if (plc.ProtocolType == 1)
            {
                var events = _db.GetEventsByPlc(plc);
                events = events.Where(x => x.DateTime >= filterDateFrom  &&  x.DateTime < filterDateTo);

                if (!string.IsNullOrEmpty(plcEventListDTO.Numbers))
                {
                    //events = events.Where(e => e.MessageNumber == plcEventListDTO.Numbers);

                    if (plcEventListDTO.NumberList != null && plcEventListDTO.NumberList.Any())
                        events = events.Where(x => plcEventListDTO.NumberList.Contains(x.MessageNumber));
                }


                if (!string.IsNullOrEmpty(plcEventListDTO.Message))
                    events = events.Where(e => e.PlcMessage.Text.ToUpper().Contains(plcEventListDTO.Message.ToUpper()));
                
                if (plcEventListDTO.HideBreak)
                    events = events.Where(x => x.CodeId != 16 && x.CodeId != 48);

                events = plcEventListDTO.SortOrderDesc ? 
                    events.OrderByDescending(x => x.DateTime).ThenByDescending(x=>x.Msec).ThenBy( x => x.Id) : 
                    events.OrderBy(x => x.DateTime).ThenBy(x => x.Msec).ThenByDescending(x => x.Id);

                plcEventListDTO.EventList = MPTEntitiesExt.MapPlcEventList(events);
                return View(plcEventListDTO);
            }

            
            if (plc.ProtocolType == 2)
            {
                var events = _db.GetEventsOldByPlc(plc);
                events = events.Where(x => x.DateTime >= filterDateFrom && x.DateTime < filterDateTo);

                if (plcEventListDTO.Numbers != null)
                {
                    if (plcEventListDTO.NumberList != null && plcEventListDTO.NumberList.Any())
                    {
                        events = events.Where(x => plcEventListDTO.NumberList.Contains(x.MessageNumber));
                    }
                }

                if (plcEventListDTO.SortOrderDesc)
                    events = events.OrderByDescending(x => x.DateTime).ThenByDescending(x => x.Id);
                else
                    events = events.OrderBy(x => x.DateTime).ThenBy(x => x.Id);

                var codes = _db.PlcEventCodes.Include(x => x.Severity).ToList();
                plcEventListDTO.EventList = MPTEntitiesExt.MapPlcOldEventList(events, codes);

                if (!string.IsNullOrEmpty(plcEventListDTO.Message))
                    plcEventListDTO.EventList = plcEventListDTO.EventList.Where(e => e.Message.ToUpper().Contains(plcEventListDTO.Message.ToUpper()));

                if (plcEventListDTO.HideBreak)
                    plcEventListDTO.EventList = plcEventListDTO.EventList.Where(x => x.Code != 16 && x.Code != 48);

                return View(plcEventListDTO);
            }

            return RedirectToAction("Index");
        }


        public ActionResult Messages(int id)
        {
            var plc = _db.GetPlc(id);
            ViewBag.PlcId = plc.Id;
            ViewBag.PlcFullName = plc.FullName;

            return View(plc.Messages);
        }

        [HttpPost]
        public ActionResult MessagesUpdate(HttpPostedFileBase excelFile = null, int? plcId = null)
        {
            if (excelFile == null || excelFile.ContentLength <= 0 || plcId == null) 
                return RedirectToAction("Index");

            var path = Path.GetTempFileName();
            try
            {
                excelFile.SaveAs(path);
                var plc = _db.GetPlc(plcId.Value);
                var excelPositionList = new ExcelPositionList(path, plc.Id);
                if (!excelPositionList.LoadMessagesSheet())
                {
                    ViewBag.ErrorMessage = "Ошибка загрузки из Excel";
                    ViewBag.PlcFullName = plc.FullName;
                    ViewBag.PlcId = plc.Id;
                    return View("Messages", plc.Messages);
                }

                var dbMessages = plc.Messages.ToList();//.ToDictionary(x=>x.Number,y=>y);
                var excelMessages = excelPositionList.PlcMessages.Values;
                
                var merge = new PlcMessagesMerge(dbMessages, excelMessages, plc.Id);

                Session["PlcMessagesMerge"] = merge;

                ViewBag.PlcId = plc.Id;
                ViewBag.PlcFullName = plc.FullName;

                var diff = merge.Diff;
                return View(diff);
            }
            catch (Exception)
            {
                
                throw;
            }
            finally
            {
                System.IO.File.Delete(path);    
            }
        }


        [HttpPost]
        public ActionResult MessagesUpdateApply()
        {
            var messagesMerge = Session["PlcMessagesMerge"] as PlcMessagesMerge;
            
            if (messagesMerge == null /* || messagesMerge.PlcId == null */)
            {
                return RedirectToAction("Index");
            }

            messagesMerge.SaveToDb(_db);
            
            return RedirectToAction("Messages", new { id = messagesMerge.PlcId });
        }

        /*
        [HttpPost]
        public JsonResult EventList(int plcId = 0, int jtStartIndex = 1, int jtPageSize = 100 )
        {
            try
            {
                var plc = _db.GetPlc(plcId);
                var plcEvents = _db.GetEventsByPlc(plc);


                
                var plcEventsViewPagedList = plcEvents.ToPagedList(jtStartIndex, jtPageSize);
                return Json(new { Result = "OK", Records = plcEventsViewPagedList });
            }
            catch (Exception ex)
            {
                return Json(new {Result = "ERROR", Message = ex.Message});
            }
        }
        */

        //
        // GET: /Plc/Details/5
        public ActionResult Details(int id = 0)
        {
            var plc = _db.PLCs.Single(p => p.Id == id);
            if (plc == null)
            {
                return HttpNotFound();
            }
            return View(plc);
        }


        public ActionResult Create()
        {
            ViewBag.FactoryId = new SelectList(_db.Factories.Where(f => f.Enable == 1), "Id", "Description");
            return View();
        }


        [HttpPost]
        public ActionResult Create(PLC plc)
        {
            if (ModelState.IsValid)
            {
                _db.PLCs.Add(plc);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FactoryId = new SelectList(_db.Factories.Where(f=>f.Enable==1), "Id", "Description", plc.FactoryId);
            //ViewBag.PlcTypeId = new SelectList(db.PlcTypes, "ID", "Name", plc.PlcTypeId);
            return View(plc);
        }


        //
        // GET: /Plc/Edit/5
        public ActionResult Edit(int id = 0)
        {
            var plc = _db.PLCs.Single(p => p.Id == id);
            if (plc == null)
            {
                return HttpNotFound();
            }
            ViewBag.FactoryId = new SelectList(_db.Factories, "ID", "Text", plc.FactoryId);
            // ViewBag.PlcTypeId = new SelectList(db.PlcTypes, "ID", "Name", plc.PlcTypeId);
            return View(plc);
        }


        //
        // POST: /Plc/Edit/5
        [HttpPost]
        public ActionResult Edit(PLC plc)
        {
            if (ModelState.IsValid)
            {
                _db.PLCs.Attach(plc);
                //db.ObjectStateManager.ChangeObjectState(plc, EntityState.Modified);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FactoryId = new SelectList(_db.Factories, "ID", "Text", plc.FactoryId);
            //ViewBag.PlcTypeId = new SelectList(_db.plctypes, "ID", "Name", plc.PlcTypeId);
            return View(plc);
        }


        //
        // GET: /Plc/Delete/5
        public ActionResult Delete(int id = 0)
        {
            var plc = _db.PLCs.Single(p => p.Id == id);
            if (plc == null)
            {
                return HttpNotFound();
            }
            return View(plc);
        }

        //
        // POST: /Plc/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var plc = _db.PLCs.Single(p => p.Id == id);
            _db.PLCs.Remove(plc);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}