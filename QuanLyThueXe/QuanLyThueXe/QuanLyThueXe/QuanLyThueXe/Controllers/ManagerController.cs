using Microsoft.AspNetCore.Mvc;

namespace QuanLyThueXe.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
