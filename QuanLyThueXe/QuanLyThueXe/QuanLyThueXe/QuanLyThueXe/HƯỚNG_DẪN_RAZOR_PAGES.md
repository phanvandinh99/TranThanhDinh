# 📚 HƯỚNG DẪN SỬ DỤNG RAZOR PAGES TRONG DỰ ÁN

## ✅ ĐÃ THỰC HIỆN

Đã chuyển trang **Home/Index** sang Razor Pages làm ví dụ.

---

## 📁 CẤU TRÚC ĐÃ TẠO

```
Pages/
├── _ViewImports.cshtml      ← Import namespaces chung
├── _ViewStart.cshtml        ← Layout mặc định
└── Home/
    ├── Index.cshtml         ← View (Razor)
    └── Index.cshtml.cs      ← PageModel (code-behind) ✅ FILE NÀY!
```

---

## 🔗 ROUTING

### Razor Pages:
- **URL:** `/HomeRazor` (custom route để tránh conflict)
- **File:** `Pages/Home/Index.cshtml`

### MVC (gốc):
- **URL:** `/Home/Index`
- **File:** `Views/Home/Index.cshtml`

**Cả 2 đều hoạt động song song!**

---

## 📝 SO SÁNH CODE

### 1. MVC (Gốc)

**HomeController.cs:**
```csharp
public class HomeController : Controller
{
    public IActionResult Index(string vehicleType)
    {
        var cars = _db.Cars.ToList();
        return View(cars);
    }
}
```

**Views/Home/Index.cshtml:**
```razor
@model IEnumerable<Car>
@foreach (var car in Model) { ... }
```

---

### 2. Razor Pages (Mới)

**Pages/Home/Index.cshtml.cs:**
```csharp
public class IndexModel : PageModel
{
    public List<Car> Cars { get; set; }
    
    public void OnGet(string vehicleType)
    {
        Cars = _db.Cars.ToList();
    }
}
```

**Pages/Home/Index.cshtml:**
```razor
@page "/HomeRazor"
@model IndexModel
@foreach (var car in Model.Cars) { ... }
```

---

## 🎯 ĐIỂM KHÁC BIỆT

| Đặc điểm | MVC | Razor Pages |
|----------|-----|-------------|
| **Logic** | Controller | PageModel (`.cshtml.cs`) |
| **View** | `Views/Controller/Action.cshtml` | `Pages/Folder/Page.cshtml` |
| **Routing** | `{controller}/{action}` | `@page` directive |
| **Model binding** | `@model` | `@model PageModel` |
| **File `.cshtml.cs`** | ❌ | ✅ |

---

## 🔧 CÁCH SỬ DỤNG

### Truy cập trang Razor Pages:
```
http://localhost:7055/HomeRazor
```

### Truy cập trang MVC (gốc):
```
http://localhost:7055/Home/Index
```

---

## 📋 CÁC METHOD TRONG PAGEMODEL

### OnGet() - Xử lý GET request
```csharp
public void OnGet()
{
    // Load data
}
```

### OnPost() - Xử lý POST request
```csharp
public IActionResult OnPost()
{
    // Xử lý form submit
    return RedirectToPage("/Index");
}
```

### OnGetAsync() - Async version
```csharp
public async Task OnGetAsync()
{
    Cars = await _db.Cars.ToListAsync();
}
```

### Với parameters:
```csharp
public void OnGet(string vehicleType, int? page)
{
    // Nhận parameters từ query string
}
```

---

## 🎨 BINDING PROPERTIES

### Property binding:
```csharp
[BindProperty]
public string VehicleType { get; set; }

[BindProperty(SupportsGet = true)]  // Cho GET request
public int? Page { get; set; }
```

### Model binding:
```csharp
public async Task<IActionResult> OnPostAsync(Car car)
{
    // car được bind tự động từ form
}
```

---

## 🔄 CHUYỂN ĐỔI TỪ MVC SANG RAZOR PAGES

### Bước 1: Tạo PageModel
- Tạo file `Pages/{Folder}/{Page}.cshtml.cs`
- Kế thừa `PageModel`
- Chuyển logic từ Controller action sang `OnGet()` hoặc `OnPost()`

### Bước 2: Tạo View
- Tạo file `Pages/{Folder}/{Page}.cshtml`
- Thêm `@page` directive
- Thêm `@model PageModelClass`
- Cập nhật syntax: `Model.Property` thay vì `@Model.Property`

### Bước 3: Cập nhật routing
- Dùng `@page "/custom-route"` để custom route
- Hoặc để mặc định: `Pages/Home/Index.cshtml` → `/Home/Index`

---

## ✅ VÍ DỤ HOÀN CHỈNH

### Pages/Home/Index.cshtml.cs:
```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuanLyThueXe.Models;

namespace QuanLyThueXe.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly CarRentalDbContext _db;

        public IndexModel(CarRentalDbContext db)
        {
            _db = db;
        }

        public List<Car> Cars { get; set; } = new();
        public SelectList VehicleTypes { get; set; } = null!;

        public void OnGet(string vehicleType)
        {
            var types = new[] { "All", "Car", "Motorbike" };
            VehicleTypes = new SelectList(types, vehicleType ?? "All");

            var cars = _db.Cars.AsQueryable();
            if (!string.IsNullOrEmpty(vehicleType) && vehicleType != "All")
            {
                cars = cars.Where(c => c.VehicleType == vehicleType);
            }

            Cars = cars.ToList();
        }
    }
}
```

### Pages/Home/Index.cshtml:
```razor
@page "/HomeRazor"
@model QuanLyThueXe.Pages.Home.IndexModel

<h2>Danh sách xe</h2>

<form method="get">
    <select name="vehicleType" asp-items="Model.VehicleTypes"></select>
    <button type="submit">Tìm</button>
</form>

@foreach (var car in Model.Cars)
{
    @await Component.InvokeAsync("CarCard", car)
}
```

---

## 🚀 TEST

1. Chạy ứng dụng
2. Truy cập: `http://localhost:7055/HomeRazor`
3. Kiểm tra xem trang có hiển thị đúng không
4. So sánh với trang MVC gốc: `http://localhost:7055/Home/Index`

---

## 📌 LƯU Ý

1. **Có thể dùng cả MVC và Razor Pages** trong cùng một project
2. **Routing:** Razor Pages mặc định route theo cấu trúc folder
3. **Layout:** Dùng chung `_Layout.cshtml` từ `Views/Shared/`
4. **View Components:** Vẫn dùng được bình thường
5. **Helpers:** Vẫn dùng được bình thường

---

## 🎯 KẾT LUẬN

✅ Đã tạo thành công trang Razor Pages với file `.cshtml.cs`!

Bây giờ bạn có thể:
- Xem ví dụ tại `/HomeRazor`
- Tạo thêm các trang Razor Pages khác
- Dùng cả MVC và Razor Pages song song

