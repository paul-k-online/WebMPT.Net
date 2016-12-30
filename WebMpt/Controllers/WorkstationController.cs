using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;


using PagedList;
using MPT.Model;


namespace WebMpt.Controllers
{
    public class WorkstationController : Controller
    {
        private readonly MPTEntities db = new MPTEntities();

        //
        // GET: /Workstation/
        public ActionResult Index()
        {
            var workstations = db.GetWorkstations();
            return View(workstations.ToList());
        }

        public ActionResult Events(int? id = null, int page = 1, int pageSize = 250, bool hideIgnored = false)
        {
            if (id == null)
            {
                var pcEventList = db.GetPcEvents( hideIgnored: hideIgnored).Include(x=>x.Workstation);
                return View("EventsAll", pcEventList.ToPagedList(page, pageSize));
            }

            
            var workstationId = id;
            var ws = db.GetWorkstation(id.Value);

            ViewBag.wsName = ws.NetworkName.ToUpper();
            ViewBag.wsIP = ws.IP;

            var pcEvents = db.GetPcEvents(wsID: id, hideIgnored: hideIgnored)
                .OrderByDescending(e => e.DateTime)
                .Where(w => w.WorkstationId == workstationId)
                .AsNoTracking()
                .AsQueryable()
                ;

            var ignoreWords = db.PcEventIgnoreWords
                                    .Where(w => w.Enable==1)
                                    .Select(x => x.Word/*.ToUpper()*/)
                                    .AsNoTracking()
                                    .ToList()
                                    ;

// ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var ignoreWord in ignoreWords)
            {
                var word = ignoreWord;
                pcEvents = pcEvents
                            .Where(evnt => !evnt.Message
                                        //.ToUpper()
                                        //.Contains(word)
                                        .StartsWith(word)
                                  );
            }


            ViewBag.sql = pcEvents.ToString();
            
            var pcEventsPagedList = pcEvents.ToPagedList(page, pageSize);
            return View(pcEventsPagedList);
        }

        //
        // GET: /Workstation/Details/5
        public ActionResult Details(long id = 0)
        {
            var workstation = db.Workstations.Single(w => w.Id == id);
            if (workstation == null)
            {
                return HttpNotFound();
            }
            return View(workstation);
        }


        //
        // GET: /Workstation/Create
        public ActionResult Create()
        {
            ViewBag.ProjectId = new SelectList(db.ProjectHMIs, "Id", "Name");

            return View();
        }


        //
        // POST: /Workstation/Create
        [HttpPost]
        public ActionResult Create(Workstation workstation)
        {
            if (ModelState.IsValid)
            {
                db.Workstations.Add(workstation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProjectId = new SelectList(db.ProjectHMIs.OrderBy(x=>x.Factory.Number).ThenBy(x=>x.Name), "Id", "Name", workstation.ProjectId);
            return View(workstation);
        }


        //
        // GET: /Workstation/Edit/5
        public ActionResult Edit(long id = 0)
        {
            var workstation = db.Workstations.Single(w => w.Id == id);
            if (workstation == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProjectId = new SelectList(db.ProjectHMIs, "ID", "Name", workstation.ProjectId);
            return View(workstation);
        }


        //
        // POST: /Workstation/Edit/5
        [HttpPost]
        public ActionResult Edit(Workstation workstation)
        {
            if (ModelState.IsValid)
            {
                db.Workstations.Attach(workstation);
                //db.ObjectStateManager.ChangeObjectState(workstation, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProjectId = new SelectList(db.ProjectHMIs, "Id", "Name", workstation.ProjectId);
            return View(workstation);
        }


        //
        // GET: /Workstation/Delete/5
        public ActionResult Delete(long id = 0)
        {
            var workstation = db.Workstations.Single(w => w.Id == id);
            if (workstation == null)
            {
                return HttpNotFound();
            }
            return View(workstation);
        }


        //
        // POST: /Workstation/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            var workstation = db.Workstations.Single(w => w.Id == id);
            db.Workstations.Remove(workstation);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}