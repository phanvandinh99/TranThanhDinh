using Microsoft.AspNetCore.Mvc;

namespace QuanLyThueXe.Controllers
{
    public class CarApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
