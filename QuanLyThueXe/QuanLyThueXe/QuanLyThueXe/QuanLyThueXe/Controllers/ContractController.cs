using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models;
using System;
using System.Linq;

namespace QuanLyThueXe.Controllers
{
    public class ContractController : Controller
    {
        private readonly CarRentalDbContext _db;

        public ContractController(CarRentalDbContext db)
        {
            _db = db;
        }

        private bool IsAuthorized()
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            return role == "Admin" || role == "Manager" || role == "Cashier";
        }

        // ===================================
        // INDEX
        // ===================================
        public IActionResult Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var contracts = _db.Contracts
                .Include(c => c.Car)
                .Include(c => c.Customer)
                .OrderByDescending(c => c.ContractDate)
                .ToList();

            return View(contracts);
        }

        // ===================================
        // CREATE
        // ===================================
        public IActionResult Create()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var model = new Contract
            {
                Quantity = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now)
            };

            LoadDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Contract contract)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            string vehicleTypeFromForm = Request.Form["VehicleType"];

            // ======= VALIDATION =======
            if (contract.CustomerId == 0)
                ModelState.AddModelError("CustomerId", "Vui lòng chọn khách hàng.");

            if (contract.CarId == 0)
                ModelState.AddModelError("CarId", "Vui lòng chọn xe.");

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            if (contract.StartDate < today)
                ModelState.AddModelError("StartDate", "Ngày thuê phải từ hôm nay trở đi.");

            if (contract.StartDate > contract.EndDate)
                ModelState.AddModelError("EndDate", "Ngày trả phải sau ngày thuê.");

            if (contract.Quantity <= 0)
                ModelState.AddModelError("Quantity", "Số lượng phải lớn hơn 0.");

            if (!ModelState.IsValid)
            {
                LoadDropdowns(contract, vehicleTypeFromForm);
                return View(contract);
            }

            // ======= Lấy thông tin xe =======
            var car = _db.Cars.Find(contract.CarId);
            if (car == null || car.Status != "Available")
            {
                ModelState.AddModelError("CarId", "Xe đã được thuê hoặc không tồn tại.");
                LoadDropdowns(contract, vehicleTypeFromForm);
                return View(contract);
            }

            // ======= Tính toán =======
            int rentalDays = (contract.EndDate.ToDateTime(TimeOnly.MinValue) - contract.StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
            if (rentalDays <= 0) rentalDays = 1;

            contract.TotalAmount = (car.PricePerDay ?? 0) * rentalDays * contract.Quantity;
            contract.ContractDate = DateTime.Now;
            contract.Status = "Active";
            contract.Deposit = contract.Deposit ?? 0;

            // ======= Lưu DB =======
            _db.Contracts.Add(contract);

            car.Status = "Rented";
            _db.Cars.Update(car);

            _db.SaveChanges();

            TempData["SuccessMessage"] = "Hợp đồng đã lập thành công!";
            return RedirectToAction("Index");
        }




        // ===================================
        // EDIT
        // ===================================
        public IActionResult Edit(int id)
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            if (!IsAuthorized() || role == "Cashier") return StatusCode(403);

            var contract = _db.Contracts.Find(id);
            if (contract == null) return NotFound();

            LoadDropdowns(contract);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Contract contract)
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            if (!IsAuthorized() || role == "Cashier") return StatusCode(403);

            // Validate
            if (contract.CustomerId == null || contract.CustomerId == 0)
                ModelState.AddModelError("CustomerId", "Vui lòng chọn khách hàng.");
            if (contract.CarId == null || contract.CarId == 0)
                ModelState.AddModelError("CarId", "Vui lòng chọn xe.");
            if (contract.StartDate < DateOnly.FromDateTime(DateTime.Now))
                ModelState.AddModelError("StartDate", "Ngày thuê phải từ hôm nay trở đi.");
            if (contract.StartDate > contract.EndDate)
                ModelState.AddModelError("EndDate", "Ngày trả phải sau ngày thuê.");

            if (!ModelState.IsValid)
            {
                LoadDropdowns(contract);
                return View(contract);
            }

            var existingContract = _db.Contracts.Find(contract.ContractId);
            if (existingContract == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hợp đồng.";
                return RedirectToAction("Index");
            }

            var car = _db.Cars.Find(contract.CarId);
            if (car == null)
            {
                ModelState.AddModelError("CarId", "Xe không tồn tại.");
                LoadDropdowns(contract);
                return View(contract);
            }

            // Nếu đổi xe, kiểm tra xe mới có sẵn không
            if (existingContract.CarId != contract.CarId && car.Status != "Available")
            {
                ModelState.AddModelError("CarId", "Xe mới không có sẵn.");
                LoadDropdowns(contract);
                return View(contract);
            }

            int rentalDays = (contract.EndDate.ToDateTime(TimeOnly.MinValue) - contract.StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
            if (rentalDays <= 0) rentalDays = 1;

            // Cập nhật thông tin hợp đồng
            existingContract.CustomerId = contract.CustomerId;
            existingContract.CarId = contract.CarId;
            existingContract.StartDate = contract.StartDate;
            existingContract.EndDate = contract.EndDate;
            existingContract.Quantity = contract.Quantity;
            existingContract.Deposit = contract.Deposit;
            existingContract.TotalAmount = (car.PricePerDay ?? 0) * rentalDays * contract.Quantity;
            existingContract.Status = contract.Status;

            // Nếu đổi xe, cập nhật trạng thái xe cũ và mới
            if (existingContract.CarId != contract.CarId)
            {
                var oldCar = _db.Cars.Find(existingContract.CarId);
                if (oldCar != null && !_db.Contracts.Any(c => c.CarId == oldCar.CarId && c.ContractId != contract.ContractId && c.Status == "Active"))
                {
                    oldCar.Status = "Available";
                }
                car.Status = "Rented";
            }

            _db.Contracts.Update(existingContract);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Hợp đồng đã được cập nhật!";
            return RedirectToAction("Index");
        }

        // ===================================
        // DELETE
        // ===================================
        public IActionResult Delete(int id)
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            if (!IsAuthorized() || role == "Cashier") return StatusCode(403);

            var contract = _db.Contracts
                .Include(c => c.Car)
                .Include(c => c.Customer)
                .FirstOrDefault(c => c.ContractId == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            if (!IsAuthorized() || role == "Cashier") return StatusCode(403);

            var contract = _db.Contracts.Find(id);
            if (contract == null) return NotFound();

            var car = _db.Cars.Find(contract.CarId);
            if (car != null)
            {
                car.Status = "Available";
                _db.Cars.Update(car);
            }

            _db.Contracts.Remove(contract);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Hợp đồng đã được xóa!";
            return RedirectToAction("Index");
        }

        // ===================================
        // DETAILS
        // ===================================
        public IActionResult Details(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var contract = _db.Contracts
                .Include(c => c.Car)
                .Include(c => c.Customer)
                .FirstOrDefault(c => c.ContractId == id);

            if (contract == null) return NotFound();

            contract.Car.ImageUrl = GetCarImagePath(contract.Car);
            return View(contract);
        }

        // ===================================
        // AJAX: GET Cars by VehicleType
        // ===================================
        [HttpGet]
        public IActionResult GetCarsByType(string vehicleType)
        {
            var cars = _db.Cars
                .Where(c => c.VehicleType == vehicleType && c.Status == "Available")
                .Select(c => new
                {
                    c.CarId,
                    DisplayText = $"{c.LicensePlate} ({c.Brand} {c.Model})"
                }).ToList();

            return Json(cars);
        }

        // ===================================
        // HELPER: Load dropdowns
        // ===================================
        // ===================================
        // HELPER: Load dropdowns (Đã thêm tham số tùy chọn)
        // ===================================
        private void LoadDropdowns(Contract contract, string selectedTypeFromForm = null)
        {
            // Lấy danh sách Khách hàng
            ViewBag.CustomerId = new SelectList(_db.Customers, "CustomerId", "FullName", contract?.CustomerId);

            // Xác định loại xe đã chọn
            string vehicleType = selectedTypeFromForm;

            if (string.IsNullOrEmpty(vehicleType) && contract?.CarId != null && contract.CarId != 0)
            {
                var selectedCar = _db.Cars.Find(contract.CarId);
                vehicleType = selectedCar?.VehicleType;
            }

            // Lấy danh sách Loại xe
            var vehicleTypes = _db.Cars
                                     .Select(c => c.VehicleType)
                                     .Distinct()
                                     .Where(v => v != null)
                                     .ToList();

            ViewBag.VehicleTypes = new SelectList(vehicleTypes, vehicleType);

            // Lấy danh sách Xe theo loại đã chọn
            var cars = _db.Cars
                              .Where(c => c.Status == "Available" &&
                                          (vehicleType == null || c.VehicleType == vehicleType))
                              .Select(c => new
                              {
                                  c.CarId,
                                  DisplayText = $"{c.LicensePlate} ({c.Brand} {c.Model})"
                              }).ToList();

            ViewBag.CarId = new SelectList(cars, "CarId", "DisplayText", contract?.CarId);
        }

        // ===================================
        // HELPER: Get car image path
        // ===================================
        private string GetCarImagePath(Car car)
        {
            if (car == null)
                return "/images/cars/car404.jpg";

            // Nếu có ImageUrl, sử dụng nó
            if (!string.IsNullOrEmpty(car.ImageUrl))
            {
                // Nếu ImageUrl đã là đường dẫn đầy đủ, dùng trực tiếp
                if (car.ImageUrl.StartsWith("/"))
                    return car.ImageUrl;
                
                // Nếu không, xây dựng đường dẫn dựa trên VehicleType
                string folder = car.VehicleType == "Car" ? "cars" :
                                car.VehicleType == "Motorbike" ? "motobikes" : "cars";
                return $"/images/{folder}/{car.ImageUrl}";
            }

            // Mặc định trả về car404.jpg
            return "/images/cars/car404.jpg";
        }

    }
}
