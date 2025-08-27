using Microsoft.AspNetCore.Mvc;
using ServerSide.Infrastructure.ParcelService;
using ServerSide.Models;
using System.Diagnostics;

namespace ServerSide.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IParcelRestClient _parcelRestClient;
                
        public HomeController(ILogger<HomeController> logger, IParcelRestClient parcelRestClient)
        {
            _logger = logger;
            _parcelRestClient = parcelRestClient;
        }

        public IActionResult Index()
        {           
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
