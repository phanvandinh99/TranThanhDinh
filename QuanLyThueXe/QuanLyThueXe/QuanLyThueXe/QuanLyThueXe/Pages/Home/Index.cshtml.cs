using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models;

namespace QuanLyThueXe.Pages.Home
{
    /// <summary>
    /// PageModel cho trang chủ - Hiển thị danh sách xe cho thuê
    /// Đây là ví dụ về Razor Pages với file .cshtml.cs (code-behind)
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly CarRentalDbContext _db;

        public IndexModel(CarRentalDbContext db)
        {
            _db = db;
        }

        // Properties để bind với View
        public List<Car> Cars { get; set; } = new();
        public SelectList VehicleTypes { get; set; } = null!;

        // Method xử lý GET request
        public void OnGet(string vehicleType)
        {
            // Lấy danh sách loại xe
            var types = new[] { "All", "Car", "Motorbike" };
            VehicleTypes = new SelectList(types, vehicleType ?? "All");

            // Lấy danh sách xe
            var cars = _db.Cars.AsQueryable();

            // Lọc theo loại xe nếu có
            if (!string.IsNullOrEmpty(vehicleType) && vehicleType != "All")
            {
                string vt = vehicleType.Trim().ToLower();
                cars = cars.Where(c => c.VehicleType != null &&
                                       c.VehicleType.Trim().ToLower() == vt);
            }

            Cars = cars.ToList();
        }
    }
}

