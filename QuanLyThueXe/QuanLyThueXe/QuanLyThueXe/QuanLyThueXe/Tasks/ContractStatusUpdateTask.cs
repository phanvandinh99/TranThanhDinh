using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuanLyThueXe.Models;
using QuanLyThueXe.Helpers;

namespace QuanLyThueXe.Tasks
{
    /// <summary>
    /// Background task để tự động cập nhật trạng thái hợp đồng và xe
    /// Chạy mỗi giờ để kiểm tra và cập nhật hợp đồng hết hạn
    /// </summary>
    public class ContractStatusUpdateTask : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ContractStatusUpdateTask> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Chạy mỗi giờ

        public ContractStatusUpdateTask(
            IServiceProvider serviceProvider,
            ILogger<ContractStatusUpdateTask> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ContractStatusUpdateTask đã khởi động");

            using PeriodicTimer timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await UpdateContractStatusesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật trạng thái hợp đồng");
                }
            }
        }

        private async Task UpdateContractStatusesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var updatedCount = 0;

            // Tìm các hợp đồng đã hết hạn nhưng vẫn còn trạng thái Active
            var expiredContracts = await dbContext.Contracts
                .Include(c => c.Car)
                .Where(c => c.Status == "Active" && c.EndDate < today)
                .ToListAsync();

            foreach (var contract in expiredContracts)
            {
                contract.Status = "Completed";
                
                // Cập nhật trạng thái xe về Available nếu không có hợp đồng active khác
                if (contract.Car != null)
                {
                    var hasOtherActiveContract = await dbContext.Contracts
                        .AnyAsync(c => c.CarId == contract.CarId && 
                                      c.ContractId != contract.ContractId && 
                                      c.Status == "Active");

                    if (!hasOtherActiveContract)
                    {
                        contract.Car.Status = "Available";
                    }
                }

                updatedCount++;
            }

            if (updatedCount > 0)
            {
                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Đã cập nhật {updatedCount} hợp đồng hết hạn");
            }
        }
    }
}

