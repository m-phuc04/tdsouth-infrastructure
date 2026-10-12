# Hệ Thống Chấm Công (TDSouth Attendance System)

Dự án thuộc phòng lab **TDSouth Infrastructure**.

## 1. Giới thiệu
Hệ thống chấm công nhân viên phục vụ ghi nhận thời gian vào ca (Check-in) và ra ca (Check-out) của các thành viên trong lab và doanh nghiệp TDSouth.

## 2. Tính năng chính
- **Đồng hồ thời gian thực (Live Clock):** Cập nhật từng giây, đối soát thời gian vào ca.
- **Thao tác Check-in & Check-out:** Ghi nhận giờ vào, giờ ra theo ca làm việc (Ca Hành Chính, Ca Sáng, Ca Chiều).
- **Tự động phân loại:** Đi đúng giờ hoặc Đi muộn (sau 08:15).
- **Thống kê Dashboard:** Tổng lượt chấm công, số người đúng giờ, đi muộn, và nhân viên đang trong ca làm việc.
- **Tìm kiếm & Xuất dữ liệu:** Lọc nhanh theo tên / mã nhân viên, hỗ trợ in hoặc xuất danh sách.
- **Tích hợp hai chế độ hoạt động:**
  - `index.php`: Chạy trên web server IIS kết nối CSDL Microsoft SQL Server (`TDSouth`).
  - `index.html`: Chạy độc lập trên trình duyệt không cần cấu hình web server, lưu trữ cục bộ qua LocalStorage.

## 3. Cấu hình CSDL SQL Server (Đối với `index.php`)
- **Server:** `localhost\SQLEXPRESS`
- **Database:** `TDSouth`
- **User / Password:** `sa` / `admin@123`
- **Bảng:** `ChamCong` (Tự động khởi tạo khi kết nối lần đầu)

## 4. Thành viên tham gia
- **Lê Minh Phúc** - Project Owner / Admin
- **Kỳ Nguyễn** - Technical Lead
- **Trường Nguyễn** - Developer
- **Tín Phan** - QA / Tester
