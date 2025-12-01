using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models; // Model mới
using QuanLyThueXe.ViewModels; // CustomerContractVM
using System;
using System.Linq;

namespace QuanLyThueXe.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CarRentalDbContext _db;

        public CustomerController(CarRentalDbContext db)
        {
            _db = db;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("Role") ?? "";
            return role == "Admin" || role == "Manager";
        }

        // INDEX
        public IActionResult Index()
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var customers = _db.Customers.Include(c => c.Car).Include(c => c.Contracts).ToList();

            var model = customers.Select(c => new CustomerContractVM
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Phone = c.Phone,
                IdentityCard = c.IdentityCard,
                Gender = c.Gender,
                BirthDate = c.BirthDate.HasValue ? c.BirthDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                CarModel = c.Car != null ? $"{c.Car.Brand} - {c.Car.Model}" : "Chưa thuê",
                StartDate = null,
                EndDate = null,
                TotalPrice = null
            }).ToList();

            return View(model);
        }

        // DETAILS
        public IActionResult Details(int? id)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");
            if (id == null) return BadRequest();

            var customer = _db.Customers
                .Include(c => c.Car)
                .Include(c => c.Contracts)
                .FirstOrDefault(c => c.CustomerId == id);

            if (customer == null) return NotFound();

            // Lọc hợp đồng active
            var activeContract = customer.Contracts?
            .Where(ct => ct.EndDate is DateOnly endDate && endDate.ToDateTime(TimeOnly.MinValue) >= DateTime.Today)
            .OrderByDescending(ct => ct.ContractDate)
            .FirstOrDefault();


            ViewBag.ActiveContract = activeContract;
            return View(customer);
        }


        // CREATE
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CarId = new SelectList(_db.Cars.Where(c => c.Status == "Available"), "CarId", "Brand");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("FullName,Phone,IdentityCard,Gender,BirthDate,CarId")] Customer customer)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _db.Customers.Add(customer);

                if (customer.CarId.HasValue)
                {
                    var car = _db.Cars.Find(customer.CarId.Value);
                    if (car != null) car.Status = "Rented";
                }

                _db.SaveChanges();
                TempData["SuccessMessage"] = "Đã thêm khách hàng thành công.";
                return RedirectToAction("Index");
            }

            ViewBag.CarId = new SelectList(_db.Cars.Where(c => c.Status == "Available"), "CarId", "Brand", customer.CarId);
            return View(customer);
        }

        // EDIT
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");
            if (id == null) return BadRequest();

            var customer = _db.Customers.Find(id);
            if (customer == null) return NotFound();

            ViewBag.CarId = new SelectList(_db.Cars, "CarId", "Brand", customer.CarId);
            ViewBag.ActiveCar = _db.Cars.FirstOrDefault(c => c.CarId == customer.CarId);
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("CustomerId,FullName,Phone,IdentityCard,Gender,BirthDate,CarId")] Customer customer)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.CarId = new SelectList(_db.Cars, "CarId", "Brand", customer.CarId);
                return View(customer);
            }

            var existing = _db.Customers.Find(customer.CustomerId);
            if (existing == null) return NotFound();

            int? oldCarId = existing.CarId;
            int? newCarId = customer.CarId;

            existing.FullName = customer.FullName;
            existing.Phone = customer.Phone;
            existing.IdentityCard = customer.IdentityCard;
            existing.Gender = customer.Gender;
            existing.BirthDate = customer.BirthDate;
            existing.CarId = newCarId;

            // update car status
            if (oldCarId.HasValue && oldCarId != newCarId)
            {
                var oldCar = _db.Cars.Find(oldCarId.Value);
                if (oldCar != null) oldCar.Status = "Available";
            }

            if (newCarId.HasValue && newCarId != oldCarId)
            {
                var newCar = _db.Cars.Find(newCarId.Value);
                if (newCar != null) newCar.Status = "Rented";
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] = "Đã cập nhật khách hàng và xe thành công.";
            return RedirectToAction("Index");
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var customer = _db.Customers.Find(id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction("Index");
            }

            try
            {
                var contracts = _db.Contracts.Where(c => c.CustomerId == id).ToList();
                var reservations = _db.Reservations.Where(r => r.CustomerId == id).ToList();

                using var transaction = _db.Database.BeginTransaction();

                if (contracts.Any()) _db.Contracts.RemoveRange(contracts);
                if (reservations.Any()) _db.Reservations.RemoveRange(reservations);

                if (customer.CarId.HasValue)
                {
                    var car = _db.Cars.Find(customer.CarId.Value);
                    if (car != null) car.Status = "Available";
                }

                _db.Customers.Remove(customer);
                _db.SaveChanges();
                transaction.Commit();

                TempData["SuccessMessage"] = "Đã xóa khách hàng cùng hợp đồng và đặt xe liên quan.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa khách hàng: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction("Index");
        }
    }
}
