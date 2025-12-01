-- Phần 1: Xóa và tạo lại DB
USE master;
IF DB_ID('CarRentalDB') IS NOT NULL
BEGIN
    ALTER DATABASE CarRentalDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CarRentalDB;
END
GO
CREATE DATABASE CarRentalDB;
GO

-- Phần 2: Sử dụng DB và tạo cấu trúc
USE CarRentalDB;
GO
-- Bảng Users
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100),
    Email NVARCHAR(100) NULL,
    Role NVARCHAR(20) DEFAULT 'User'
);
GO

-- Thêm dữ liệu mẫu cho Users
INSERT INTO Users (Username, PasswordHash, FullName, Email, Role) VALUES
('admin', '123456', N'Quản trị viên', 'admin@email.com', 'Quản trị viên'),
('customer1', '123456', N'Khách hàng A', 'customer1@email.com', 'Khách hàng'),
('employee1', '123456', N'Nhân viên A', 'employee1@email.com', 'Nhân viên'),
('manager1', '123456', N'Quản lý A', 'manager1@email.com', 'Quản lý'),
('cashier1', '123456', N'Thu ngân A', 'cashier1@email.com', 'Thu ngân');
GO

-- Chuẩn hóa vai trò (Role)
UPDATE Users SET Role = 'Admin'    WHERE Role COLLATE SQL_Latin1_General_CP1_CI_AS IN ('admin', 'ADMIN', 'Quản trị viên', 'QuanTri');
UPDATE Users SET Role = 'Manager'  WHERE Role COLLATE SQL_Latin1_General_CP1_CI_AS IN ('manager', 'MANAGER', 'Quản lý', 'QuanLy');
UPDATE Users SET Role = 'Employee' WHERE Role COLLATE SQL_Latin1_General_CP1_CI_AS IN ('employee', 'EMPLOYEE', 'Nhân viên', 'NhanVien');
UPDATE Users SET Role = 'Cashier'  WHERE Role COLLATE SQL_Latin1_General_CP1_CI_AS IN ('cashier', 'CASHIER', 'Thu ngân', 'ThuNgan');
UPDATE Users SET Role = 'Customer' WHERE Role COLLATE SQL_Latin1_General_CP1_CI_AS IN ('customer', 'CUSTOMER', 'Khách hàng', 'KhachHang', 'User');
GO

-- Bảng Cars
CREATE TABLE Cars (
    CarId INT PRIMARY KEY IDENTITY(1,1),
    LicensePlate NVARCHAR(20) NOT NULL UNIQUE,
    Brand NVARCHAR(50) NOT NULL,
    Model NVARCHAR(50) NOT NULL,
    PricePerDay DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Available',
    VehicleType NVARCHAR(20) DEFAULT 'Car',
    ImageUrl NVARCHAR(255) NULL
);
GO

-- Thêm cột tính toán CarName
ALTER TABLE Cars
ADD CarName AS (Brand + ' ' + Model) PERSISTED;
GO

-- Thêm dữ liệu ô tô
INSERT INTO Cars (LicensePlate, Brand, Model, PricePerDay, Status, VehicleType, ImageUrl) VALUES
('51A-12345', 'Toyota', 'Vios',      800000, 'Available', 'Car', '/images/cars/vios.jpg'),
('51B-67890', 'Toyota', 'Fortuner', 1200000, 'Available', 'Car', '/images/cars/fortuner.jpg'),
('60C-11223', 'Honda',  'Civic',     900000, 'Available', 'Car', '/images/cars/honda.png'),
('59D-44556', 'Mazda',  'CX-5',     1000000, 'Available', 'Car', '/images/cars/mazda.png'),
('72A-77889', 'Kia',    'Morning',   500000, 'Available', 'Car', '/images/cars/morning.jpg'),
('51F-33445', 'Hyundai','Accent',    700000, 'Available', 'Car', '/images/cars/accent.jpg'),
('43G-55667', 'Ford',   'Everest',  1100000, 'Available', 'Car', '/images/cars/everest.jpg'),
('29H-88990', 'VinFast','Lux A2.0', 950000, 'Available', 'Car', '/images/cars/vinfast.jpg'),
('30K-11234', 'Mercedes','C200',   1500000, 'Available', 'Car', '/images/cars/mercedes.jpg'),
('31L-55678', 'BMW',    'X5',       2000000, 'Available', 'Car', '/images/cars/bmw.jpg');

-- Thêm dữ liệu xe máy
INSERT INTO Cars (LicensePlate, Brand, Model, PricePerDay, Status, VehicleType, ImageUrl) VALUES
('59X1-12345', 'Honda', 'Air Blade',   150000, 'Available', 'Motorbike', '/images/motorbikes/airblade.jpg'),
('59Y2-67890', 'Yamaha','Exciter 150', 180000, 'Available', 'Motorbike', '/images/motorbikes/exciter.png'),
('50Z3-11223', 'Honda', 'SH 150i',     250000, 'Available', 'Motorbike', '/images/motorbikes/sh150i.jpg'),
('60M4-44556', 'Suzuki','Raider',      160000, 'Available', 'Motorbike', '/images/motorbikes/raider.jpg');
GO

