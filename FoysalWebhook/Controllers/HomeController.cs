using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FoysalWebhook.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [HttpPost]
        public async Task<ActionResult> Submit()
        {
            // Create an event with action 'event1' and additional data
            await this.NotifyAsync("event1", new { P1 = "p1" });

            return new EmptyResult();
        }


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}