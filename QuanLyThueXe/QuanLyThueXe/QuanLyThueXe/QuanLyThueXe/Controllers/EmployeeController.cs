using Microsoft.AspNetCore.Mvc;

namespace QuanLyThueXe.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
