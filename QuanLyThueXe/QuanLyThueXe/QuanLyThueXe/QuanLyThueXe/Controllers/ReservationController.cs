using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models;
using System;
using System.Linq;

namespace QuanLyThueXe.Controllers
{
    public class ReservationController : Controller
    {
        private readonly CarRentalDbContext _db;

        public ReservationController(CarRentalDbContext db)
        {
            _db = db;
        }

        private bool IsAuthorized()
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            return role == "Admin" || role == "Manager" || role == "Cashier";
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        // ===================================
        // INDEX
        // ===================================
        public IActionResult Index(string searchTerm = "", string statusFilter = "All")
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var reservationsQuery = _db.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .AsQueryable();

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                reservationsQuery = reservationsQuery.Where(r =>
                    (r.Car != null && (r.Car.LicensePlate.Contains(searchTerm) || 
                                      (r.Car.Brand != null && r.Car.Brand.Contains(searchTerm)) ||
                                      (r.Car.Model != null && r.Car.Model.Contains(searchTerm)))) ||
                    (r.Customer != null && (r.Customer.FullName != null && r.Customer.FullName.Contains(searchTerm) ||
                                           (r.Customer.Phone != null && r.Customer.Phone.Contains(searchTerm)) ||
                                           (r.Customer.Email != null && r.Customer.Email.Contains(searchTerm)))));
            }

            // Lọc theo trạng thái
            if (statusFilter != "All" && !string.IsNullOrEmpty(statusFilter))
            {
                reservationsQuery = reservationsQuery.Where(r => r.Status == statusFilter);
            }

            var reservations = reservationsQuery
                .OrderByDescending(r => r.ReservationDate)
                .ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter;

