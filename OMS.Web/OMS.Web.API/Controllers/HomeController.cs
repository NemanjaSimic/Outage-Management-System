using OMS.Web.Common.Interfaces.Logger;
using System;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        public HomeController(ILogger logger)
        {
            _logger = logger;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            _logger.LogDebug("Accessing HomePage Controller");
            _logger.LogError(new Exception("Some testing text for a exception in HomePage Controller"), "Testing with custom message");

            return View();
        }
    }
}