using Microsoft.AspNetCore.Mvc;
using QuanLyThueXe.Helpers;

namespace QuanLyThueXe.ViewComponents
{
    public class UserMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var session = HttpContext.Session;
            ViewBag.IsLoggedIn = AuthHelper.IsLoggedIn(session);
            ViewBag.Username = AuthHelper.GetUsername(session);
            ViewBag.Role = AuthHelper.GetRole(session);
            ViewBag.IsAdmin = AuthHelper.IsAdmin(session);
            ViewBag.IsAuthorized = AuthHelper.IsAuthorized(session);
            ViewBag.IsCustomer = AuthHelper.IsCustomer(session);
            
            return View();
        }
    }
}

