using MediatR;
using OMS.Web.Services.Commands;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            _mediator.Send(new TurnOffSwitchCommand(11111));
            _mediator.Send(new TurnOnSwitchCommand(22222));

            return View();
        }
    }
}