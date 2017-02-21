using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using MPT.Positions;
using MPT.RSView.ImportExport;
using MPT.RSView.ImportExport.Csv;
using System.Xml.Linq;
using System.Net.Mime;

namespace WebMpt.Controllers
{
    public class RSViewTagGeneratorController : Controller
    {
        //
        // GET: /RSViewTagGenerator/

        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Generate(HttpPostedFileBase excelFile = null, string nodeName = null)
        {
            if (excelFile == null || excelFile.ContentLength <= 0 || string.IsNullOrWhiteSpace(nodeName))
                return RedirectToAction("Index");

            var path = Path.GetTempFileName();
            ViewBag.ErrorMessage = path + Environment.NewLine;
            try
            {
                
                excelFile.SaveAs(path);
                var posList = new ExcelPositionList(path);
                if (!posList.LoadAiSheet())
                    ViewBag.ErrorMessage += "Ошибка загрузки страницы AI из Excel\n" + Environment.NewLine;

                if (!posList.LoadAoSheet())
                    ViewBag.ErrorMessage += "Ошибка загрузки страницы AO из Excel\n" + Environment.NewLine;

                if (!posList.LoadDioSheet())
                    ViewBag.ErrorMessage += "Ошибка загрузки страницы DIO из Excel\n" + Environment.NewLine;

                var binDirectoryPath = Server.MapPath("~/bin");
                var p = AppDomain.CurrentDomain.BaseDirectory;
                var shemaFilePath = Path.Combine(binDirectoryPath, @"RSView\ImportExport\POSITIONLIST.xml");
                var shema = XElement.Load(shemaFilePath);
                var converter = new RSViewPositionListConverter(posList, shema, nodeName);
                var csvGenerator = new CsvGenerator(converter.ConvertAllPositionsToRsViewTags(), nodeName);

                var fileData = csvGenerator.GetZipStream().ToArray();
                var fileName = csvGenerator.ZipFileName;

                //Response.AppendHeader("Content-Disposition", );
                return File(fileData, MediaTypeNames.Application.Zip, fileName);
            }
            catch (Exception ex)
            {
                System.IO.File.Delete(path);
                ViewBag.ErrorMessage += ex.ToString();
                return View();
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}