using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models;
using QuanLyThueXe.Helpers;

namespace QuanLyThueXe.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly CarRentalDbContext _db;

        public NotificationViewComponent(CarRentalDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                
                if (!userId.HasValue)
                {
                    return View(new List<Notification>());
                }

                // Kiểm tra xem bảng Notifications có tồn tại không
                // Nếu không tồn tại, trả về danh sách rỗng
                try
                {
                    // Lấy thông báo chưa đọc, sắp xếp theo thời gian mới nhất
                    var notifications = await _db.Notifications
                        .Where(n => !n.IsRead.HasValue || !n.IsRead.Value)
                        .OrderByDescending(n => n.CreatedAt)
                        .Take(10)
                        .ToListAsync();

                    ViewBag.UnreadCount = notifications.Count;

                    return View(notifications);
                }
                catch (Microsoft.Data.SqlClient.SqlException)
                {
                    // Bảng chưa tồn tại, trả về danh sách rỗng
                    ViewBag.UnreadCount = 0;
                    return View(new List<Notification>());
                }
            }
            catch
            {
                // Xử lý mọi lỗi khác, trả về danh sách rỗng
                ViewBag.UnreadCount = 0;
                return View(new List<Notification>());
            }
        }
    }
}

