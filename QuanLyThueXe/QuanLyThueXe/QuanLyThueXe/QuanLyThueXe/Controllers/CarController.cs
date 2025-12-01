using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace QuanLyThueXe.Controllers
{
    public class CarController : Controller
    {
        private readonly CarRentalDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;

        // Constructor
        public CarController(CarRentalDbContext db, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }

        // =======================
        // Danh sách xe
        // =======================
        public IActionResult Index()
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role)) return RedirectToAction("Login", "Account");

            var cars = _db.Cars.ToList();
            return View(cars);
        }

        // =======================
        // Tạo xe mới (Create GET)
        // =======================
        public IActionResult Create()
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role") ?? "";
            if (role != "Admin" && role != "Manager")
                return RedirectToAction("Index", "Home");

            LoadImageList(null);
            return View();
        }

        // =======================
        // Tạo xe mới (Create POST)
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Car car)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role") ?? "";
            if (role != "Admin" && role != "Manager")
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                _db.Cars.Add(car);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            LoadImageList(car.ImageUrl);
            return View(car);
        }

        // =======================
        // Sửa xe (Edit GET)
        // =======================
        public IActionResult Edit(int id)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role") ?? "";
            if (role != "Admin" && role != "Manager")
                return RedirectToAction("Index", "Home");

            var car = _db.Cars.Find(id);
            if (car == null) return NotFound();

            ViewBag.StatusList = new SelectList(new[] { "Available", "Rented" }, car.Status);
            LoadImageList(car.ImageUrl);

            return View(car);
        }

        // =======================
        // Sửa xe (Edit POST)
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Car car)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role") ?? "";
            if (role != "Admin" && role != "Manager")
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                var originalCar = _db.Cars.Find(car.CarId);
                if (originalCar != null)
                {
                    originalCar.LicensePlate = car.LicensePlate;
                    originalCar.Brand = car.Brand;
                    originalCar.Model = car.Model;
                    originalCar.PricePerDay = car.PricePerDay;
                    originalCar.Status = car.Status;
                    originalCar.ImageUrl = car.ImageUrl;

                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return NotFound();
            }

            ViewBag.StatusList = new SelectList(new[] { "Available", "Rented" }, car.Status);
            LoadImageList(car.ImageUrl);
            return View(car);
        }

        // =======================
        // Xóa xe (Delete GET)
        // =======================
        public IActionResult Delete(int id)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role") ?? "";
            if (role != "Admin" && role != "Manager")
                return RedirectToAction("Index", "Home");

            var car = _db.Cars.Find(id);
            if (car == null) return NotFound();
            return View(car);
        }

        // =======================
        // Xóa xe (Delete POST)
        // =======================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role") ?? "";
            if (role != "Admin" && role != "Manager")
                return RedirectToAction("Index", "Home");

            var car = _db.Cars.Find(id);
            if (car == null) return RedirectToAction("Index");

            // Xóa liên kết với các reservation và contract
            var reservations = _db.Reservations.Where(r => r.CarId == id).ToList();
            _db.Reservations.RemoveRange(reservations);

            var contracts = _db.Contracts.Where(c => c.CarId == id).ToList();
            _db.Contracts.RemoveRange(contracts);

            _db.Cars.Remove(car);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Đã xóa xe {car.LicensePlate} thành công.";
            return RedirectToAction("Index");
        }

        // =======================
        // Load danh sách ảnh
        // =======================
        private void LoadImageList(string selectedImage)
        {
            var imageFolders = new[] { "cars", "motobikes" };
            var allImageFiles = new List<SelectListItem>();

            foreach (var folder in imageFolders)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "images", folder);
                if (Directory.Exists(folderPath))
                {
                    var images = Directory.GetFiles(folderPath)
                        .Select(Path.GetFileName)
                        .Select(file => new SelectListItem
                        {
                            Value = $"/images/{folder}/{file}",
                            Text = $"{folder.ToUpper()} - {file}",
                            Selected = $"/images/{folder}/{file}" == selectedImage
                        });
                    allImageFiles.AddRange(images);
                }
            }

            allImageFiles.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn ảnh --" });

            ViewBag.ImageList = new SelectList(allImageFiles, "Value", "Text", selectedImage);
        }
    }
}
