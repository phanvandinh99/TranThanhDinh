using Microsoft.EntityFrameworkCore;
using QuanLyThueXe.Models;
using QuanLyThueXe.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Thêm hỗ trợ Razor Pages
builder.Services.AddServerSideBlazor(); // Thêm hỗ trợ Blazor Server

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

// Map Razor Pages
app.MapRazorPages();

// Map Blazor Hub
app.MapBlazorHub();

// Map MVC Controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
