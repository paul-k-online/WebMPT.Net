using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebMpt.Controllers
{
    public class RsViewController : Controller
    {
        //
        // GET: /RsView/
        public ActionResult Index()
        {
            return View();
        }

        
        [HttpPost]
        public ActionResult GenerateTags(HttpPostedFileBase excelFile, string nodeName)
        {


            return View();
        }
        
    }
}
