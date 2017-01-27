using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MPT.Model;
using MPT.Positions;

using WebMpt.Model;

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
        public ActionResult Events(object id,
            string from = null, string to = null,
            string numbers = null, string message = null,
            bool hideBreak = false, bool desc = false, bool groups = false)
        {
            PLC plc = null;
            try
            {
                int plcid = Convert.ToInt32(id);
                plc = _db.GetPLC(plcid);

            }
            catch(Exception)
            { }

            if (plc == null)
            {
                plc = _db.GetPLC(id.ToString());
            }

            if (plc == null)
                return RedirectToAction("Index");


            //ViewBag.PlcFullName = plc.FullName;

            var plcEventListDTO = new PlcEventListDTO
            {
                Plc = plc,
                Numbers = numbers,
                Message = message,
                From = DateTime.Now.Date,
                To = DateTime.Now.Date,
                HideBreak = hideBreak,
                Desc = desc,
                Group = groups,
            };

            if (!string.IsNullOrWhiteSpace(from))
            {
                try
                {
                    plcEventListDTO.From = DateTime.Parse(from);
                }
                catch { }
            }
            if (!string.IsNullOrWhiteSpace(to))
            {
                try
                {
                    plcEventListDTO.To = DateTime.Parse(to);
                }
                catch {}
            }

            plcEventListDTO.PlcEventDTOList  = _db.GetPlcEventDTOList(
                plcEventListDTO.Plc,
                plcEventListDTO.From,
                plcEventListDTO.To.AddDays(1),
                plcEventListDTO.NumberList,
                plcEventListDTO.Message,
                plcEventListDTO.HideBreak,
                plcEventListDTO.Desc
                );


            if (plcEventListDTO.PlcEventDTOList == null)
                return RedirectToAction("Index");

            return View(plcEventListDTO);
        }



        //[HttpPost]
        public JsonResult EventList(int id = 0, int jtStartIndex = 1, int jtPageSize = 100)
        {
            try
            {
                // var plcEventsViewPagedList = plcEvents.ToPagedList(jtStartIndex, jtPageSize);
                var plc = _db.GetPLC(id);
                var plcEventListDTO = new PlcEventListDTO()
                {
                    Plc = plc,
                };
                /*
                plcEventListDTO.PlcEventDTOList = _db.GetPlcEventDTOList(
                        plcEventListDTO.Plc,
                        plcEventListDTO.DateBegin,
                        plcEventListDTO.DateEnd,
                        plcEventListDTO.NumberList,
                        plcEventListDTO.Message,
                        plcEventListDTO.HideBreak,
                        plcEventListDTO.SortOrderDesc
                        );
                        */
                return Json(new { Result = "OK", Records = plcEventListDTO.PlcEventDTOList });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }



        public ActionResult Messages(int id)
        {
            var plc = _db.GetPLC(id);
            ViewBag.PlcId = plc.Id;
            ViewBag.PlcFullName = plc.FullName;

            return View(plc.Messages);
        }



        [HttpPost]
        public ActionResult RsViewTagsGenerate(HttpPostedFileBase excelFile, string nodeName = null)
        {
            return View();
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
                var plc = _db.GetPLC(plcId.Value);
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