using Microsoft.AspNetCore.Mvc;

namespace QuanLyThueXe.Controllers
{
    public class AdminController : Controller
    {
        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            // Bạn có thể check role ở đây nếu muốn
            var role = HttpContext.Session.GetString("Role") ?? "";
            if (role != "Admin")
            {
                // Nếu không phải admin, redirect về home hoặc login
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
