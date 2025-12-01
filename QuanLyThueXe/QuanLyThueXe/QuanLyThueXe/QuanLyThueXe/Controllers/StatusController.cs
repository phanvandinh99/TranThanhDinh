using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models;
using QuanLyThueXe.Helpers;

namespace QuanLyThueXe.Controllers
{
    public class StatusController : Controller
    {
        private readonly CarRentalDbContext _db;

        public StatusController(CarRentalDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Tra cứu tình trạng xe - Chỉ dành cho Admin/Manager
        /// </summary>
        public async Task<IActionResult> Index(string searchTerm = "", string statusFilter = "All", string vehicleTypeFilter = "All")
        {
            // Kiểm tra quyền truy cập
            if (!AuthHelper.IsAuthorized(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            // Thống kê tổng quan
            var totalCars = await _db.Cars.CountAsync();
            var availableCars = await _db.Cars.CountAsync(c => c.Status == "Available");
            var rentedCars = await _db.Cars.CountAsync(c => c.Status == "Rented");
            var pendingReservations = await _db.Reservations.CountAsync(r => r.Status == "Pending");
            var activeContracts = await _db.Contracts.CountAsync(c => c.Status == "Active");

            ViewBag.TotalCars = totalCars;
            ViewBag.AvailableCars = availableCars;
            ViewBag.RentedCars = rentedCars;
            ViewBag.PendingReservations = pendingReservations;
            ViewBag.ActiveContracts = activeContracts;

            // Lấy danh sách xe với thông tin chi tiết
            var carsQuery = _db.Cars
                .Include(c => c.Contracts)
                .ThenInclude(ct => ct.Customer)
                .Include(c => c.Reservations)
                .AsQueryable();

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                carsQuery = carsQuery.Where(c =>
                    c.LicensePlate.Contains(searchTerm) ||
                    (c.Brand != null && c.Brand.Contains(searchTerm)) ||
                    (c.Model != null && c.Model.Contains(searchTerm)));
            }

            // Lọc theo trạng thái
            if (statusFilter != "All")
            {
                carsQuery = carsQuery.Where(c => c.Status == statusFilter);
            }

            // Lọc theo loại xe
            if (vehicleTypeFilter != "All")
            {
                carsQuery = carsQuery.Where(c => c.VehicleType == vehicleTypeFilter);
            }

            var cars = await carsQuery
                .OrderBy(c => c.Status)
                .ThenBy(c => c.LicensePlate)
                .ToListAsync();

            // Tạo ViewBag cho dropdowns
            ViewBag.StatusFilter = statusFilter;
            ViewBag.VehicleTypeFilter = vehicleTypeFilter;
            ViewBag.SearchTerm = searchTerm;

            // Lấy danh sách hợp đồng đang hoạt động
            var activeContractsList = await _db.Contracts
                .Include(c => c.Car)
                .Include(c => c.Customer)
                .Where(c => c.Status == "Active")
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            ViewBag.ActiveContractsList = activeContractsList;

            // Lấy danh sách đơn đặt chờ duyệt
            var pendingReservationsList = await _db.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            ViewBag.PendingReservationsList = pendingReservationsList;

            return View(cars);
        }
    }
}
