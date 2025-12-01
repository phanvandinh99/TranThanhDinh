using Microsoft.AspNetCore.Mvc;

namespace QuanLyThueXe.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
