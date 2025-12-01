# 📚 TÀI LIỆU GIẢI THÍCH CÁC THÀNH PHẦN TRONG DỰ ÁN

## 🎯 TỔNG QUAN

Dự án sử dụng 4 loại thành phần chính:
1. **Tasks** (Background Services) - Chạy tự động nền
2. **Helpers** - Các hàm tiện ích tái sử dụng
3. **View Components** - Component UI có logic
4. **Razor Partial Views** - Partial view tái sử dụng

---

## 1️⃣ TASKS (Background Services)

### 📍 Vị trí: `Tasks/`

Tasks là các dịch vụ chạy nền tự động, không cần người dùng tương tác.

### ✅ Các Task đã sử dụng:

#### 1. ContractStatusUpdateTask.cs
**Mục đích:** Tự động cập nhật trạng thái hợp đồng và xe khi hết hạn

**Cách hoạt động:**
- Chạy mỗi **1 giờ** một lần
- Tìm các hợp đồng có `Status = "Active"` nhưng `EndDate < hôm nay`
- Tự động đổi trạng thái hợp đồng thành `"Completed"`
- Cập nhật trạng thái xe về `"Available"` nếu không còn hợp đồng active khác

**Ví dụ:**
```
Hợp đồng #5: Thuê từ 01/12 → 05/12
Ngày 06/12: Task tự động chuyển trạng thái → "Completed"
Xe được trả về trạng thái "Available"
```

**Đăng ký trong Program.cs:**
```csharp
builder.Services.AddHostedService<ContractStatusUpdateTask>();
```

---

#### 2. NotificationTask.cs
**Mục đích:** Tự động tạo thông báo cho hợp đồng sắp hết hạn

**Cách hoạt động:**
- Chạy mỗi **1 ngày** một lần
- Tìm các hợp đồng sẽ hết hạn trong **3 ngày tới**
- Tạo thông báo trong bảng `Notifications`
- Tránh tạo thông báo trùng lặp

**Ví dụ:**
```
Hôm nay: 01/12
Hợp đồng #5: Hết hạn 04/12 (còn 3 ngày)
→ Task tạo thông báo: "Hợp đồng #5 sẽ hết hạn sau 3 ngày"
```

**Đăng ký trong Program.cs:**
```csharp
builder.Services.AddHostedService<NotificationTask>();
```

---

### 🔧 Cách sử dụng Task:

1. **Kế thừa BackgroundService:**
```csharp
public class MyTask : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Code chạy tự động
    }
}
```

2. **Sử dụng PeriodicTimer:**
```csharp
using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(1));
while (await timer.WaitForNextTickAsync(stoppingToken))
{
    // Chạy mỗi giờ
}
```

3. **Truy cập Database:**
```csharp
using var scope = _serviceProvider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();
```

---

## 2️⃣ HELPERS (Các hàm tiện ích)

### 📍 Vị trí: `Helpers/`

Helpers là các class static chứa các hàm tiện ích có thể tái sử dụng ở nhiều nơi.

### ✅ Các Helper đã sử dụng:

#### 1. ImageHelper.cs
**Mục đích:** Xử lý đường dẫn ảnh xe

**Các method:**
- `GetCarImageUrl(Car car)` - Lấy URL ảnh xe, nếu không có thì dùng ảnh mặc định
- `GetImageUrl(string imageUrl, string vehicleType)` - Lấy URL ảnh từ string

**Ví dụ sử dụng:**
```csharp
// Trong Controller hoặc View
var imageUrl = ImageHelper.GetCarImageUrl(car);
// Kết quả: "/images/cars/toyota.jpg" hoặc "/images/cars/car404.jpg" (nếu không có)
```

**Tính năng:**
- Tự động sửa đường dẫn sai (`motorbikes` → `motobikes`)
- Tự động chọn thư mục theo `VehicleType` (Car → cars, Motorbike → motobikes)
- Trả về ảnh mặc định nếu không có

---

#### 2. DateHelper.cs
**Mục đích:** Format và tính toán ngày tháng

**Các method:**
- `FormatDate(DateTime? date)` - Format: `dd/MM/yyyy`
- `FormatDateTime(DateTime? date)` - Format: `dd/MM/yyyy HH:mm`
- `FormatDateOnly(DateOnly? date)` - Format DateOnly: `dd/MM/yyyy`
- `CalculateDays(DateOnly start, DateOnly end)` - Tính số ngày thuê
- `IsContractExpired(DateOnly endDate)` - Kiểm tra hợp đồng hết hạn chưa
- `IsContractExpiringSoon(DateOnly endDate, int daysBefore = 3)` - Kiểm tra sắp hết hạn

**Ví dụ sử dụng:**
```csharp
// Trong View
@DateHelper.FormatDate(contract.StartDate)
// Kết quả: "01/12/2024"

@DateHelper.CalculateDays(contract.StartDate, contract.EndDate)
// Kết quả: 5 (ngày)
```

