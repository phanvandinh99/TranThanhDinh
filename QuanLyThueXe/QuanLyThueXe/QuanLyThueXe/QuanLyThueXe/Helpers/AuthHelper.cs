using Microsoft.AspNetCore.Http;

namespace QuanLyThueXe.Helpers
{
    public static class AuthHelper
    {
        /// <summary>
        /// Kiểm tra user đã đăng nhập chưa
        /// </summary>
        public static bool IsLoggedIn(ISession session)
        {
            return session.GetInt32("UserId") != null;
        }

        /// <summary>
        /// Kiểm tra user có quyền Admin hoặc Manager
        /// </summary>
        public static bool IsAdminOrManager(ISession session)
        {
            string role = session.GetString("Role") ?? "";
            return role == "Admin" || role == "Manager";
        }

        /// <summary>
        /// Kiểm tra user có quyền Admin
        /// </summary>
        public static bool IsAdmin(ISession session)
        {
            string role = session.GetString("Role") ?? "";
            return role == "Admin";
        }

        /// <summary>
        /// Kiểm tra user có quyền quản lý (Admin, Manager, Cashier)
        /// </summary>
        public static bool IsAuthorized(ISession session)
        {
            string role = session.GetString("Role") ?? "";
            return role == "Admin" || role == "Manager" || role == "Cashier";
        }

        /// <summary>
        /// Lấy UserId từ session
        /// </summary>
        public static int? GetUserId(ISession session)
        {
            return session.GetInt32("UserId");
        }

        /// <summary>
        /// Lấy Username từ session
        /// </summary>
        public static string GetUsername(ISession session)
        {
            return session.GetString("Username") ?? "Khách";
        }

        /// <summary>
        /// Lấy Role từ session
        /// </summary>
        public static string GetRole(ISession session)
        {
            return session.GetString("Role") ?? "User";
        }
    }
}

