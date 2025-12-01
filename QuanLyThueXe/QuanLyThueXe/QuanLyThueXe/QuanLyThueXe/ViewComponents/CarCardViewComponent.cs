using Microsoft.AspNetCore.Mvc;
using QuanLyThueXe.Models;
using QuanLyThueXe.Helpers;

namespace QuanLyThueXe.ViewComponents
{
    public class CarCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Car car)
        {
            // Sử dụng ImageHelper để lấy ảnh
            ViewBag.ImageUrl = ImageHelper.GetCarImageUrl(car);
            ViewBag.PriceFormatted = PriceHelper.FormatPrice(car.PricePerDay);
            
            return View(car);
        }
    }
}

