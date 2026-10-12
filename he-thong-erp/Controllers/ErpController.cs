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
    /// Controller C# điều phối nghiệp vụ Quản trị Nguồn lực Doanh nghiệp (ERP) - TDSouth Infrastructure
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

            if (form["btnThemTB"] != null || string.Equals(form["action"], "add_asset", StringComparison.OrdinalIgnoreCase))
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

            return string.Empty;
        }

        public static string FormatVnd(long amount)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:N0} ₫", amount).Replace(",", ".");
        }

        public string RenderErpPage(string actionMsg = "", string searchKeyword = "")
        {
            List<ThietBiAsset> assets = _service.GetAssets(searchKeyword);
            List<DuAnProject> projects = _service.GetProjects();
            List<ThanhVienRole> roles = _service.GetCollaboratorRoles();
            ErpDashboardSummary summary = ErpDashboardSummary.Calculate(assets, projects, _service.IsDbConnected, _service.DbStatusMessage);

            var sb = new StringBuilder(24576);
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
            sb.AppendLine("        :root { --primary: #0f172a; --accent: #2563eb; --sidebar: #1e293b; --bg: #f1f5f9; --card: #ffffff; --border: #e2e8f0; --text: #0f172a; --muted: #64748b; }");
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
            sb.AppendLine("        .card { background: var(--card); border: 1px solid var(--border); border-radius: 12px; padding: 1.5rem; margin-bottom: 1.5rem; }");
            sb.AppendLine("        .card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; padding-bottom: 0.75rem; border-bottom: 1px solid var(--border); }");
            sb.AppendLine("        .tab-content { display: none; }");
            sb.AppendLine("        .tab-content.active { display: block; }");
            sb.AppendLine("        table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }");
            sb.AppendLine("        th, td { padding: 0.85rem 0.75rem; text-align: left; border-bottom: 1px solid var(--border); }");
            sb.AppendLine("        th { background: #f8fafc; font-size: 0.78rem; text-transform: uppercase; color: #475569; }");
            sb.AppendLine("        .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 1rem; align-items: end; }");
            sb.AppendLine("        .form-control { width: 100%; padding: 0.6rem 0.8rem; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 0.9rem; }");
            sb.AppendLine("        .btn { padding: 0.65rem 1.2rem; background: #2563eb; color: #fff; border: none; border-radius: 8px; font-weight: 700; cursor: pointer; }");
            sb.AppendLine("        .badge { padding: 0.25rem 0.65rem; border-radius: 999px; font-size: 0.75rem; font-weight: 700; }");
            sb.AppendLine("        .badge-success { background: #dcfce7; color: #15803d; }");
            sb.AppendLine("        .badge-warning { background: #fef3c7; color: #b45309; }");
            sb.AppendLine("        .badge-info { background: #dbeafe; color: #1d4ed8; }");
            sb.AppendLine("        .alert { padding: 0.85rem 1rem; background: #dcfce7; color: #166534; border: 1px solid #86efac; border-radius: 8px; margin-bottom: 1.25rem; font-weight: 600; }");
            sb.AppendLine("        .progress-bg { width: 130px; height: 8px; background: #e2e8f0; border-radius: 999px; overflow: hidden; display: inline-block; vertical-align: middle; margin-right: 8px; }");
            sb.AppendLine("        .progress-fill { height: 100%; background: #2563eb; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<aside class=\"sidebar\">");
            sb.AppendLine("    <div class=\"brand\">");
            sb.AppendLine("        <div class=\"brand-title\">TDSOUTH ERP (C#)</div>");
            sb.AppendLine("        <div class=\"brand-sub\">Enterprise Resource Planning &bull; ASP.NET</div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    <div class=\"nav-item active\" onclick=\"switchTab('tab-overview', this)\">📊 Tổng Quan Điều Hành</div>");
            sb.AppendLine("    <div class=\"nav-item\" onclick=\"switchTab('tab-assets', this)\">🖥️ Vật Tư &amp; Thiết Bị IT</div>");
            sb.AppendLine("    <div class=\"nav-item\" onclick=\"switchTab('tab-projects', this)\">🚀 Dự Án Hạ Tầng Lab</div>");
            sb.AppendLine("    <div class=\"nav-item\" onclick=\"switchTab('tab-hr', this)\">👥 Nhân Sự &amp; Phân Quyền</div>");
            sb.AppendLine("</aside>");
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

            sb.AppendLine("    <div class=\"kpi-grid\">");
            sb.AppendFormat("        <div class=\"kpi-card\"><div class=\"kpi-label\">Tổng Giá Trị Tài Sản IT</div><div class=\"kpi-val\">{0}</div></div>\r\n", FormatVnd(summary.TotalAssetValue));
            sb.AppendFormat("        <div class=\"kpi-card\"><div class=\"kpi-label\">Tổng Số Lượng Thiết Bị</div><div class=\"kpi-val\">{0} đơn vị ({1} danh mục)</div></div>\r\n", summary.TotalAssetUnits, summary.TotalAssetCategories);
            sb.AppendFormat("        <div class=\"kpi-card\"><div class=\"kpi-label\">Dự Án Hạ Tầng Trọng Điểm</div><div class=\"kpi-val\">{0} dự án</div></div>\r\n", summary.TotalProjects);
            sb.AppendFormat("        <div class=\"kpi-card\"><div class=\"kpi-label\">Cam Kết Uptime SLA</div><div class=\"kpi-val\" style=\"color:#059669;\">{0}</div></div>\r\n", summary.SystemSla);
            sb.AppendLine("    </div>");

            // TAB 1 & TAB 2
            sb.AppendLine("    <div id=\"tab-overview\" class=\"tab-content active\">");
            AppendAssetSection(sb, assets);
            AppendProjectSection(sb, projects);
            sb.AppendLine("    </div>");

            sb.AppendLine("    <div id=\"tab-assets\" class=\"tab-content\">");
            AppendAssetSection(sb, assets);
            sb.AppendLine("    </div>");

            sb.AppendLine("    <div id=\"tab-projects\" class=\"tab-content\">");
            AppendProjectSection(sb, projects);
            sb.AppendLine("    </div>");

            sb.AppendLine("    <div id=\"tab-hr\" class=\"tab-content\">");
            sb.AppendLine("        <div class=\"card\">");
            sb.AppendLine("            <div class=\"card-header\"><h3>👥 Ma Trận Phân Quyền Thành Viên Dự Án (COLLABORATORS.md)</h3></div>");
            sb.AppendLine("            <table><thead><tr><th>STT</th><th>Họ và Tên</th><th>Vai Trò Dự Án</th><th>Quyền GitHub</th><th>Mô Tả Trách Nhiệm</th></tr></thead><tbody>");
            foreach (var r in roles)
            {
                sb.AppendFormat("                <tr><td>{0}</td><td><strong>{1}</strong></td><td><span class=\"badge badge-info\">{2}</span></td><td><strong>{3}</strong></td><td>{4}</td></tr>\r\n",
                    r.STT, WebUtility.HtmlEncode(r.HoTen), WebUtility.HtmlEncode(r.VaiTroDuAn), WebUtility.HtmlEncode(r.QuyenGitHub), WebUtility.HtmlEncode(r.MoTaTrachNhiem));
            }
            sb.AppendLine("            </tbody></table>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</main>");
            sb.AppendLine("<script>");
            sb.AppendLine("    function switchTab(id, el) {");
            sb.AppendLine("        document.querySelectorAll('.tab-content').forEach(function(t){ t.classList.remove('active'); });");
            sb.AppendLine("        document.querySelectorAll('.nav-item').forEach(function(n){ n.classList.remove('active'); });");
            sb.AppendLine("        document.getElementById(id).classList.add('active');");
            sb.AppendLine("        if (el) el.classList.add('active');");
            sb.AppendLine("    }");
            sb.AppendLine("</script>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        private static void AppendAssetSection(StringBuilder sb, List<ThietBiAsset> assets)
        {
            sb.AppendLine("        <div class=\"card\">");
            sb.AppendLine("            <div class=\"card-header\"><h3>➕ Nhập Kho Thiết Bị / Vật Tư Hạ Tầng Mới (C# SQL Server)</h3></div>");
            sb.AppendLine("            <form method=\"POST\" action=\"\" class=\"form-grid\">");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Mã Thiết Bị (*)</label><input type=\"text\" name=\"matb\" class=\"form-control\" placeholder=\"VD: SRV-02\" required></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Tên Thiết Bị (*)</label><input type=\"text\" name=\"tentb\" class=\"form-control\" placeholder=\"VD: HP ProLiant DL380 Gen10\" required></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Phân Loại</label><select name=\"loaitb\" class=\"form-control\"><option>Máy Chủ (Server)</option><option>Thiết Bị Mạng (Switch)</option><option>Bảo Mật (Firewall)</option><option>Lưu Trữ (Storage)</option><option>Bản Quyền (License)</option><option>Vật Tư Phụ</option></select></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Số Lượng</label><input type=\"number\" name=\"soluong\" value=\"1\" min=\"1\" class=\"form-control\"></div>");
            sb.AppendLine("                <div><label style=\"font-size:0.8rem;font-weight:600;\">Đơn Giá (VNĐ)</label><input type=\"number\" name=\"dongia\" value=\"25000000\" min=\"0\" class=\"form-control\"></div>");
            sb.AppendLine("                <div><button type=\"submit\" name=\"btnThemTB\" value=\"1\" class=\"btn\">+ Lưu Vào CSDL</button></div>");
            sb.AppendLine("            </form>");
            sb.AppendLine("        </div>");

            sb.AppendLine("        <div class=\"card\">");
            sb.AppendLine("            <div class=\"card-header\"><h3>🖥️ Danh Mục Tài Sản &amp; Vật Tư CNTT (Bảng ERP_ThietBi)</h3></div>");
            sb.AppendLine("            <table><thead><tr><th>Mã TB</th><th>Tên Thiết Bị / Vật Tư</th><th>Phân Loại</th><th>SL</th><th>Đơn Giá</th><th>Thành Tiền</th><th>Trạng Thái</th></tr></thead><tbody>");
            foreach (var a in assets)
            {
                string badge = a.TrangThai == "Đang sử dụng" ? "badge-success" : "badge-warning";
                sb.AppendFormat("                <tr><td><strong>{0}</strong></td><td>{1}</td><td><span class=\"badge badge-info\">{2}</span></td><td><strong>{3}</strong></td><td>{4}</td><td style=\"font-weight:700;color:#1e40af;\">{5}</td><td><span class=\"badge {6}\">{7}</span></td></tr>\r\n",
                    WebUtility.HtmlEncode(a.MaTB), WebUtility.HtmlEncode(a.TenTB), WebUtility.HtmlEncode(a.LoaiTB), a.SoLuong, FormatVnd(a.DonGia), FormatVnd(a.ThanhTien), badge, WebUtility.HtmlEncode(a.TrangThai));
            }
            sb.AppendLine("            </tbody></table>");
            sb.AppendLine("        </div>");
        }

        private static void AppendProjectSection(StringBuilder sb, List<DuAnProject> projects)
        {
            sb.AppendLine("        <div class=\"card\">");
            sb.AppendLine("            <div class=\"card-header\"><h3>🚀 Tiến Độ Dự Án Hạ Tầng TDSouth Infrastructure</h3></div>");
            sb.AppendLine("            <table><thead><tr><th>Mã DA</th><th>Tên Dự Án Hạ Tầng</th><th>Phụ Trách</th><th>Kinh Phí Dự Toán</th><th>Tiến Độ</th><th>Trạng Thái</th></tr></thead><tbody>");
            foreach (var p in projects)
            {
                string badge = p.TienDo == 100 ? "badge-success" : "badge-info";
                sb.AppendFormat("                <tr><td><strong>{0}</strong></td><td><strong>{1}</strong></td><td>{2}</td><td>{3}</td><td><div class=\"progress-bg\"><div class=\"progress-fill\" style=\"width:{4}%;\"></div></div><strong>{4}%</strong></td><td><span class=\"badge {5}\">{6}</span></td></tr>\r\n",
                    WebUtility.HtmlEncode(p.MaDuAn), WebUtility.HtmlEncode(p.TenDuAn), WebUtility.HtmlEncode(p.TruongNhom), FormatVnd(p.KinhPhiDuToan), p.TienDo, badge, WebUtility.HtmlEncode(p.TrangThai));
            }
            sb.AppendLine("            </tbody></table>");
            sb.AppendLine("        </div>");
        }
    }
}