---

#### 3. PriceHelper.cs
**Mục đích:** Format và tính toán giá tiền

**Các method:**
- `FormatPrice(decimal? price)` - Format: `1,000,000 VND`
- `FormatPrice(decimal? price, string unit)` - Format với đơn vị tùy chỉnh
- `CalculateRentalTotal(decimal pricePerDay, int days, int quantity)` - Tính tổng tiền thuê
- `CalculateRemainingAmount(decimal total, decimal? deposit)` - Tính tiền còn lại

**Ví dụ sử dụng:**
```csharp
// Trong View
@PriceHelper.FormatPrice(car.PricePerDay)
// Kết quả: "800,000 VND"

@PriceHelper.CalculateRemainingAmount(contract.TotalAmount, contract.Deposit)
// Kết quả: 2,000,000 (sau khi trừ cọc)
```

---

#### 4. AuthHelper.cs
**Mục đích:** Kiểm tra quyền và lấy thông tin user từ Session

**Các method:**
- `IsLoggedIn(ISession session)` - Kiểm tra đã đăng nhập chưa
- `IsAdmin(ISession session)` - Kiểm tra có phải Admin không
- `IsManager(ISession session)` - Kiểm tra có phải Manager không
- `IsAuthorized(ISession session)` - Kiểm tra có quyền quản lý (Admin/Manager/Cashier)
- `IsCustomer(ISession session)` - Kiểm tra có phải Customer không
- `GetUserId(ISession session)` - Lấy UserId
- `GetUsername(ISession session)` - Lấy Username
- `GetRole(ISession session)` - Lấy Role

**Ví dụ sử dụng:**
```csharp
// Trong Controller
if (AuthHelper.IsAdmin(HttpContext.Session))
{
    // Chỉ Admin mới vào được
}

// Trong View
@if (AuthHelper.IsCustomer(Context.Session))
{
    <p>Bạn là khách hàng</p>
}
```

---

### 🔧 Cách tạo Helper mới:

```csharp
namespace QuanLyThueXe.Helpers
{
    public static class MyHelper
    {
        public static string MyMethod(string input)
        {
            // Logic xử lý
            return result;
        }
    }
}
```

**Sử dụng:**
```csharp
@using QuanLyThueXe.Helpers
@MyHelper.MyMethod("input")
```

---

## 3️⃣ VIEW COMPONENTS

### 📍 Vị trí: 
- Class: `ViewComponents/`
- View: `Views/Shared/Components/{ComponentName}/Default.cshtml`

View Components là các component UI có logic riêng, có thể inject services và truy cập database.

### ✅ Các View Component đã sử dụng:

#### 1. CarCardViewComponent
**Mục đích:** Hiển thị card xe với ảnh và giá đã format

**Class:** `ViewComponents/CarCardViewComponent.cs`
**View:** `Views/Shared/Components/CarCard/Default.cshtml`

**Cách hoạt động:**
1. Nhận `Car` object làm tham số
2. Sử dụng `ImageHelper` và `PriceHelper` để format dữ liệu
3. Truyền vào ViewBag
4. Render view Default.cshtml

**Ví dụ sử dụng:**
```razor
@await Component.InvokeAsync("CarCard", car)
```

**Kết quả:** Hiển thị card xe với ảnh, tên, giá, nút "Chi tiết" và "Thuê ngay"

---

#### 2. NotificationViewComponent
**Mục đích:** Hiển thị dropdown thông báo trong navbar

**Class:** `ViewComponents/NotificationViewComponent.cs`
**View:** `Views/Shared/Components/Notification/Default.cshtml`

**Cách hoạt động:**
1. Lấy UserId từ Session
2. Query database lấy 10 thông báo chưa đọc mới nhất
3. Đếm số thông báo chưa đọc
4. Render dropdown với badge số lượng

**Ví dụ sử dụng:**
```razor
@await Component.InvokeAsync("Notification")
```

**Kết quả:** Dropdown "🔔 Thông báo (5)" trong navbar

---

#### 3. UserMenuViewComponent
**Mục đích:** Hiển thị menu người dùng (đăng nhập/đăng xuất)

**Class:** `ViewComponents/UserMenuViewComponent.cs`
**View:** `Views/Shared/Components/UserMenu/Default.cshtml`

**Cách hoạt động:**
1. Lấy thông tin từ Session qua `AuthHelper`
2. Truyền vào ViewBag (IsLoggedIn, Username, Role, IsAdmin, etc.)
3. Render menu dropdown hoặc link đăng nhập

**Ví dụ sử dụng:**
```razor
@await Component.InvokeAsync("UserMenu")
```

**Kết quả:** 
- Nếu đã đăng nhập: Dropdown "👤 Username (Role)" với menu
- Nếu chưa: Link "Đăng nhập" và "Đăng ký"

