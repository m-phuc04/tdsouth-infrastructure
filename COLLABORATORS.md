# MA TRẬN PHÂN QUYỀN THÀNH VIÊN DỰ ÁN (PROJECT ACCESS MATRIX)

* **Dự án áp dụng:** Hệ thống chấm công & Hệ thống ERP
* **Repository:** tdsouth-infrastructure
* **Ngày cập nhật:** 12/10/2026

| STT | Họ và Tên | Vai trò dự án (Project Role) | Quyền hạn trên GitHub (Permission Level) | Mô tả trách nhiệm & Phạm vi công việc |
| :---: | :--- | :--- | :---: | :--- |
| 1 | **Lê Minh Phúc** | Project Owner / Admin | **Admin** | Quản trị toàn quyền repository, thiết lập Environments, phê duyệt PR chính vào `main`. |
| 2 | **Kỳ Nguyễn** | Technical Lead | **Maintain** | Quản lý review code, kiểm duyệt nhánh `test`/`staging`, giải quyết conflict mã nguồn. |
| 3 | **Trường Nguyễn** | Developer | **Write** | Lập trình tính năng, đẩy code lên nhánh `feature/*`, tạo Pull Request. |
| 4 | **Tín Phan** | QA / Tester | **Triage / Read** | Kiểm thử phần mềm, ghi nhận Issue lỗi, xác nhận checklist kiểm thử trước khi release. |

### Nguyên tắc bảo mật & Quản lý nhánh:
* Tất cả Pull Request hợp nhất (Merge) vào nhánh `production` (`main`) bắt buộc phải có sự phê duyệt từ **Lê Minh Phúc (Admin)** hoặc **Kỳ Nguyễn (Tech Lead)**.
* Thành viên **Tín Phan (Tester)** chịu trách nhiệm kiểm thử và xác nhận Pass trên môi trường Test/Staging trước khi trigger deployment.
