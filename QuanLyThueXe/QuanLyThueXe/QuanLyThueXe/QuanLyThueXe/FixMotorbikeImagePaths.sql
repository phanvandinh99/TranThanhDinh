-- Script sửa đường dẫn ảnh motorbike từ motorbikes sang motobikes
-- Chạy script này nếu database đã có dữ liệu với đường dẫn sai

USE CarRentalDB;
GO

-- Sửa đường dẫn ảnh cho xe máy
UPDATE Cars
SET ImageUrl = REPLACE(ImageUrl, '/images/motorbikes/', '/images/motobikes/')
WHERE VehicleType = 'Motorbike' 
  AND ImageUrl LIKE '/images/motorbikes/%';
GO

-- Kiểm tra kết quả
SELECT CarId, LicensePlate, Brand, Model, VehicleType, ImageUrl
FROM Cars
WHERE VehicleType = 'Motorbike';
GO

PRINT 'Đã sửa đường dẫn ảnh motorbike thành công!';
GO

