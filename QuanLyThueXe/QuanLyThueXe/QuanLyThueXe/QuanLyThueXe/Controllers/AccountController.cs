using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using QuanLyThueXe.Models;

namespace QuanLyThueXe.Controllers
{
    public class AccountController : Controller
    {
        private readonly CarRentalDbContext _db;

        public AccountController(CarRentalDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu!");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string fullname, string email)
        {
            if (_db.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại!");
                return View();
            }

            if (!email.EndsWith("@email.com"))
            {
                ModelState.AddModelError("", "Email phải có dạng ...@email.com!");
                return View();
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = password,
                FullName = fullname,
                Role = "User",
                Email = email
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Vui lòng nhập email.");
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại.");
                return View();
            }

            TempData["EmailToReset"] = email;
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["EmailToReset"] == null)
                return RedirectToAction("ForgotPassword");

            ViewBag.Email = TempData["EmailToReset"];
            TempData.Keep("EmailToReset");

            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            string email = TempData["EmailToReset"] as string;

            if (email == null)
                return RedirectToAction("ForgotPassword");

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu nhập lại không khớp.");
                TempData["EmailToReset"] = email;
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return RedirectToAction("ForgotPassword");

            user.PasswordHash = newPassword;
            _db.SaveChanges();

            TempData["Message"] = "Đặt lại mật khẩu thành công!";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