---

### 🔧 Cách tạo View Component mới:

**1. Tạo Class:**
```csharp
namespace QuanLyThueXe.ViewComponents
{
    public class MyViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            // Logic
            return View();
        }
    }
}
```

**2. Tạo View:**
- Tạo file: `Views/Shared/Components/My/Default.cshtml`

**3. Sử dụng:**
```razor
@await Component.InvokeAsync("My")
```

---

## 4️⃣ RAZOR PARTIAL VIEWS

### 📍 Vị trí: `Views/Shared/`

Partial Views là các view nhỏ có thể include vào view khác, không có logic phức tạp.

### ✅ Các Partial View đã sử dụng:

#### 1. _CarCard.cshtml
**Mục đích:** Hiển thị card xe (phiên bản đơn giản hơn View Component)

**Vị trí:** `Views/Shared/_CarCard.cshtml`

**Cách sử dụng:**
```razor
@await Html.PartialAsync("_CarCard", car)
```

**Khác biệt với View Component:**
- Partial View: Không có class riêng, chỉ là HTML/Razor
- View Component: Có class riêng, có thể inject services, có logic

---

#### 2. _Layout.cshtml
**Mục đích:** Layout chính của ứng dụng

**Vị trí:** `Views/Shared/_Layout.cshtml`

**Tính năng:**
- Chứa HTML structure chung
- Include các View Components (Notification, UserMenu)
- Chứa navigation bar

---

#### 3. _ValidationScriptsPartial.cshtml
**Mục đích:** Include các script validation (jQuery Validation)

**Vị trí:** `Views/Shared/_ValidationScriptsPartial.cshtml`

**Cách sử dụng:**
```razor
@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

---

### 🔧 Cách tạo Partial View mới:

**1. Tạo file:** `Views/Shared/_MyPartial.cshtml`

**2. Sử dụng:**
```razor
@await Html.PartialAsync("_MyPartial", model)
```

---

## 📊 SO SÁNH CÁC THÀNH PHẦN

| Thành phần | Có Logic? | Có Database? | Có Services? | Tái sử dụng |
|------------|-----------|--------------|--------------|-------------|
| **Task** | ✅ | ✅ | ✅ | Tự động chạy |
| **Helper** | ✅ | ❌ | ❌ | Mọi nơi |
| **View Component** | ✅ | ✅ | ✅ | Trong Views |
| **Partial View** | ❌ | ❌ | ❌ | Trong Views |

---

## 🎯 KHI NÀO DÙNG GÌ?

### ✅ Dùng Task khi:
- Cần chạy tự động định kỳ (cập nhật trạng thái, gửi email, v.v.)
- Không cần người dùng tương tác

### ✅ Dùng Helper khi:
- Cần format dữ liệu (ngày, giá, ảnh)
- Cần kiểm tra quyền
- Logic đơn giản, không cần database

### ✅ Dùng View Component khi:
- UI có logic phức tạp (query database, xử lý dữ liệu)
- Cần inject services
- Cần tái sử dụng ở nhiều nơi với logic riêng

### ✅ Dùng Partial View khi:
- Chỉ cần HTML/Razor đơn giản
- Không cần logic phức tạp
- Chỉ để tách code cho dễ đọc

---

## 📝 VÍ DỤ TỔNG HỢP

### Trong một View:

```razor
@model Car
@using QuanLyThueXe.Helpers

<!-- Sử dụng Helper -->
<img src="@ImageHelper.GetCarImageUrl(Model)" />
<p>Giá: @PriceHelper.FormatPrice(Model.PricePerDay)</p>
<p>Ngày: @DateHelper.FormatDate(DateTime.Now)</p>

<!-- Sử dụng View Component -->
@await Component.InvokeAsync("CarCard", Model)

<!-- Sử dụng Partial View -->
@await Html.PartialAsync("_CarCard", Model)

<!-- Kiểm tra quyền với Helper -->
@if (AuthHelper.IsAdmin(Context.Session))
{
    <button>Sửa</button>
}
```

---

## 🔗 LIÊN KẾT CÁC THÀNH PHẦN

```
Task → Sử dụng Helper (DateHelper)
     ↓
View Component → Sử dụng Helper (ImageHelper, PriceHelper)
     ↓
View → Sử dụng Helper + View Component + Partial View
```

---

## ✅ TÓM TẮT

1. **Tasks**: Chạy tự động nền, cập nhật database định kỳ
2. **Helpers**: Hàm tiện ích format, kiểm tra quyền
3. **View Components**: UI component có logic, query database
4. **Partial Views**: HTML/Razor đơn giản, tái sử dụng

Tất cả các thành phần này giúp code:
- ✅ Dễ bảo trì
- ✅ Tái sử dụng
- ✅ Tổ chức tốt
- ✅ Giảm trùng lặp


