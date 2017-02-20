using Microsoft.AspNetCore.Mvc;

namespace GetStartedDotnet.Controllers
{
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
    }
}
