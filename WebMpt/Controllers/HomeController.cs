using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebMpt.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "";
            //return View();
            return RedirectToAction("Index", "Plc");
        }

        public ActionResult About()
        {
            ViewBag.Message = "";
            return View();
        }
    }
}
