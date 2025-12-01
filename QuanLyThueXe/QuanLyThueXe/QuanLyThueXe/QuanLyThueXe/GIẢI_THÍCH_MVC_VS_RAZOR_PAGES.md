# 📚 GIẢI THÍCH: MVC vs RAZOR PAGES

## ❓ CÂU HỎI: Tại sao không có file `.cshtml.cs`?

**Trả lời:** Dự án của bạn đang dùng **MVC (Model-View-Controller)**, không phải **Razor Pages**. 

---

## 🔄 SO SÁNH 2 MÔ HÌNH

### 1️⃣ MVC (Model-View-Controller) - Dự án hiện tại

**Cấu trúc:**
```
Controllers/
  └── HomeController.cs          ← Logic ở đây
Views/
  └── Home/
      └── Index.cshtml           ← Chỉ có HTML/Razor, KHÔNG có .cshtml.cs
```

**Cách hoạt động:**
1. **Controller** (`HomeController.cs`) chứa logic
2. **View** (`Index.cshtml`) chỉ hiển thị
3. **KHÔNG có** file `.cshtml.cs`

**Ví dụ trong dự án:**

**HomeController.cs:**
```csharp
public class HomeController : Controller
{
    public IActionResult Index(string vehicleType)
    {
        // Logic ở đây
        var cars = _db.Cars.ToList();
        return View(cars);  // Truyền data vào View
    }
}
```

**Index.cshtml:**
```razor
@model IEnumerable<Car>  ← Nhận data từ Controller

@foreach (var car in Model)
{
    <p>@car.Brand</p>
}
```

---

### 2️⃣ RAZOR PAGES - Có file `.cshtml.cs`

**Cấu trúc:**
```
Pages/
  └── Home/
      ├── Index.cshtml           ← View
      └── Index.cshtml.cs        ← PageModel (code-behind) ← FILE NÀY!
```

**Cách hoạt động:**
1. **PageModel** (`Index.cshtml.cs`) chứa logic
2. **View** (`Index.cshtml`) hiển thị
3. **CÓ** file `.cshtml.cs` (PageModel)

**Ví dụ nếu dùng Razor Pages:**

**Index.cshtml.cs:**
```csharp
public class IndexModel : PageModel
{
    private readonly CarRentalDbContext _db;
    
    public IndexModel(CarRentalDbContext db)
    {
        _db = db;
    }
    
    public List<Car> Cars { get; set; }
    
    public void OnGet(string vehicleType)  // Logic ở đây
    {
        Cars = _db.Cars.ToList();
    }
}
```

**Index.cshtml:**
```razor
@page
@model IndexModel

@foreach (var car in Model.Cars)  ← Truy cập qua Model
{
    <p>@car.Brand</p>
}
```

---

## 📊 BẢNG SO SÁNH

| Đặc điểm | MVC | Razor Pages |
|----------|-----|-------------|
| **File structure** | `Controller.cs` + `View.cshtml` | `Page.cshtml` + `Page.cshtml.cs` |
| **Logic ở đâu?** | Controller | PageModel (`.cshtml.cs`) |
| **Routing** | `{controller}/{action}` | `@page` directive |
| **Phù hợp** | Ứng dụng lớn, nhiều logic | Trang đơn giản, ít logic |
| **File `.cshtml.cs`** | ❌ KHÔNG có | ✅ CÓ |

---

## 🎯 DỰ ÁN CỦA BẠN ĐANG DÙNG MVC

### ✅ Cấu trúc hiện tại:

```
QuanLyThueXe/
├── Controllers/              ← Logic ở đây
│   ├── HomeController.cs
│   ├── CarController.cs
│   └── ...
├── Views/                    ← Chỉ có .cshtml, KHÔNG có .cshtml.cs
│   ├── Home/
│   │   └── Index.cshtml
│   └── Car/
│       └── Index.cshtml
```

### ❌ KHÔNG có:
- `Pages/` folder
- File `.cshtml.cs`
- `@page` directive trong views

### ✅ CÓ:
- `Controllers/` folder
- `Views/` folder với `.cshtml`
- Routing: `/Home/Index`, `/Car/Index`

---

## 🔧 NẾU MUỐN DÙNG RAZOR PAGES

### Bước 1: Thay đổi Program.cs

**Hiện tại (MVC):**
```csharp
builder.Services.AddControllersWithViews();
app.MapControllerRoute(...);
```

**Nếu chuyển sang Razor Pages:**
```csharp
builder.Services.AddRazorPages();
app.MapRazorPages();
```

### Bước 2: Tạo cấu trúc Pages

```
Pages/
  └── Home/
      ├── Index.cshtml
      └── Index.cshtml.cs      ← File code-behind
```

### Bước 3: Tạo PageModel

**Index.cshtml.cs:**
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
        
        public List<Car> Cars { get; set; }
        
        public void OnGet(string vehicleType)
        {
            Cars = _db.Cars.ToList();
        }
    }
}
```

### Bước 4: Tạo View

**Index.cshtml:**
```razor
@page
@model QuanLyThueXe.Pages.Home.IndexModel

<h2>Danh sách xe</h2>
@foreach (var car in Model.Cars)
{
    <p>@car.Brand</p>
}
```

---

## ❓ KHI NÀO DÙNG GÌ?

### ✅ Dùng MVC khi:
- Ứng dụng lớn, phức tạp
- Cần tách biệt rõ ràng Controller và View
- Nhiều trang dùng chung logic
- Đã quen với MVC pattern

### ✅ Dùng Razor Pages khi:
- Trang đơn giản, ít logic
- Mỗi trang độc lập
- Muốn code gần nhau (view và logic cùng folder)
- Ứng dụng nhỏ, ít trang

---

## 🎯 KẾT LUẬN

**Dự án của bạn:**
- ✅ Đang dùng **MVC**
- ❌ **KHÔNG có** file `.cshtml.cs`
- ✅ Logic nằm trong **Controllers**
- ✅ Views chỉ có file `.cshtml`

**File `.cshtml.cs` chỉ có trong Razor Pages, không có trong MVC!**

---

## 📝 VÍ DỤ MINH HỌA

### MVC (Dự án hiện tại):

**HomeController.cs:**
```csharp
public IActionResult Index()
{
    var cars = _db.Cars.ToList();
    return View(cars);
}
```

**Index.cshtml:**
```razor
@model IEnumerable<Car>
@foreach (var car in Model) { ... }
```

---

### Razor Pages (Nếu chuyển):

**Index.cshtml.cs:**
```csharp
public class IndexModel : PageModel
{
    public void OnGet()
    {
        Cars = _db.Cars.ToList();
    }
}
```

**Index.cshtml:**
```razor
@page
@model IndexModel
@foreach (var car in Model.Cars) { ... }
```

---

## ✅ TÓM TẮT

| Câu hỏi | Trả lời |
|---------|---------|
| **Có file `.cshtml.cs` không?** | ❌ KHÔNG (vì dùng MVC) |
| **Logic ở đâu?** | ✅ Controllers |
| **View ở đâu?** | ✅ Views/*.cshtml |
| **Có thể thêm `.cshtml.cs` không?** | ❌ KHÔNG cần (MVC không dùng) |

**Nếu muốn dùng `.cshtml.cs`, phải chuyển sang Razor Pages!**