-- Chuẩn hóa VehicleType (mặc dù đã chuẩn khi insert, nhưng thêm đảm bảo)
UPDATE Cars
SET VehicleType = 'Car'
WHERE LOWER(LTRIM(RTRIM(VehicleType))) IN ('car', 'oto', 'xe hơi', 'xehoi');

UPDATE Cars
SET VehicleType = 'Motorbike'
WHERE LOWER(LTRIM(RTRIM(VehicleType))) IN ('motorbike', 'motobike', 'xe máy', 'xe may', 'moto')
   OR Brand IN ('Honda', 'Yamaha', 'Suzuki', 'Piaggio');
GO

-- Ràng buộc kiểm tra loại phương tiện
ALTER TABLE Cars
ADD CONSTRAINT CK_Cars_VehicleType CHECK (VehicleType IN ('Car', 'Motorbike'));
GO

-- Bảng Customers
CREATE TABLE Customers (
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    IdentityCard NVARCHAR(50),
    Address NVARCHAR(255),
    Email NVARCHAR(100),
    Gender NVARCHAR(10),
    BirthDate DATE,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CarId INT NULL
);
GO

-- Khóa ngoại sau khi bảng Cars tồn tại
ALTER TABLE Customers
ADD CONSTRAINT FK_Customers_Cars FOREIGN KEY (CarId) REFERENCES Cars(CarId);
GO

-- Bảng Reservations
CREATE TABLE Reservations (
    ReservationId INT PRIMARY KEY IDENTITY(1,1),
    CarId INT NOT NULL FOREIGN KEY REFERENCES Cars(CarId),
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customers(CustomerId),
    ReservationDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Pending'
);
GO

-- Bảng Contracts
CREATE TABLE Contracts (
    ContractId INT PRIMARY KEY IDENTITY(1,1),
    CarId INT NOT NULL FOREIGN KEY REFERENCES Cars(CarId),
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customers(CustomerId),
    ContractDate DATETIME DEFAULT GETDATE(),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Deposit DECIMAL(18,2) DEFAULT 0,
    Quantity INT NOT NULL DEFAULT 1,
    Status NVARCHAR(20) DEFAULT 'Active'
);
GO

-- Bảng Drivers
CREATE TABLE Drivers (
    DriverId INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Gender NVARCHAR(10),
    Age INT,
    Phone NVARCHAR(20),
    ExperienceYears INT,
    PricePerDay DECIMAL(10,2),
    ImageUrl NVARCHAR(255),
    Status NVARCHAR(20) DEFAULT 'Available',
    Description NVARCHAR(255),
    LicenseType NVARCHAR(50),
    IsAvailable BIT DEFAULT 1
);
GO

-- Dữ liệu mẫu cho Drivers
INSERT INTO Drivers (FullName, Gender, Age, Phone, ExperienceYears, PricePerDay, ImageUrl, Description) VALUES
(N'Nguyễn Văn A', N'Nam', 32, '0905123456', 5, 500000, '/images/drivers/driver1.jpg', N'Tài xế chuyên nghiệp, thân thiện.'),
(N'Trần Thị B', N'Nữ', 29, '0905234567', 4, 480000, '/images/drivers/driver2.jpg', N'Lái xe cẩn thận, am hiểu đường xá.'),
(N'Lê Văn C', N'Nam', 40, '0905345678', 10, 600000, '/images/drivers/driver3.jpg', N'Kinh nghiệm lái xe đường dài.'),
(N'Phạm Thị D', N'Nữ', 35, '0905456789', 7, 550000, '/images/drivers/driver4.jpg', N'Thiện tại đang rảnh cuối tuần.'),
(N'Hoàng Văn E', N'Nam', 27, '0905567890', 3, 450000, '/images/drivers/driver5.jpg', N'Tính tình hòa đồng, lái xe an toàn.'),
(N'Nguyễn Thị F', N'Nữ', 31, '0905678901', 6, 520000, '/images/drivers/driver6.jpg', N'Tài xế riêng cho khách VIP.'),
(N'Đỗ Văn G', N'Nam', 45, '0905789012', 12, 650000, '/images/drivers/driver7.jpg', N'Từng làm tài xế cho công ty du lịch.'),
(N'Bùi Thị H', N'Nữ', 26, '0905890123', 2, 400000, '/images/drivers/driver8.jpg', N'Lái xe cẩn thận, phục vụ chu đáo.'),
(N'Vũ Văn I', N'Nam', 38, '0905901234', 9, 580000, '/images/drivers/driver9.jpg', N'Biết tiếng Anh giao tiếp, chuyên tour.'),
(N'Phan Thị K', N'Nữ', 30, '0906012345', 5, 500000, '/images/drivers/driver10.jpg', N'Tài xế dịch vụ gia đình và sự kiện.');
GO

-- Kiểm tra dữ liệu (tuỳ chọn)
SELECT UserId, Username, Role FROM Users;
SELECT CarId, LicensePlate, CarName, VehicleType FROM Cars;
SELECT * FROM Customers;
SELECT * FROM Drivers;