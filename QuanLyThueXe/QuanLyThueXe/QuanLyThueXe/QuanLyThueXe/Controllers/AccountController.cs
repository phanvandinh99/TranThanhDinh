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
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin!");
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                bool isPasswordValid = false;

                // Kiểm tra xem password hash có phải là BCrypt hash không
                // BCrypt hash thường bắt đầu với $2a$, $2b$, hoặc $2y$
                bool isBcryptHash = user.PasswordHash != null && 
                                    (user.PasswordHash.StartsWith("$2a$") || 
                                     user.PasswordHash.StartsWith("$2b$") || 
                                     user.PasswordHash.StartsWith("$2y$"));

                if (isBcryptHash)
                {
                    // Password đã được hash bằng BCrypt
                    try
                    {
                        isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                    }
                    catch
                    {
                        // Nếu có lỗi khi verify (có thể hash không hợp lệ), thử so sánh plain text
                        isPasswordValid = user.PasswordHash == password;
                    }
                }
                else
                {
                    // Password cũ là plain text - so sánh trực tiếp
                    isPasswordValid = user.PasswordHash == password;
                }

                if (isPasswordValid)
                {
                    // Nếu password là plain text, tự động migrate sang BCrypt
                    if (!isBcryptHash)
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                        _db.SaveChanges();
                    }

                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role ?? "User");

                    return RedirectToAction("Index", "Home");
                }
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
            // Validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || 
                string.IsNullOrWhiteSpace(fullname) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin!");
                return View();
            }

            if (username.Length < 3)
            {
                ModelState.AddModelError("", "Tên đăng nhập phải có ít nhất 3 ký tự!");
                return View();
            }

            if (password.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự!");
                return View();
            }

            if (_db.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại!");
                return View();
            }

            if (_db.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email đã được sử dụng!");
                return View();
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                ModelState.AddModelError("", "Email không hợp lệ!");
                return View();
            }

            // Hash password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                Username = username,
                PasswordHash = hashedPassword,
                FullName = fullname,
                Role = "User",
                Email = email
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
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

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự.");
                TempData["EmailToReset"] = email;
                return View();
            }

            // Hash password mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
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