            return View(reservations);
        }

        // ===================================
        // CREATE
        // ===================================
        [HttpGet]
        public IActionResult Create(int? carId = null)
        {
            // Cho phép user thường cũng có thể đặt xe
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            var model = new Reservation
            {
                ReservationDate = DateTime.Now,
                Status = "Pending",
                CarId = carId
            };

            // Nếu có carId từ route, tự động điền
            if (carId.HasValue)
            {
                var car = _db.Cars.Find(carId.Value);
                if (car != null)
                {
                    ViewBag.SelectedCar = car;
                }
            }

            // Tự động tìm hoặc tạo Customer từ User đã đăng nhập
            if (userId.HasValue)
            {
                var user = _db.Users.Find(userId.Value);
                if (user != null)
                {
                    // Tìm Customer theo email hoặc tên
                    var customer = _db.Customers
                        .FirstOrDefault(c => c.Email == user.Email || c.FullName == user.FullName);

                    if (customer == null && !string.IsNullOrEmpty(user.Email))
                    {
                        // Tạo Customer mới từ thông tin User
                        customer = new Customer
                        {
                            FullName = user.FullName ?? user.Username,
                            Email = user.Email,
                            CreatedAt = DateTime.Now
                        };
                        _db.Customers.Add(customer);
                        _db.SaveChanges();
                    }

                    if (customer != null)
                    {
                        model.CustomerId = customer.CustomerId;
                        ViewBag.CustomerName = customer.FullName;
                    }
                }
            }

            LoadDropdownsForUser(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reservation reservation)
        {
            // Cho phép user thường cũng có thể đặt xe
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            // Tự động lấy CustomerId từ User nếu chưa có
            if ((reservation.CustomerId == null || reservation.CustomerId == 0) && userId.HasValue)
            {
                var user = _db.Users.Find(userId.Value);
                if (user != null)
                {
                    var existingCustomer = _db.Customers
                        .FirstOrDefault(c => c.Email == user.Email || c.FullName == user.FullName);
                    
                    if (existingCustomer != null)
                    {
                        reservation.CustomerId = existingCustomer.CustomerId;
                    }
                }
            }

            // Validation
            if (reservation.CustomerId == null || reservation.CustomerId == 0)
                ModelState.AddModelError("CustomerId", "Vui lòng chọn khách hàng.");

            if (reservation.CarId == null || reservation.CarId == 0)
                ModelState.AddModelError("CarId", "Vui lòng chọn xe.");

            if (reservation.ReservationDate == null)
                ModelState.AddModelError("ReservationDate", "Vui lòng chọn ngày đặt.");

            if (reservation.ReservationDate.HasValue && reservation.ReservationDate.Value.Date < DateTime.Today)
                ModelState.AddModelError("ReservationDate", "Ngày đặt không thể trong quá khứ.");

            if (!ModelState.IsValid)
            {
                LoadDropdownsForUser(reservation);
                return View(reservation);
            }

            // Kiểm tra xe có sẵn không
            var car = _db.Cars.Find(reservation.CarId);
            if (car == null)
            {
                ModelState.AddModelError("CarId", "Xe không tồn tại.");
                LoadDropdownsForUser(reservation);
                return View(reservation);
            }

            if (car.Status != "Available")
            {
                ModelState.AddModelError("CarId", "Xe hiện không có sẵn để đặt.");
                LoadDropdownsForUser(reservation);
                return View(reservation);
            }

            // Kiểm tra khách hàng
            var customer = _db.Customers.Find(reservation.CustomerId);
            if (customer == null)
            {
                ModelState.AddModelError("CustomerId", "Khách hàng không tồn tại.");
                LoadDropdownsForUser(reservation);
                return View(reservation);
            }

            // Đặt giá trị mặc định
            if (string.IsNullOrWhiteSpace(reservation.Status))
                reservation.Status = "Pending";

            if (reservation.ReservationDate == null)
                reservation.ReservationDate = DateTime.Now;

            // Lưu vào database
            _db.Reservations.Add(reservation);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Đặt xe đã được tạo thành công! Chúng tôi sẽ liên hệ với bạn sớm nhất.";
            
            // Nếu là user thường, chuyển về trang chủ
            string role = HttpContext.Session.GetString("Role") ?? "";
            if (role == "User")
            {
                return RedirectToAction("Index", "Home");
            }
            
            return RedirectToAction("Index");
        }

        // ===================================
        // EDIT
        // ===================================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            if (!IsAuthorized() || role == "Cashier") return StatusCode(403);

            var reservation = _db.Reservations.Find(id);
            if (reservation == null) return NotFound();

            LoadDropdowns(reservation);
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Reservation reservation)
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            if (!IsAuthorized() || role == "Cashier") return StatusCode(403);

            // Validation
            if (reservation.CustomerId == null || reservation.CustomerId == 0)
                ModelState.AddModelError("CustomerId", "Vui lòng chọn khách hàng.");

            if (reservation.CarId == null || reservation.CarId == 0)
                ModelState.AddModelError("CarId", "Vui lòng chọn xe.");

            if (reservation.ReservationDate.HasValue && reservation.ReservationDate.Value.Date < DateTime.Today)
                ModelState.AddModelError("ReservationDate", "Ngày đặt không thể trong quá khứ.");

            if (!ModelState.IsValid)
            {
                LoadDropdowns(reservation);
                return View(reservation);
            }

            var existingReservation = _db.Reservations.Find(reservation.ReservationId);
            if (existingReservation == null) return NotFound();

            // Cập nhật thông tin
            existingReservation.CustomerId = reservation.CustomerId;
            existingReservation.CarId = reservation.CarId;
            existingReservation.ReservationDate = reservation.ReservationDate;
            existingReservation.Status = reservation.Status;

            _db.Reservations.Update(existingReservation);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Đặt xe đã được cập nhật!";
            return RedirectToAction("Index");
        }

        // ===================================
        // DELETE
        // ===================================
        [HttpGet]
        public IActionResult Delete(int id)
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            if (!IsAuthorized() || role == "Cashier") return StatusCode(403);

            var reservation = _db.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            if (!IsAuthorized() || role == "Cashier") return StatusCode(403);

            var reservation = _db.Reservations.Find(id);
            if (reservation == null) return NotFound();

            _db.Reservations.Remove(reservation);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Đặt xe đã được xóa!";
            return RedirectToAction("Index");
        }

        // ===================================
        // MY RESERVATIONS (Cho khách hàng xem đơn đặt của mình)
        // ===================================
        public IActionResult MyReservations()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var user = _db.Users.Find(userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");

            // Tìm Customer theo email hoặc tên
            var customer = _db.Customers
                .FirstOrDefault(c => c.Email == user.Email || c.FullName == user.FullName);

            if (customer == null)
            {
                TempData["InfoMessage"] = "Bạn chưa có đơn đặt xe nào.";
                return View(new List<Reservation>());
            }

            var reservations = _db.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Where(r => r.CustomerId == customer.CustomerId)
                .OrderByDescending(r => r.ReservationDate)
                .ToList();

            return View(reservations);
        }

        // ===================================
        // DETAILS
        // ===================================
        public IActionResult Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var reservation = _db.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Include(r => r.Notifications)
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            // Kiểm tra quyền: User chỉ xem được reservation của chính mình
            string role = HttpContext.Session.GetString("Role") ?? "";
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (role == "User" && userId.HasValue)
            {
                var user = _db.Users.Find(userId.Value);
                if (user != null)
                {
                    var customer = _db.Customers
                        .FirstOrDefault(c => c.Email == user.Email || c.FullName == user.FullName);
                    
                    if (customer == null || reservation.CustomerId != customer.CustomerId)
                    {
                        TempData["ErrorMessage"] = "Bạn không có quyền xem đặt xe này.";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            return View(reservation);
        }

        // ===================================
        // APPROVE RESERVATION (Chuyển thành Contract)
        // ===================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var reservation = _db.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đặt xe.";
                return RedirectToAction("Index");
            }

            if (reservation.Status == "Approved" || reservation.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Đặt xe này đã được xử lý.";
                return RedirectToAction("Details", new { id });
            }

            // Kiểm tra xe có sẵn không
            if (reservation.Car?.Status != "Available")
            {
                TempData["ErrorMessage"] = "Xe hiện không có sẵn.";
                return RedirectToAction("Details", new { id });
            }

            // Cập nhật trạng thái reservation
            reservation.Status = "Approved";
            _db.Reservations.Update(reservation);

            // Có thể tạo contract tự động ở đây nếu cần
            // Hoặc chuyển hướng đến trang tạo contract với thông tin đã điền sẵn

            _db.SaveChanges();

            TempData["SuccessMessage"] = "Đặt xe đã được duyệt! Bạn có thể tạo hợp đồng từ đặt xe này.";
            return RedirectToAction("Details", new { id });
        }

        // ===================================
        // CANCEL RESERVATION
        // ===================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var reservation = _db.Reservations.Find(id);
            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đặt xe.";
                return RedirectToAction("Index");
            }

            reservation.Status = "Cancelled";
            _db.Reservations.Update(reservation);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Đặt xe đã được hủy.";
            return RedirectToAction("Index");
        }

        // ===================================
        // HELPER: Load dropdowns (cho Admin/Manager/Cashier)
        // ===================================
        private void LoadDropdowns(Reservation reservation = null)
        {
            // Danh sách khách hàng
            ViewBag.CustomerId = new SelectList(
                _db.Customers.OrderBy(c => c.FullName),
                "CustomerId",
                "FullName",
                reservation?.CustomerId
            );

            // Danh sách xe có sẵn
            var availableCars = _db.Cars
                .Where(c => c.Status == "Available")
                .Select(c => new
                {
                    c.CarId,
                    DisplayText = $"{c.LicensePlate} - {c.Brand} {c.Model} ({c.VehicleType})"
                })
                .ToList();

            ViewBag.CarId = new SelectList(
                availableCars,
                "CarId",
                "DisplayText",
                reservation?.CarId
            );

            // Danh sách trạng thái
            var statuses = new[] { "Pending", "Approved", "Cancelled", "Completed" };
            ViewBag.StatusList = new SelectList(statuses, reservation?.Status ?? "Pending");
        }

        // ===================================
        // HELPER: Load dropdowns (cho User thường)
        // ===================================
        private void LoadDropdownsForUser(Reservation reservation = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            // Chỉ hiển thị Customer của chính user đó
            if (userId.HasValue)
            {
                var user = _db.Users.Find(userId.Value);
                if (user != null)
                {
                    var customer = _db.Customers
                        .FirstOrDefault(c => c.Email == user.Email || c.FullName == user.FullName);
                    
                    if (customer != null)
                    {
                        ViewBag.CustomerId = new SelectList(
                            new[] { customer },
                            "CustomerId",
                            "FullName",
                            reservation?.CustomerId ?? customer.CustomerId
                        );
                        ViewBag.CustomerName = customer.FullName;
                    }
                }
            }

            // Danh sách xe có sẵn
            var availableCars = _db.Cars
                .Where(c => c.Status == "Available")
                .Select(c => new
                {
                    c.CarId,
                    DisplayText = $"{c.LicensePlate} - {c.Brand} {c.Model} ({c.VehicleType}) - {c.PricePerDay:N0} VND/ngày"
                })
                .ToList();

            ViewBag.CarId = new SelectList(
                availableCars,
                "CarId",
                "DisplayText",
                reservation?.CarId
            );

            // User không thể chọn trạng thái, mặc định là Pending
            ViewBag.StatusList = null;
        }
    }
}
