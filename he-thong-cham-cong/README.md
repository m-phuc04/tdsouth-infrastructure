# Hệ Thống Chấm Công (TDSouth Attendance System - C# ASP.NET)

Dự án thuộc phòng lab **TDSouth Infrastructure**, được xây dựng bằng ngôn ngữ **C# (.NET / ASP.NET)**.

## 1. Kiến trúc mã nguồn C#
- `Default.aspx` & `Default.aspx.cs`: Giao diện và Code-Behind C# ASP.NET chạy trực tiếp trên **IIS Web Server**.
- `Models/ChamCongModel.cs`: Lớp thực thể `ChamCongRecord`, `NhanVienCollaborator` (theo `COLLABORATORS.md`), và `ChamCongStatistics`.
- `Services/ChamCongService.cs`: Tầng kết nối CSDL Microsoft SQL Server (`System.Data.SqlClient`), tự động khởi tạo bảng `ChamCong` và hỗ trợ chế độ dự phòng In-Memory khi SQL Server chưa bật.
- `Controllers/ChamCongController.cs`: Tầng điều phối nghiệp vụ Check-in, Check-out, Tìm kiếm và kết xuất giao diện.
- `Program.cs` & `ChamCongWeb.csproj`: Máy chủ Web C# độc lập (`HttpListener` trên cổng `8081`).
- `Web.config` & `appsettings.json`: Cấu hình chuỗi kết nối SQL Server và tài liệu mặc định trên IIS.

## 2. Cấu hình CSDL SQL Server
- **Server:** `localhost\SQLEXPRESS`
- **Database:** `TDSouth`
- **User / Password:** `sa` / `admin@123`
- **Bảng:** `ChamCong` (Tự động khởi tạo khi kết nối lần đầu)

## 3. Hướng dẫn triển khai & Chạy ứng dụng
1. **Triển khai trên IIS Web Server:**
   - Trỏ thư mục vật lý của Website / Virtual Directory trên IIS vào thư mục `he-thong-cham-cong`.
   - IIS tự động nhận `Default.aspx` (thông qua `Web.config`) và biên dịch `Default.aspx.cs`.
2. **Chạy trực tiếp bằng trình biên dịch C# (.NET Framework Built-in):**
   ```powershell
   C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /r:System.Web.dll /r:System.Data.dll /out:ChamCongWeb.exe Program.cs Controllers\ChamCongController.cs Services\ChamCongService.cs Models\ChamCongModel.cs
   .\ChamCongWeb.exe
   ```
   Sau đó truy cập `http://localhost:8081/`.

