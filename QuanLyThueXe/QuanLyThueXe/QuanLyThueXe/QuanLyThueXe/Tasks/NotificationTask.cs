using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuanLyThueXe.Models;
using QuanLyThueXe.Helpers;

namespace QuanLyThueXe.Tasks
{
    /// <summary>
    /// Background task để tạo thông báo cho hợp đồng sắp hết hạn
    /// Chạy mỗi ngày để kiểm tra và tạo thông báo
    /// </summary>
    public class NotificationTask : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationTask> _logger;
        private readonly TimeSpan _period = TimeSpan.FromDays(1); // Chạy mỗi ngày

        public NotificationTask(
            IServiceProvider serviceProvider,
            ILogger<NotificationTask> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationTask đã khởi động");

            using PeriodicTimer timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await CreateExpiringContractNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo thông báo");
                }
            }
        }

        private async Task CreateExpiringContractNotificationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var threeDaysLater = today.AddDays(3);
            var createdCount = 0;

            // Tìm các hợp đồng sắp hết hạn (trong vòng 3 ngày)
            var expiringContracts = await dbContext.Contracts
                .Include(c => c.Customer)
                .Include(c => c.Car)
                .Where(c => c.Status == "Active" && 
                            c.EndDate >= today && 
                            c.EndDate <= threeDaysLater)
                .ToListAsync();

            foreach (var contract in expiringContracts)
            {
                // Kiểm tra xem đã có thông báo cho hợp đồng này chưa
                var existingNotification = await dbContext.Notifications
                    .AnyAsync(n => n.Message.Contains($"Hợp đồng #{contract.ContractId}") && 
                                  n.CreatedAt >= DateTime.Today);

                if (!existingNotification)
                {
                    var daysUntilExpiry = DateHelper.CalculateDays(today, contract.EndDate);
                    var message = $"Hợp đồng #{contract.ContractId} của khách hàng {contract.Customer?.FullName} " +
                                 $"sẽ hết hạn sau {daysUntilExpiry} ngày. " +
                                 $"Xe: {contract.Car?.Brand} {contract.Car?.Model}";

                    var notification = new Notification
                    {
                        Message = message,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };

                    dbContext.Notifications.Add(notification);
                    createdCount++;
                }
            }

            if (createdCount > 0)
            {
                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Đã tạo {createdCount} thông báo cho hợp đồng sắp hết hạn");
            }
        }
    }
}

