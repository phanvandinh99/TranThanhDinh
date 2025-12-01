using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models;
using QuanLyThueXe.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Thêm DbContext
builder.Services.AddDbContext<CarRentalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Nếu dùng session
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Đăng ký Background Services (Tasks)
builder.Services.AddHostedService<ContractStatusUpdateTask>();
builder.Services.AddHostedService<NotificationTask>();

var app = builder.Build();

app.UseSession();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
