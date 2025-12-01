using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models;
using System.Linq;
using System.Text;

namespace QuanLyThueXe.Controllers
{
    public class ReportController : Controller
    {
        private readonly CarRentalDbContext _db;

        public ReportController(CarRentalDbContext db)
        {
            _db = db;
        }

        private bool IsAuthorized()
        {
            string role = HttpContext.Session.GetString("Role") ?? "";
            return role == "Admin" || role == "Manager";
        }

        // ========================
        // TRANG BÁO CÁO
        // ========================
        public IActionResult Index()
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var contracts = _db.Contracts
                .Include(c => c.Customer)
                .Include(c => c.Car)
                .Where(c => c.Status != "Cancelled")
                .OrderByDescending(c => c.ContractDate)
                .ToList();

            return View(contracts);
        }

        // ========================
        // XUẤT EXCEL DẠNG CSV (Excel đọc OK)
        // ========================
        public IActionResult ExportContractsCsv()
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var contracts = _db.Contracts
                .Include(c => c.Customer)
                .Include(c => c.Car)
                .Where(c => c.Status != "Cancelled")
                .OrderByDescending(c => c.ContractDate)
                .ToList();

            var csv = new StringBuilder();

            csv.AppendLine("Mã HĐ,Khách hàng,Biển số,Ngày thuê,Ngày trả,Tổng tiền,Trạng thái");

            foreach (var c in contracts)
            {
                csv.AppendLine(
                    $"{c.ContractId}," +
                    $"{c.Customer?.FullName}," +
                    $"{c.Car?.LicensePlate}," +
                    $"{c.StartDate:dd/MM/yyyy}," +
                    $"{c.EndDate:dd/MM/yyyy}," +
                    $"{c.TotalAmount}," +
                    $"{c.Status}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"BaoCaoHopDong_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }
    }
}
