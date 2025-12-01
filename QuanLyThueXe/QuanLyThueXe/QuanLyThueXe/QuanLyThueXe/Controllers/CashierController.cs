using Microsoft.AspNetCore.Mvc;

namespace QuanLyThueXe.Controllers
{
    public class CashierController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
