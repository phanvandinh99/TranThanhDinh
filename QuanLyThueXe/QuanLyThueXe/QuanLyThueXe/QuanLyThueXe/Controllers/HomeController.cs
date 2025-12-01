using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThueXe.Models; // DbContext và entity
using System.Linq;

namespace QuanLyThueXe.Controllers
{
    public class HomeController : Controller
    {
        private readonly CarRentalDbContext _db;

        public HomeController(CarRentalDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(string vehicleType)
        {
            // L?y danh sách lo?i xe
            var types = new[] { "All", "Car", "Motorbike" };
            ViewBag.VehicleTypes = new SelectList(types, vehicleType ?? "All");

            // L?y danh sách xe
            var cars = _db.Cars.AsQueryable();

            if (!string.IsNullOrEmpty(vehicleType) && vehicleType != "All")
            {
                string vt = vehicleType.Trim().ToLower();
                cars = cars.Where(c => c.VehicleType != null &&
                                       c.VehicleType.Trim().ToLower() == vt);
            }

            return View(cars.ToList());
        }
    }
}
