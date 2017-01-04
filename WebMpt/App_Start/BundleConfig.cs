using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Optimization;

namespace WebMpt
{
    internal class AsIsBundleOrderer : IBundleOrderer
    {
        public virtual IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }
    internal static class BundleExtensions
    {
        public static Bundle ForceOrdered(this Bundle sb)
        {
            sb.Orderer = new AsIsBundleOrderer();
            return sb;
        }
    }

    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/Scripts/jquery-{version}.js")
                );
            
            bundles.Add(new ScriptBundle("~/bundles/jqueryval")
                .Include("~/Scripts/jquery.unobtrusive*")
                .Include("~/Scripts/jquery.validate*")
                );

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr")
                .Include("~/Scripts/modernizr-{version}.js")
                );


            bundles.Add(new ScriptBundle("~/bundles/bootstrap")
                .Include("~/Scripts/bootstrap.*")
                .ForceOrdered());

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-datetimepicker")
                .Include("~/Scripts/moment-with-locales.*")
                .Include("~/Scripts/bootstrap-datetimepicker.*")
                .Include("~/Scripts/bootstrap-datepicker.*")
                .ForceOrdered());
            
            
            bundles.Add(new StyleBundle("~/Content/css")
                .Include("~/Content/bootstrap.*")
                .Include("~/Content/site.css")
                );


            bundles.Add(new StyleBundle("~/Content/bootstrap-datetimepicker")
                .Include("~/Content/bootstrap-datetimepicker.*")
                .Include("~/Content/bootstrap-datepicker.*")
                );

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
        }
    }
}