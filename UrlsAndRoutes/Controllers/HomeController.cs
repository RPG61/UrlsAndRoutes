using System.Web.Mvc;

namespace UrlsAndRoutes.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            ViewBag.Controller = "Home";
            ViewBag.Action = "Index";

            return View("ActionName");
        }
    }
}