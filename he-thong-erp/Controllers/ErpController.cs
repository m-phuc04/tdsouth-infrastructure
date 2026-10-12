using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text;
using TDSouth.Erp.Models;
using TDSouth.Erp.Services;

namespace TDSouth.Erp.Controllers
{
    /// <summary>
    /// Controller C# điều phối nghiệp vụ Quản trị Nguồn lực Doanh nghiệp (ERP) - Đầy đủ CRUD
    /// </summary>
    public class ErpController
    {
        private readonly ErpService _service;

        public ErpController(ErpService service = null)
        {
            _service = service ?? new ErpService();
        }

        public string HandlePostRequest(NameValueCollection form)
        {
            if (form == null) return string.Empty;

            string action = form["action"] ?? string.Empty;

            // 1. THIẾT BỊ / VẬT TƯ: THÊM (CREATE)
            if (string.Equals(action, "add_asset", StringComparison.OrdinalIgnoreCase) || form["btnThemTB"] != null)
            {
                string maTB = form["matb"] ?? "";
                string tenTB = form["tentb"] ?? "";
                string loaiTB = form["loaitb"] ?? "Máy Chủ (Server)";
                string trangThai = form["trangthai"] ?? "Đang sử dụng";

                int soLuong = 1;
                int.TryParse(form["soluong"], out soLuong);

                long donGia = 0;
                long.TryParse(form["dongia"], out donGia);

                return _service.AddAsset(maTB, tenTB, loaiTB, soLuong, donGia, trangThai);
            }

            // 2. THIẾT BỊ: SỬA (UPDATE)
            if (string.Equals(action, "update_asset", StringComparison.OrdinalIgnoreCase))
            {
                int id;
                int.TryParse(form["id"], out id);
                string maTB = form["matb"] ?? "";
                string tenTB = form["tentb"] ?? "";
                string loaiTB = form["loaitb"] ?? "";
                string trangThai = form["trangthai"] ?? "";

                int soLuong = 1;
                int.TryParse(form["soluong"], out soLuong);

                long donGia = 0;
                long.TryParse(form["dongia"], out donGia);

                return _service.UpdateAsset(id, maTB, tenTB, loaiTB, soLuong, donGia, trangThai);
            }

            // 3. THIẾT BỊ: XÓA (DELETE)
            if (string.Equals(action, "delete_asset", StringComparison.OrdinalIgnoreCase))
            {
                int id;
                int.TryParse(form["id"], out id);
                return _service.DeleteAsset(id);
            }

            // 4. DỰ ÁN: THÊM (CREATE)
            if (string.Equals(action, "add_project", StringComparison.OrdinalIgnoreCase))
            {
                int tienDo = 0;
                int.TryParse(form["tiendo"], out tienDo);

                long kinhPhi = 0;
                long.TryParse(form["kinhphi"], out kinhPhi);

                var p = new DuAnProject
                {
                    MaDuAn = form["maduan"] ?? "",
                    TenDuAn = form["tenduan"] ?? "",
                    TruongNhom = form["truongnhom"] ?? "",
                    TienDo = tienDo,
                    TrangThai = form["trangthai"] ?? "Đang triển khai",
                    KinhPhiDuToan = kinhPhi
                };
                return _service.AddProject(p);
            }

            // 5. DỰ ÁN: SỬA (UPDATE)
            if (string.Equals(action, "update_project", StringComparison.OrdinalIgnoreCase))
            {
                int tienDo = 0;
                int.TryParse(form["tiendo"], out tienDo);

                long kinhPhi = 0;
                long.TryParse(form["kinhphi"], out kinhPhi);

                var p = new DuAnProject
                {
                    MaDuAn = form["maduan"] ?? "",
                    TenDuAn = form["tenduan"] ?? "",
                    TruongNhom = form["truongnhom"] ?? "",
                    TienDo = tienDo,
                    TrangThai = form["trangthai"] ?? "",
                    KinhPhiDuToan = kinhPhi
                };
                return _service.UpdateProject(p);
            }

            // 6. DỰ ÁN: XÓA (DELETE)
            if (string.Equals(action, "delete_project", StringComparison.OrdinalIgnoreCase))
            {
                return _service.DeleteProject(form["maduan"]);
            }

            // 7. THÀNH VIÊN: THÊM (CREATE)
            if (string.Equals(action, "add_role", StringComparison.OrdinalIgnoreCase))
            {
                return _service.AddRole(form["hoten"], form["vaitro"], form["quyen"], form["mota"]);
            }

            // 8. THÀNH VIÊN: SỬA (UPDATE)
            if (string.Equals(action, "update_role", StringComparison.OrdinalIgnoreCase))
            {
                int stt;
                int.TryParse(form["stt"], out stt);
                return _service.UpdateRole(stt, form["hoten"], form["vaitro"], form["quyen"], form["mota"]);
            }

            // 9. THÀNH VIÊN: XÓA (DELETE)
            if (string.Equals(action, "delete_role", StringComparison.OrdinalIgnoreCase))
            {
                int stt;
                int.TryParse(form["stt"], out stt);
                return _service.DeleteRole(stt);
            }

            return string.Empty;
        }

