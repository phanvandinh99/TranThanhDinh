using QuanLyThueXe.Models;

namespace QuanLyThueXe.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Lấy đường dẫn ảnh xe, nếu không có thì trả về ảnh mặc định
        /// </summary>
        public static string GetCarImageUrl(Car? car)
        {
            if (car == null)
                return "/images/cars/car404.jpg";

            if (!string.IsNullOrEmpty(car.ImageUrl))
            {
                // Nếu ImageUrl đã là đường dẫn đầy đủ
                if (car.ImageUrl.StartsWith("/"))
                {
                    // Sửa đường dẫn sai: motorbikes -> motobikes (không có chữ r)
                    string correctedUrl = car.ImageUrl.Replace("/images/motorbikes/", "/images/motobikes/");
                    return correctedUrl;
                }

                // Nếu không, xây dựng đường dẫn dựa trên VehicleType
                string folder = car.VehicleType == "Car" ? "cars" :
                                car.VehicleType == "Motorbike" ? "motobikes" : "cars";
                return $"/images/{folder}/{car.ImageUrl}";
            }

            // Mặc định trả về car404.jpg
            return "/images/cars/car404.jpg";
        }

        /// <summary>
        /// Lấy đường dẫn ảnh từ ImageUrl string
        /// </summary>
        public static string GetImageUrl(string? imageUrl, string vehicleType = "Car")
        {
            if (string.IsNullOrEmpty(imageUrl))
                return "/images/cars/car404.jpg";

            if (imageUrl.StartsWith("/"))
            {
                // Sửa đường dẫn sai: motorbikes -> motobikes (không có chữ r)
                string correctedUrl = imageUrl.Replace("/images/motorbikes/", "/images/motobikes/");
                return correctedUrl;
            }

            string folder = vehicleType == "Car" ? "cars" :
                            vehicleType == "Motorbike" ? "motobikes" : "cars";
            return $"/images/{folder}/{imageUrl}";
        }
    }
}

