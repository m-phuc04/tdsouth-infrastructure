# Hệ Thống ERP (TDSouth Enterprise Resource Planning)

Dự án thuộc phòng lab **TDSouth Infrastructure**.

## 1. Giới thiệu
Hệ thống ERP quản trị nguồn lực và tài sản công nghệ thông tin cho doanh nghiệp TDSouth, bao gồm theo dõi vật tư hạ tầng, quản lý dự án mạng & máy chủ, cũng như ma trận phân quyền thành viên dự án.

## 2. Các phân hệ chính
1. **Tổng quan điều hành (Overview Dashboard):**
   - KPI tài sản IT, ngân sách dự toán, số lượng dự án, tỉ lệ SLA hệ thống đạt 99.98%.
2. **Quản lý Vật tư & Thiết bị IT (Asset & Inventory):**
   - Danh mục Server, Switch, Firewall, NAS Storage, Cáp mạng, License.
   - Form nhập mới thiết bị, tính toán tổng ngân sách tài sản.
3. **Quản lý Dự án Hạ tầng (Projects):**
   - Theo dõi tiến độ triển khai Active Directory, DNS/DHCP Failover, Web Server IIS SSL, FTPS & File Server.
4. **Nhân sự & Phân quyền (HR & Roles):**
   - Tích hợp ma trận phân quyền từ `COLLABORATORS.md`.

## 3. Chế độ triển khai
- **Web Server IIS + PHP:** File `index.php` kết nối CSDL Microsoft SQL Server (`TDSouth`), bảng `ERP_ThietBi`.
- **Trình duyệt Web trực tiếp:** File `index.html` chạy mượt mà không cần web server, hỗ trợ đầy đủ thao tác thêm/xóa/lưu dữ liệu qua LocalStorage.

## 4. Cấu hình CSDL SQL Server (Đối với `index.php`)
- **Server:** `localhost\SQLEXPRESS`
- **Database:** `TDSouth`
- **User / Password:** `sa` / `admin@123`
- **Bảng:** `ERP_ThietBi` (Tự động khởi tạo khi kết nối lần đầu)

## 5. Thành viên tham gia
- **Lê Minh Phúc** - Project Owner / Admin
- **Kỳ Nguyễn** - Technical Lead
- **Trường Nguyễn** - Developer
- **Tín Phan** - QA / Tester