        public static string FormatVnd(long amount)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:N0} ₫", amount).Replace(",", ".");
        }

        public string RenderErpPage(string actionMsg = "", string searchKeyword = "")
        {
            List<ThietBiAsset> assets = _service.GetAssets(searchKeyword);
            List<DuAnProject> projects = _service.GetProjects(searchKeyword);
            List<ThanhVienRole> roles = _service.GetCollaboratorRoles(searchKeyword);
            ErpDashboardSummary summary = ErpDashboardSummary.Calculate(assets, projects, _service.IsDbConnected, _service.DbStatusMessage);

            var sb = new StringBuilder(32768);
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"vi\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("    <title>Hệ Thống ERP C# ASP.NET | TDSouth Infrastructure</title>");
            sb.AppendLine("    <link rel=\"preconnect\" href=\"https://fonts.googleapis.com\">");
            sb.AppendLine("    <link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin>");
            sb.AppendLine("    <link href=\"https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap\" rel=\"stylesheet\">");
            sb.AppendLine("    <style>");
            sb.AppendLine("        :root { --primary: #0f172a; --accent: #2563eb; --accent-hover: #1d4ed8; --sidebar: #1e293b; --bg: #f8fafc; --card: #ffffff; --border: #e2e8f0; --text: #0f172a; --muted: #64748b; --danger: #dc2626; --success: #059669; }");
            sb.AppendLine("        * { box-sizing: border-box; margin: 0; padding: 0; font-family: 'Inter', system-ui, sans-serif; }");
            sb.AppendLine("        body { background: var(--bg); color: var(--text); display: flex; min-height: 100vh; }");
            sb.AppendLine("        .sidebar { width: 270px; background: var(--sidebar); color: #fff; padding: 1.5rem 1rem; display: flex; flex-direction: column; gap: 0.5rem; flex-shrink: 0; }");
            sb.AppendLine("        .brand { padding: 0.5rem 0.75rem 1.25rem; border-bottom: 1px solid #334155; margin-bottom: 0.75rem; }");
            sb.AppendLine("        .brand-title { font-size: 1.15rem; font-weight: 800; color: #60a5fa; }");
            sb.AppendLine("        .brand-sub { font-size: 0.78rem; color: #94a3b8; margin-top: 0.2rem; }");
            sb.AppendLine("        .nav-item { padding: 0.75rem 1rem; border-radius: 8px; cursor: pointer; font-weight: 600; font-size: 0.9rem; color: #cbd5e1; transition: 0.15s; }");
            sb.AppendLine("        .nav-item:hover, .nav-item.active { background: #2563eb; color: #fff; }");
            sb.AppendLine("        .main { flex: 1; padding: 1.75rem 2rem; overflow-x: auto; }");
            sb.AppendLine("        .topbar { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 1rem; margin-bottom: 1.5rem; }");
            sb.AppendLine("        .kpi-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 1.25rem; margin-bottom: 1.75rem; }");
            sb.AppendLine("        .kpi-card { background: var(--card); border: 1px solid var(--border); border-radius: 12px; padding: 1.25rem 1.5rem; }");
            sb.AppendLine("        .kpi-label { font-size: 0.82rem; color: var(--muted); font-weight: 600; }");
            sb.AppendLine("        .kpi-val { font-size: 1.65rem; font-weight: 800; margin-top: 0.35rem; color: #1e40af; }");
            sb.AppendLine("        .card { background: var(--card); border: 1px solid var(--border); border-radius: 12px; padding: 1.5rem; margin-bottom: 1.5rem; box-shadow: 0 2px 6px rgba(0,0,0,0.03); }");
            sb.AppendLine("        .card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; padding-bottom: 0.75rem; border-bottom: 1px solid var(--border); flex-wrap: wrap; gap: 0.5rem; }");
            sb.AppendLine("        .tab-content { display: none; }");
            sb.AppendLine("        .tab-content.active { display: block; }");
            sb.AppendLine("        table { width: 100%; border-collapse: collapse; font-size: 0.88rem; }");
            sb.AppendLine("        th, td { padding: 0.75rem 0.75rem; text-align: left; border-bottom: 1px solid var(--border); }");
            sb.AppendLine("        th { background: #f8fafc; font-size: 0.78rem; text-transform: uppercase; color: #475569; font-weight: 700; }");
            sb.AppendLine("        .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(170px, 1fr)); gap: 0.85rem; align-items: end; }");
            sb.AppendLine("        .form-control { width: 100%; padding: 0.6rem 0.8rem; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 0.9rem; }");
            sb.AppendLine("        .btn { padding: 0.65rem 1.2rem; background: var(--accent); color: #fff; border: none; border-radius: 8px; font-weight: 700; cursor: pointer; transition: 0.2s; display: inline-flex; align-items: center; justify-content: center; gap: 0.35rem; }");
            sb.AppendLine("        .btn:hover { background: var(--accent-hover); }");
            sb.AppendLine("        .btn-sm { padding: 0.35rem 0.65rem; font-size: 0.8rem; border-radius: 6px; }");
            sb.AppendLine("        .btn-outline { background: transparent; border: 1px solid #cbd5e1; color: #334155; }");
            sb.AppendLine("        .btn-danger { background: var(--danger); color: #fff; }");
            sb.AppendLine("        .btn-success { background: var(--success); color: #fff; }");
            sb.AppendLine("        .badge { padding: 0.25rem 0.65rem; border-radius: 999px; font-size: 0.75rem; font-weight: 700; }");
            sb.AppendLine("        .badge-success { background: #dcfce7; color: #15803d; }");
            sb.AppendLine("        .badge-warning { background: #fef3c7; color: #b45309; }");
            sb.AppendLine("        .badge-info { background: #dbeafe; color: #1d4ed8; }");
            sb.AppendLine("        .alert { padding: 0.85rem 1rem; background: #dcfce7; color: #166534; border: 1px solid #86efac; border-radius: 8px; margin-bottom: 1.25rem; font-weight: 600; font-size: 0.92rem; }");
            sb.AppendLine("        .progress-bg { width: 110px; height: 8px; background: #e2e8f0; border-radius: 999px; overflow: hidden; display: inline-block; vertical-align: middle; margin-right: 8px; }");
            sb.AppendLine("        .progress-fill { height: 100%; background: #2563eb; }");
            sb.AppendLine("        .modal { display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(15,23,42,0.6); z-index: 1000; justify-content: center; align-items: center; }");
            sb.AppendLine("        .modal.open { display: flex; }");
            sb.AppendLine("        .modal-card { background: #ffffff; border-radius: 12px; width: 100%; max-width: 520px; padding: 1.5rem; box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1); }");
            sb.AppendLine("        .modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.25rem; padding-bottom: 0.5rem; border-bottom: 1px solid var(--border); }");
            sb.AppendLine("        .modal-close { background: none; border: none; font-size: 1.5rem; cursor: pointer; color: var(--muted); }");
            sb.AppendLine("        .form-group { margin-bottom: 0.85rem; }");
            sb.AppendLine("        .form-group label { display: block; font-size: 0.82rem; font-weight: 600; margin-bottom: 0.3rem; color: #334155; }");
            sb.AppendLine("        .actions-cell { display: flex; gap: 0.35rem; align-items: center; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // SIDEBAR
            sb.AppendLine("<aside class=\"sidebar\">");
            sb.AppendLine("    <div class=\"brand\">");
            sb.AppendLine("        <div class=\"brand-title\">TDSOUTH ERP (C#)</div>");
            sb.AppendLine("        <div class=\"brand-sub\">Enterprise Resource Planning &bull; Full CRUD</div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    <div class=\"nav-item active\" onclick=\"switchTab('tab-overview', this)\">📊 Tổng Quan Điều Hành</div>");
            sb.AppendLine("    <div class=\"nav-item\" onclick=\"switchTab('tab-assets', this)\">🖥️ Vật Tư &amp; Thiết Bị IT (CRUD)</div>");
            sb.AppendLine("    <div class=\"nav-item\" onclick=\"switchTab('tab-projects', this)\">🚀 Dự Án Hạ Tầng Lab (CRUD)</div>");
            sb.AppendLine("    <div class=\"nav-item\" onclick=\"switchTab('tab-hr', this)\">👥 Nhân Sự &amp; Phân Quyền (CRUD)</div>");
            sb.AppendLine("</aside>");

            // MAIN CONTENT
            sb.AppendLine("<main class=\"main\">");
            sb.AppendLine("    <div class=\"topbar\">");
            sb.AppendLine("        <div><h1 style=\"font-size:1.45rem;font-weight:800;\">Hệ Thống Quản Trị Nguồn Lực Doanh Nghiệp (C# ERP)</h1></div>");
            sb.AppendFormat("        <span class=\"badge {0}\">{1}</span>\r\n",
                summary.IsDbConnected ? "badge-success" : "badge-warning",
                WebUtility.HtmlEncode(summary.DbStatusMessage));
            sb.AppendLine("    </div>");

            if (!string.IsNullOrWhiteSpace(actionMsg))
            {
                sb.AppendFormat("    <div class=\"alert\">{0}</div>\r\n", WebUtility.HtmlEncode(actionMsg));
            }

            // SEARCH BAR
            sb.AppendLine("    <form method=\"GET\" action=\"\" style=\"display:flex;gap:0.5rem;margin-bottom:1.5rem;max-width:550px;\">");
            sb.AppendFormat("        <input type=\"text\" name=\"q\" value=\"{0}\" class=\"form-control\" placeholder=\"Tìm kiếm thiết bị, dự án, nhân sự...\">\r\n",
                WebUtility.HtmlEncode(searchKeyword ?? ""));
            sb.AppendLine("        <button type=\"submit\" class=\"btn btn-outline btn-sm\">🔍 Tìm Kiếm</button>");
            sb.AppendLine("    </form>");

            // KPI CARDS
            sb.AppendLine("    <div class=\"kpi-grid\">");
            sb.AppendFormat("        <div class=\"kpi-card\"><div class=\"kpi-label\">Tổng Giá Trị Tài Sản IT</div><div class=\"kpi-val\">{0}</div></div>\r\n", FormatVnd(summary.TotalAssetValue));
            sb.AppendFormat("        <div class=\"kpi-card\"><div class=\"kpi-label\">Tổng Số Lượng Thiết Bị</div><div class=\"kpi-val\">{0} đơn vị ({1} danh mục)</div></div>\r\n", summary.TotalAssetUnits, summary.TotalAssetCategories);
            sb.AppendFormat("        <div class=\"kpi-card\"><div class=\"kpi-label\">Dự Án Hạ Tầng Trọng Điểm</div><div class=\"kpi-val\">{0} dự án</div></div>\r\n", summary.TotalProjects);
            sb.AppendFormat("        <div class=\"kpi-card\"><div class=\"kpi-label\">Cam Kết Uptime SLA</div><div class=\"kpi-val\" style=\"color:#059669;\">{0}</div></div>\r\n", summary.SystemSla);
            sb.AppendLine("    </div>");

            // TAB 1: TỔNG QUAN
            sb.AppendLine("    <div id=\"tab-overview\" class=\"tab-content active\">");
            AppendAssetSection(sb, assets);
            AppendProjectSection(sb, projects);
            AppendHrSection(sb, roles);
            sb.AppendLine("    </div>");

            // TAB 2: THIẾT BỊ
            sb.AppendLine("    <div id=\"tab-assets\" class=\"tab-content\">");
            AppendAssetSection(sb, assets);
            sb.AppendLine("    </div>");

            // TAB 3: DỰ ÁN
            sb.AppendLine("    <div id=\"tab-projects\" class=\"tab-content\">");
            AppendProjectSection(sb, projects);
            sb.AppendLine("    </div>");

            // TAB 4: NHÂN SỰ
            sb.AppendLine("    <div id=\"tab-hr\" class=\"tab-content\">");
            AppendHrSection(sb, roles);
            sb.AppendLine("    </div>");

            sb.AppendLine("</main>");

            // ======================== MODALS ========================

            // MODAL EDIT ASSET
            sb.AppendLine("<div id=\"modalEditAsset\" class=\"modal\">");
            sb.AppendLine("    <div class=\"modal-card\">");
            sb.AppendLine("        <div class=\"modal-header\"><h3>✏️ Chỉnh Sửa Thiết Bị / Vật Tư</h3><button type=\"button\" class=\"modal-close\" onclick=\"closeEditAssetModal()\">&times;</button></div>");
            sb.AppendLine("        <form method=\"POST\" action=\"\">");
            sb.AppendLine("            <input type=\"hidden\" id=\"edit_asset_id\" name=\"id\">");
            sb.AppendLine("            <div class=\"form-group\"><label>Mã Thiết Bị (*)</label><input type=\"text\" id=\"edit_asset_matb\" name=\"matb\" class=\"form-control\" required></div>");
            sb.AppendLine("            <div class=\"form-group\"><label>Tên Thiết Bị (*)</label><input type=\"text\" id=\"edit_asset_tentb\" name=\"tentb\" class=\"form-control\" required></div>");
            sb.AppendLine("            <div class=\"form-group\"><label>Phân Loại</label><select id=\"edit_asset_loaitb\" name=\"loaitb\" class=\"form-control\"><option>Máy Chủ (Server)</option><option>Thiết Bị Mạng (Switch)</option><option>Bảo Mật (Firewall)</option><option>Lưu Trữ (Storage)</option><option>Bản Quyền (License)</option><option>Vật Tư Phụ</option></select></div>");
            sb.AppendLine("            <div style=\"display:grid;grid-template-columns:1fr 1fr;gap:0.75rem;\">");
            sb.AppendLine("                <div class=\"form-group\"><label>Số Lượng</label><input type=\"number\" id=\"edit_asset_soluong\" name=\"soluong\" min=\"1\" class=\"form-control\" required></div>");
            sb.AppendLine("                <div class=\"form-group\"><label>Đơn Giá (VNĐ)</label><input type=\"number\" id=\"edit_asset_dongia\" name=\"dongia\" min=\"0\" class=\"form-control\" required></div>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div class=\"form-group\"><label>Trạng Thái</label><select id=\"edit_asset_trangthai\" name=\"trangthai\" class=\"form-control\"><option>Đang sử dụng</option><option>Đang bảo trì</option><option>Lưu kho dự phòng</option><option>Thanh lý</option></select></div>");
            sb.AppendLine("            <button type=\"submit\" name=\"action\" value=\"update_asset\" class=\"btn\" style=\"width:100%;margin-top:0.5rem;\">💾 Lưu Cập Nhật Thiết Bị</button>");
            sb.AppendLine("        </form>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</div>");

            // MODAL EDIT PROJECT
            sb.AppendLine("<div id=\"modalEditProject\" class=\"modal\">");
            sb.AppendLine("    <div class=\"modal-card\">");
            sb.AppendLine("        <div class=\"modal-header\"><h3>✏️ Chỉnh Sửa Dự Án Hạ Tầng</h3><button type=\"button\" class=\"modal-close\" onclick=\"closeEditProjModal()\">&times;</button></div>");
            sb.AppendLine("        <form method=\"POST\" action=\"\">");
            sb.AppendLine("            <div class=\"form-group\"><label>Mã Dự Án (Khóa chính)</label><input type=\"text\" id=\"edit_proj_maduan\" name=\"maduan\" class=\"form-control\" readonly style=\"background:#f1f5f9;\"></div>");
            sb.AppendLine("            <div class=\"form-group\"><label>Tên Dự Án (*)</label><input type=\"text\" id=\"edit_proj_tenduan\" name=\"tenduan\" class=\"form-control\" required></div>");
            sb.AppendLine("            <div class=\"form-group\"><label>Trưởng Nhóm / Phụ Trách</label><input type=\"text\" id=\"edit_proj_truongnhom\" name=\"truongnhom\" class=\"form-control\" required></div>");
            sb.AppendLine("            <div style=\"display:grid;grid-template-columns:1fr 1fr;gap:0.75rem;\">");
            sb.AppendLine("                <div class=\"form-group\"><label>Tiến Độ (%)</label><input type=\"number\" id=\"edit_proj_tiendo\" name=\"tiendo\" min=\"0\" max=\"100\" class=\"form-control\" required></div>");
            sb.AppendLine("                <div class=\"form-group\"><label>Kinh Phí Dự Toán (VNĐ)</label><input type=\"number\" id=\"edit_proj_kinhphi\" name=\"kinhphi\" min=\"0\" class=\"form-control\" required></div>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div class=\"form-group\"><label>Trạng Thái</label><select id=\"edit_proj_trangthai\" name=\"trangthai\" class=\"form-control\"><option>Đang triển khai</option><option>Đang hoàn thiện</option><option>Đang kiểm thử</option><option>Hoàn thành</option><option>Tạm dừng</option></select></div>");
            sb.AppendLine("            <button type=\"submit\" name=\"action\" value=\"update_project\" class=\"btn\" style=\"width:100%;margin-top:0.5rem;\">💾 Lưu Cập Nhật Dự Án</button>");
            sb.AppendLine("        </form>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</div>");

            // MODAL EDIT ROLE
            sb.AppendLine("<div id=\"modalEditRole\" class=\"modal\">");
            sb.AppendLine("    <div class=\"modal-card\">");
            sb.AppendLine("        <div class=\"modal-header\"><h3>✏️ Chỉnh Sửa Nhân Sự &amp; Phân Quyền</h3><button type=\"button\" class=\"modal-close\" onclick=\"closeEditRoleModal()\">&times;</button></div>");
            sb.AppendLine("        <form method=\"POST\" action=\"\">");
            sb.AppendLine("            <input type=\"hidden\" id=\"edit_role_stt\" name=\"stt\">");
            sb.AppendLine("            <div class=\"form-group\"><label>Họ và Tên (*)</label><input type=\"text\" id=\"edit_role_hoten\" name=\"hoten\" class=\"form-control\" required></div>");
            sb.AppendLine("            <div class=\"form-group\"><label>Vai Trò Dự Án (*)</label><input type=\"text\" id=\"edit_role_vaitro\" name=\"vaitro\" class=\"form-control\" required></div>");
            sb.AppendLine("            <div class=\"form-group\"><label>Quyền Hạn GitHub / Hệ Thống</label><select id=\"edit_role_quyen\" name=\"quyen\" class=\"form-control\"><option value=\"Admin\">Admin</option><option value=\"Maintain\">Maintain</option><option value=\"Write\">Write</option><option value=\"Triage / Read\">Triage / Read</option></select></div>");
            sb.AppendLine("            <div class=\"form-group\"><label>Mô Tả Trách Nhiệm</label><textarea id=\"edit_role_mota\" name=\"mota\" class=\"form-control\" rows=\"3\"></textarea></div>");
            sb.AppendLine("            <button type=\"submit\" name=\"action\" value=\"update_role\" class=\"btn\" style=\"width:100%;margin-top:0.5rem;\">💾 Lưu Phân Quyền</button>");
            sb.AppendLine("        </form>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</div>");

            // SCRIPTS
            sb.AppendLine("<script>");
            sb.AppendLine("    function switchTab(id, el) {");
            sb.AppendLine("        document.querySelectorAll('.tab-content').forEach(function(t){ t.classList.remove('active'); });");
            sb.AppendLine("        document.querySelectorAll('.nav-item').forEach(function(n){ n.classList.remove('active'); });");
            sb.AppendLine("        document.getElementById(id).classList.add('active');");
            sb.AppendLine("        if (el) el.classList.add('active');");
            sb.AppendLine("    }");
            sb.AppendLine("    function openEditAssetModal(id, matb, tentb, loaitb, soluong, dongia, status) {");
            sb.AppendLine("        document.getElementById('edit_asset_id').value = id;");
            sb.AppendLine("        document.getElementById('edit_asset_matb').value = matb;");
            sb.AppendLine("        document.getElementById('edit_asset_tentb').value = tentb;");
            sb.AppendLine("        document.getElementById('edit_asset_loaitb').value = loaitb;");
            sb.AppendLine("        document.getElementById('edit_asset_soluong').value = soluong;");
            sb.AppendLine("        document.getElementById('edit_asset_dongia').value = dongia;");
            sb.AppendLine("        document.getElementById('edit_asset_trangthai').value = status;");
            sb.AppendLine("        document.getElementById('modalEditAsset').classList.add('open');");
            sb.AppendLine("    }");
            sb.AppendLine("    function closeEditAssetModal() { document.getElementById('modalEditAsset').classList.remove('open'); }");
            sb.AppendLine("    function openEditProjModal(maduan, tenduan, truongnhom, tiendo, trangthai, kinhphi) {");
            sb.AppendLine("        document.getElementById('edit_proj_maduan').value = maduan;");
            sb.AppendLine("        document.getElementById('edit_proj_tenduan').value = tenduan;");
            sb.AppendLine("        document.getElementById('edit_proj_truongnhom').value = truongnhom;");
            sb.AppendLine("        document.getElementById('edit_proj_tiendo').value = tiendo;");
            sb.AppendLine("        document.getElementById('edit_proj_trangthai').value = trangthai;");
            sb.AppendLine("        document.getElementById('edit_proj_kinhphi').value = kinhphi;");
            sb.AppendLine("        document.getElementById('modalEditProject').classList.add('open');");
            sb.AppendLine("    }");
            sb.AppendLine("    function closeEditProjModal() { document.getElementById('modalEditProject').classList.remove('open'); }");
            sb.AppendLine("    function openEditRoleModal(stt, hoten, vaitro, quyen, mota) {");
            sb.AppendLine("        document.getElementById('edit_role_stt').value = stt;");
            sb.AppendLine("        document.getElementById('edit_role_hoten').value = hoten;");
            sb.AppendLine("        document.getElementById('edit_role_vaitro').value = vaitro;");
            sb.AppendLine("        document.getElementById('edit_role_quyen').value = quyen;");
            sb.AppendLine("        document.getElementById('edit_role_mota').value = mota;");
            sb.AppendLine("        document.getElementById('modalEditRole').classList.add('open');");
            sb.AppendLine("    }");
            sb.AppendLine("    function closeEditRoleModal() { document.getElementById('modalEditRole').classList.remove('open'); }");
            sb.AppendLine("</script>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        private static void AppendAssetSection(StringBuilder sb, List<ThietBiAsset> assets)
        {
            sb.AppendLine("        <div class=\"card\">");
            sb.AppendLine("            <div class=\"card-header\"><h3>➕ Nhập Kho Thiết Bị / Vật Tư Hạ Tầng Mới (Create)</h3></div>");
            sb.AppendLine("            <form method=\"POST\" action=\"\" class=\"form-grid\">");
            sb.AppendLine("                <input type=\"hidden\" name=\"action\" value=\"add_asset\">");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Mã Thiết Bị (*)</label><input type=\"text\" name=\"matb\" class=\"form-control\" placeholder=\"VD: SRV-02\" required></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Tên Thiết Bị (*)</label><input type=\"text\" name=\"tentb\" class=\"form-control\" placeholder=\"VD: HP ProLiant DL380\" required></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Phân Loại</label><select name=\"loaitb\" class=\"form-control\"><option>Máy Chủ (Server)</option><option>Thiết Bị Mạng (Switch)</option><option>Bảo Mật (Firewall)</option><option>Lưu Trữ (Storage)</option><option>Bản Quyền (License)</option><option>Vật Tư Phụ</option></select></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Số Lượng</label><input type=\"number\" name=\"soluong\" value=\"1\" min=\"1\" class=\"form-control\"></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Đơn Giá (VNĐ)</label><input type=\"number\" name=\"dongia\" value=\"25000000\" min=\"0\" class=\"form-control\"></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Trạng Thái</label><select name=\"trangthai\" class=\"form-control\"><option>Đang sử dụng</option><option>Đang bảo trì</option><option>Lưu kho dự phòng</option></select></div>");
            sb.AppendLine("                <div><button type=\"submit\" class=\"btn\">+ Lưu Thiết Bị</button></div>");
            sb.AppendLine("            </form>");
            sb.AppendLine("        </div>");

            sb.AppendLine("        <div class=\"card\">");
            sb.AppendLine("            <div class=\"card-header\"><h3>🖥️ Danh Mục Tài Sản &amp; Vật Tư CNTT (" + assets.Count + " thiết bị)</h3></div>");
            sb.AppendLine("            <div style=\"overflow-x:auto;\">");
            sb.AppendLine("                <table><thead><tr><th>Mã TB</th><th>Tên Thiết Bị / Vật Tư</th><th>Phân Loại</th><th>SL</th><th>Đơn Giá</th><th>Thành Tiền</th><th>Trạng Thái</th><th style=\"text-align:center;\">Thao Tác</th></tr></thead><tbody>");
            foreach (var a in assets)
            {
                string badge = a.TrangThai == "Đang sử dụng" ? "badge-success" : "badge-warning";
                sb.AppendLine("                <tr>");
                sb.AppendFormat("                    <td><strong>{0}</strong></td>\r\n", WebUtility.HtmlEncode(a.MaTB));
                sb.AppendFormat("                    <td>{0}</td>\r\n", WebUtility.HtmlEncode(a.TenTB));
                sb.AppendFormat("                    <td><span class=\"badge badge-info\">{0}</span></td>\r\n", WebUtility.HtmlEncode(a.LoaiTB));
                sb.AppendFormat("                    <td><strong>{0}</strong></td>\r\n", a.SoLuong);
                sb.AppendFormat("                    <td>{0}</td>\r\n", FormatVnd(a.DonGia));
                sb.AppendFormat("                    <td style=\"font-weight:700;color:#1e40af;\">{0}</td>\r\n", FormatVnd(a.ThanhTien));
                sb.AppendFormat("                    <td><span class=\"badge {0}\">{1}</span></td>\r\n", badge, WebUtility.HtmlEncode(a.TrangThai));
                sb.AppendLine("                    <td><div class=\"actions-cell\">");
                sb.AppendFormat("                        <button type=\"button\" class=\"btn btn-outline btn-sm\" onclick=\"openEditAssetModal({0},'{1}','{2}','{3}',{4},{5},'{6}')\">✏️ Sửa</button>\r\n",
                    a.ID,
                    WebUtility.HtmlEncode(a.MaTB).Replace("'", "\\'"),
                    WebUtility.HtmlEncode(a.TenTB).Replace("'", "\\'"),
                    WebUtility.HtmlEncode(a.LoaiTB).Replace("'", "\\'"),
                    a.SoLuong,
                    a.DonGia,
                    WebUtility.HtmlEncode(a.TrangThai).Replace("'", "\\'"));
                sb.AppendFormat("                        <form method=\"POST\" action=\"\" onsubmit=\"return confirm('Bạn có chắc chắn muốn xóa thiết bị [{0}]?');\" style=\"display:inline;\">\r\n", WebUtility.HtmlEncode(a.MaTB));
                sb.AppendFormat("                            <input type=\"hidden\" name=\"id\" value=\"{0}\">\r\n", a.ID);
                sb.AppendLine("                            <button type=\"submit\" name=\"action\" value=\"delete_asset\" class=\"btn btn-danger btn-sm\">🗑️ Xóa</button>");
                sb.AppendLine("                        </form>");
                sb.AppendLine("                    </div></td>");
                sb.AppendLine("                </tr>");
            }
            if (assets.Count == 0)
            {
                sb.AppendLine("                <tr><td colspan=\"8\" style=\"text-align:center;color:var(--muted);padding:1.5rem;\">Chưa có thiết bị nào trong danh mục.</td></tr>");
            }
            sb.AppendLine("                </tbody></table>");
            sb.AppendLine("            </div>");
            sb.AppendLine("        </div>");
        }

        private static void AppendProjectSection(StringBuilder sb, List<DuAnProject> projects)
        {
            sb.AppendLine("        <div class=\"card\">");
            sb.AppendLine("            <div class=\"card-header\"><h3>➕ Khởi Tạo Dự Án Hạ Tầng Mới (Create)</h3></div>");
            sb.AppendLine("            <form method=\"POST\" action=\"\" class=\"form-grid\">");
            sb.AppendLine("                <input type=\"hidden\" name=\"action\" value=\"add_project\">");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Mã Dự Án (*)</label><input type=\"text\" name=\"maduan\" class=\"form-control\" placeholder=\"VD: PRJ-VM05\" required></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Tên Dự Án (*)</label><input type=\"text\" name=\"tenduan\" class=\"form-control\" placeholder=\"VD: Cụm ảo hóa VMware ESXi\" required></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Trưởng Nhóm</label><input type=\"text\" name=\"truongnhom\" class=\"form-control\" placeholder=\"VD: Nguyễn Văn A\" required></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Tiến Độ (%)</label><input type=\"number\" name=\"tiendo\" value=\"10\" min=\"0\" max=\"100\" class=\"form-control\"></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Kinh Phí (VNĐ)</label><input type=\"number\" name=\"kinhphi\" value=\"35000000\" min=\"0\" class=\"form-control\"></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Trạng Thái</label><select name=\"trangthai\" class=\"form-control\"><option>Đang triển khai</option><option>Đang hoàn thiện</option><option>Đang kiểm thử</option><option>Hoàn thành</option></select></div>");
            sb.AppendLine("                <div><button type=\"submit\" class=\"btn\">+ Tạo Dự Án</button></div>");
            sb.AppendLine("            </form>");
            sb.AppendLine("        </div>");

            sb.AppendLine("        <div class=\"card\">");
            sb.AppendLine("            <div class=\"card-header\"><h3>🚀 Tiến Độ Dự Án Hạ Tầng TDSouth (" + projects.Count + " dự án)</h3></div>");
            sb.AppendLine("            <div style=\"overflow-x:auto;\">");
            sb.AppendLine("                <table><thead><tr><th>Mã DA</th><th>Tên Dự Án Hạ Tầng</th><th>Phụ Trách</th><th>Kinh Phí Dự Toán</th><th>Tiến Độ</th><th>Trạng Thái</th><th style=\"text-align:center;\">Thao Tác</th></tr></thead><tbody>");
            foreach (var p in projects)
            {
                string badge = p.TienDo == 100 ? "badge-success" : "badge-info";
                sb.AppendLine("                <tr>");
                sb.AppendFormat("                    <td><strong>{0}</strong></td>\r\n", WebUtility.HtmlEncode(p.MaDuAn));
                sb.AppendFormat("                    <td><strong>{0}</strong></td>\r\n", WebUtility.HtmlEncode(p.TenDuAn));
                sb.AppendFormat("                    <td>{0}</td>\r\n", WebUtility.HtmlEncode(p.TruongNhom));
                sb.AppendFormat("                    <td>{0}</td>\r\n", FormatVnd(p.KinhPhiDuToan));
                sb.AppendFormat("                    <td><div class=\"progress-bg\"><div class=\"progress-fill\" style=\"width:{0}%;\"></div></div><strong>{0}%</strong></td>\r\n", p.TienDo);
                sb.AppendFormat("                    <td><span class=\"badge {0}\">{1}</span></td>\r\n", badge, WebUtility.HtmlEncode(p.TrangThai));
                sb.AppendLine("                    <td><div class=\"actions-cell\">");
                sb.AppendFormat("                        <button type=\"button\" class=\"btn btn-outline btn-sm\" onclick=\"openEditProjModal('{0}','{1}','{2}',{3},'{4}',{5})\">✏️ Sửa</button>\r\n",
                    WebUtility.HtmlEncode(p.MaDuAn).Replace("'", "\\'"),
                    WebUtility.HtmlEncode(p.TenDuAn).Replace("'", "\\'"),
                    WebUtility.HtmlEncode(p.TruongNhom).Replace("'", "\\'"),
                    p.TienDo,
                    WebUtility.HtmlEncode(p.TrangThai).Replace("'", "\\'"),
                    p.KinhPhiDuToan);
                sb.AppendFormat("                        <form method=\"POST\" action=\"\" onsubmit=\"return confirm('Bạn có chắc chắn muốn xóa dự án [{0}]?');\" style=\"display:inline;\">\r\n", WebUtility.HtmlEncode(p.MaDuAn));
                sb.AppendFormat("                            <input type=\"hidden\" name=\"maduan\" value=\"{0}\">\r\n", WebUtility.HtmlEncode(p.MaDuAn));
                sb.AppendLine("                            <button type=\"submit\" name=\"action\" value=\"delete_project\" class=\"btn btn-danger btn-sm\">🗑️ Xóa</button>");
                sb.AppendLine("                        </form>");
                sb.AppendLine("                    </div></td>");
                sb.AppendLine("                </tr>");
            }
            if (projects.Count == 0)
            {
                sb.AppendLine("                <tr><td colspan=\"7\" style=\"text-align:center;color:var(--muted);padding:1.5rem;\">Chưa có dự án nào trong danh mục.</td></tr>");
            }
            sb.AppendLine("                </tbody></table>");
            sb.AppendLine("            </div>");
            sb.AppendLine("        </div>");
        }

        private static void AppendHrSection(StringBuilder sb, List<ThanhVienRole> roles)
        {
            sb.AppendLine("        <div class=\"card\">");
            sb.AppendLine("            <div class=\"card-header\"><h3>➕ Thêm Thành Viên &amp; Phân Quyền Mới (Create)</h3></div>");
            sb.AppendLine("            <form method=\"POST\" action=\"\" class=\"form-grid\">");
            sb.AppendLine("                <input type=\"hidden\" name=\"action\" value=\"add_role\">");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Họ và Tên (*)</label><input type=\"text\" name=\"hoten\" class=\"form-control\" placeholder=\"VD: Nguyễn Văn B\" required></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Vai Trò Dự Án (*)</label><input type=\"text\" name=\"vaitro\" class=\"form-control\" placeholder=\"VD: Network Admin\" required></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Quyền GitHub</label><select name=\"quyen\" class=\"form-control\"><option value=\"Admin\">Admin</option><option value=\"Maintain\">Maintain</option><option value=\"Write\" selected>Write</option><option value=\"Triage / Read\">Triage / Read</option></select></div>");
            sb.AppendLine("                <div style=\"grid-column:span 2;\"><label style=\"font-size:0.8rem;font-weight:600;\">Mô Tả Trách Nhiệm</label><input type=\"text\" name=\"mota\" class=\"form-control\" placeholder=\"VD: Phụ trách cấu hình Switch/Router phòng lab\"></div>");
            sb.AppendLine("                <div><button type=\"submit\" class=\"btn\">+ Thêm Thành Viên</button></div>");
            sb.AppendLine("            </form>");
            sb.AppendLine("        </div>");

            sb.AppendLine("        <div class=\"card\">");
            sb.AppendLine("            <div class=\"card-header\"><h3>👥 Ma Trận Phân Quyền Thành Viên Dự Án (COLLABORATORS.md)</h3></div>");
            sb.AppendLine("            <div style=\"overflow-x:auto;\">");
            sb.AppendLine("                <table><thead><tr><th>STT</th><th>Họ và Tên</th><th>Vai Trò Dự Án</th><th>Quyền GitHub</th><th>Mô Tả Trách Nhiệm</th><th style=\"text-align:center;\">Thao Tác</th></tr></thead><tbody>");
            foreach (var r in roles)
            {
                sb.AppendLine("                <tr>");
                sb.AppendFormat("                    <td>#{0}</td>\r\n", r.STT);
                sb.AppendFormat("                    <td><strong>{0}</strong></td>\r\n", WebUtility.HtmlEncode(r.HoTen));
                sb.AppendFormat("                    <td><span class=\"badge badge-info\">{0}</span></td>\r\n", WebUtility.HtmlEncode(r.VaiTroDuAn));
                sb.AppendFormat("                    <td><strong>{0}</strong></td>\r\n", WebUtility.HtmlEncode(r.QuyenGitHub));
                sb.AppendFormat("                    <td>{0}</td>\r\n", WebUtility.HtmlEncode(r.MoTaTrachNhiem));
                sb.AppendLine("                    <td><div class=\"actions-cell\">");
                sb.AppendFormat("                        <button type=\"button\" class=\"btn btn-outline btn-sm\" onclick=\"openEditRoleModal({0},'{1}','{2}','{3}','{4}')\">✏️ Sửa</button>\r\n",
                    r.STT,
                    WebUtility.HtmlEncode(r.HoTen).Replace("'", "\\'"),
                    WebUtility.HtmlEncode(r.VaiTroDuAn).Replace("'", "\\'"),
                    WebUtility.HtmlEncode(r.QuyenGitHub).Replace("'", "\\'"),
                    WebUtility.HtmlEncode(r.MoTaTrachNhiem).Replace("'", "\\'"));
                sb.AppendFormat("                        <form method=\"POST\" action=\"\" onsubmit=\"return confirm('Bạn có chắc chắn muốn xóa thành viên #{0}?');\" style=\"display:inline;\">\r\n", r.STT);
                sb.AppendFormat("                            <input type=\"hidden\" name=\"stt\" value=\"{0}\">\r\n", r.STT);
                sb.AppendLine("                            <button type=\"submit\" name=\"action\" value=\"delete_role\" class=\"btn btn-danger btn-sm\">🗑️ Xóa</button>");
                sb.AppendLine("                        </form>");
                sb.AppendLine("                    </div></td>");
                sb.AppendLine("                </tr>");
            }
            if (roles.Count == 0)
            {
                sb.AppendLine("                <tr><td colspan=\"6\" style=\"text-align:center;color:var(--muted);padding:1.5rem;\">Chưa có thành viên nào.</td></tr>");
            }
            sb.AppendLine("                </tbody></table>");
            sb.AppendLine("            </div>");
            sb.AppendLine("        </div>");
        }
    }
}
