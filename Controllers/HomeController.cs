using ContactBookPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace ContactBookPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //if it matches the sting, it will run this action
        [Route("/Home/HandleError/{code:int}")]
        public IActionResult HandleError(int code)
        {
            var customError = new CustomError();

            customError.code = code;

            if (code == 400)
            {
                customError.message = "The page you are looking for might have been removed or temporarily unavailable";
            }
            else
            {
                customError.message = "Sorry, something went wrong";

            }

            return View("~/Views/Shared/CustomError.cshtml", customError);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}