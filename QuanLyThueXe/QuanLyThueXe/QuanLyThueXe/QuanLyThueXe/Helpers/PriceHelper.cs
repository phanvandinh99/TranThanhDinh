using System;

namespace QuanLyThueXe.Helpers
{
    public static class PriceHelper
    {
        /// <summary>
        /// Format giá tiền theo định dạng Việt Nam (N0 VND)
        /// </summary>
        public static string FormatPrice(decimal? price)
        {
            if (!price.HasValue)
                return "0 VND";
            return $"{price.Value:N0} VND";
        }

        /// <summary>
        /// Format giá tiền với đơn vị tùy chỉnh
        /// </summary>
        public static string FormatPrice(decimal? price, string unit)
        {
            if (!price.HasValue)
                return $"0 {unit}";
            return $"{price.Value:N0} {unit}";
        }

        /// <summary>
        /// Tính tổng tiền thuê xe
        /// </summary>
        public static decimal CalculateRentalTotal(decimal pricePerDay, int days, int quantity = 1)
        {
            return pricePerDay * days * quantity;
        }

        /// <summary>
        /// Tính số tiền còn lại sau khi trừ tiền cọc
        /// </summary>
        public static decimal CalculateRemainingAmount(decimal totalAmount, decimal? deposit)
        {
            return totalAmount - (deposit ?? 0);
        }
    }
}

