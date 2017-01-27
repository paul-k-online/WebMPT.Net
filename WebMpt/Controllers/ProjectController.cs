using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MPT.Model;

namespace WebMpt.Controllers
{
    public class ProjectController : Controller
    {
        private MPTEntities db = new MPTEntities();



        //
        // GET: /Project/
        public ActionResult Index()
        {
            return View(db.ProjectHMIs.Include(x=>x.Factory).OrderBy(x=>x.FactoryId).ThenBy(x=>x.OrderIndex) .ToList());
        }



        //
        // GET: /Project/Details/5
        public ActionResult Details(long id = 0)
        {
            var project = db.ProjectHMIs.Single(p => p.Id == id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }



        //
        // GET: /Project/Create
        public ActionResult Create()
        {
            return View();
        }



        //
        // POST: /Project/Create
        [HttpPost]
        public ActionResult Create(ProjectHMI project)
        {
            if (!ModelState.IsValid) 
                return View(project);
            
            db.ProjectHMIs.Add(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        //
        // GET: /Project/Edit/5
        public ActionResult Edit(long id = 0)
        {
            var project = db.ProjectHMIs.Single(p => p.Id == id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }



        //
        // POST: /Project/Edit/5
        [HttpPost]
        public ActionResult Edit(ProjectHMI project)
        {
            if (!ModelState.IsValid) 
                return View(project);
            
            db.ProjectHMIs.Attach(project);
            //db.ObjectStateManager.ChangeObjectState(project, EntityState.Modified);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        //
        // GET: /Project/Delete/5
        public ActionResult Delete(long id = 0)
        {
            var project = db.ProjectHMIs.Single(p => p.Id == id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }



        //
        // POST: /Project/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            var project = db.ProjectHMIs.Single(p => p.Id == id);
            db.ProjectHMIs.Remove(project);
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