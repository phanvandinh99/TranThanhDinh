using System;

namespace QuanLyThueXe.Helpers
{
    public static class DateHelper
    {
        /// <summary>
        /// Format ngày theo định dạng Việt Nam (dd/MM/yyyy)
        /// </summary>
        public static string FormatDate(DateTime? date)
        {
            if (!date.HasValue)
                return "N/A";
            return date.Value.ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Format ngày theo định dạng Việt Nam (dd/MM/yyyy HH:mm)
        /// </summary>
        public static string FormatDateTime(DateTime? date)
        {
            if (!date.HasValue)
                return "N/A";
            return date.Value.ToString("dd/MM/yyyy HH:mm");
        }

        /// <summary>
        /// Format DateOnly theo định dạng Việt Nam (dd/MM/yyyy)
        /// </summary>
        public static string FormatDateOnly(DateOnly? date)
        {
            if (!date.HasValue)
                return "N/A";
            return date.Value.ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Tính số ngày giữa hai ngày
        /// </summary>
        public static int CalculateDays(DateOnly startDate, DateOnly endDate)
        {
            return (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
        }

        /// <summary>
        /// Kiểm tra xem hợp đồng có hết hạn không
        /// </summary>
        public static bool IsContractExpired(DateOnly endDate)
        {
            return endDate < DateOnly.FromDateTime(DateTime.Now);
        }

        /// <summary>
        /// Kiểm tra xem hợp đồng sắp hết hạn (trong vòng 3 ngày)
        /// </summary>
        public static bool IsContractExpiringSoon(DateOnly endDate, int daysBefore = 3)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var daysUntilExpiry = (endDate.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).Days;
            return daysUntilExpiry >= 0 && daysUntilExpiry <= daysBefore;
        }
    }
}

