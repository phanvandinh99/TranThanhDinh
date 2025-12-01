using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models;
using QuanLyThueXe.Helpers;

namespace QuanLyThueXe.Controllers
{
    public class NotificationController : Controller
    {
        private readonly CarRentalDbContext _db;

        public NotificationController(CarRentalDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Hiển thị danh sách tất cả thông báo
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var notifications = await _db.Notifications
                    .Include(n => n.Reservation)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                var unreadCount = notifications.Count(n => !n.IsRead.HasValue || !n.IsRead.Value);
                ViewBag.UnreadCount = unreadCount;
                ViewBag.TotalCount = notifications.Count;

                return View(notifications);
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                // Bảng chưa tồn tại
                ViewBag.UnreadCount = 0;
                ViewBag.TotalCount = 0;
                return View(new List<Notification>());
            }
        }

        /// <summary>
        /// Đánh dấu thông báo là đã đọc
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var notification = await _db.Notifications.FindAsync(id);
                if (notification != null)
                {
                    notification.IsRead = true;
                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã đánh dấu thông báo là đã đọc.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông báo.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đánh dấu đã đọc.";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo là đã đọc
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var unreadNotifications = await _db.Notifications
                    .Where(n => !n.IsRead.HasValue || !n.IsRead.Value)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã đánh dấu {unreadNotifications.Count} thông báo là đã đọc.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đánh dấu đã đọc.";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa thông báo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var notification = await _db.Notifications.FindAsync(id);
                if (notification != null)
                {
                    _db.Notifications.Remove(notification);
                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa thông báo thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông báo.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa thông báo.";
            }

            return RedirectToAction("Index");
        }
    }
}
