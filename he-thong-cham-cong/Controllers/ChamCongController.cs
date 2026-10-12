using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using TDSouth.ChamCong.Models;
using TDSouth.ChamCong.Services;

namespace TDSouth.ChamCong.Controllers
{
    /// <summary>
    /// Controller C# điều phối nghiệp vụ và kết xuất giao diện Hệ Thống Chấm Công (TDSouth Infrastructure)
    /// </summary>
    public class ChamCongController
    {
        private readonly ChamCongService _service;

        public ChamCongController(ChamCongService service = null)
        {
            _service = service ?? new ChamCongService();
        }

        public string HandlePostAction(NameValueCollection form)
        {
            if (form == null) return string.Empty;

            string action = form["action"] ?? string.Empty;
            string maNV = form["manv"] ?? string.Empty;
            string hoTen = form["hoten"] ?? string.Empty;
            string phongBan = form["phongban"] ?? string.Empty;
            string caLamViec = form["calamviec"] ?? string.Empty;

            if (string.Equals(action, "checkin", StringComparison.OrdinalIgnoreCase))
            {
                return _service.CheckIn(maNV, hoTen, phongBan, caLamViec);
            }
            if (string.Equals(action, "checkout", StringComparison.OrdinalIgnoreCase))
            {
                return _service.CheckOut(maNV, hoTen);
            }

            return string.Empty;
        }

        public ChamCongStatistics GetDashboardStats(List<ChamCongRecord> records)
        {
            return ChamCongStatistics.Calculate(records, _service.IsDbConnected, _service.DbStatusMessage);
        }

        public List<ChamCongRecord> GetRecords(string keyword = null)
        {
            return _service.GetAttendanceRecords(keyword);
        }

        public string RenderHtmlPage(string actionMsg = "", string searchKeyword = "")
        {
            List<ChamCongRecord> records = _service.GetAttendanceRecords(searchKeyword);
            ChamCongStatistics stats = GetDashboardStats(records);
            List<NhanVienCollaborator> collaborators = NhanVienCollaborator.GetDefaultTeam();

            var sb = new StringBuilder(16384);
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"vi\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("    <title>Hệ Thống Chấm Công C# ASP.NET | TDSouth Infrastructure</title>");
            sb.AppendLine("    <link rel=\"preconnect\" href=\"https://fonts.googleapis.com\">");
            sb.AppendLine("    <link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin>");
            sb.AppendLine("    <link href=\"https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap\" rel=\"stylesheet\">");
            sb.AppendLine("    <style>");
            sb.AppendLine("        :root { --primary: #1e40af; --primary-hover: #1d4ed8; --primary-light: #dbeafe; --success: #059669; --warning: #d97706; --danger: #dc2626; --bg: #f1f5f9; --card-bg: #ffffff; --text-main: #0f172a; --text-muted: #64748b; --border: #e2e8f0; --radius: 12px; }");
            sb.AppendLine("        * { box-sizing: border-box; margin: 0; padding: 0; font-family: 'Inter', system-ui, sans-serif; }");
            sb.AppendLine("        body { background-color: var(--bg); color: var(--text-main); line-height: 1.5; min-height: 100vh; }");
            sb.AppendLine("        .navbar { background: linear-gradient(135deg, #0f172a 0%, #1e3a8a 100%); color: #fff; padding: 1rem 2rem; display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 1rem; box-shadow: 0 4px 12px rgba(0,0,0,0.12); }");
            sb.AppendLine("        .brand { display: flex; align-items: center; gap: 0.85rem; }");
            sb.AppendLine("        .brand-logo { width: 44px; height: 44px; background: #3b82f6; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 1.1rem; }");
            sb.AppendLine("        .brand-title { font-size: 1.2rem; font-weight: 700; }");
            sb.AppendLine("        .brand-sub { font-size: 0.8rem; color: #93c5fd; }");
            sb.AppendLine("        .nav-right { display: flex; align-items: center; gap: 0.85rem; flex-wrap: wrap; }");
            sb.AppendLine("        .badge-status { padding: 0.4rem 0.85rem; border-radius: 999px; font-size: 0.8rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.4rem; }");
            sb.AppendLine("        .status-online { background: rgba(16,185,129,0.2); color: #6ee7b7; border: 1px solid rgba(16,185,129,0.4); }");
            sb.AppendLine("        .status-demo { background: rgba(245,158,11,0.2); color: #fcd34d; border: 1px solid rgba(245,158,11,0.4); }");
            sb.AppendLine("        .container { max-width: 1280px; margin: 1.75rem auto; padding: 0 1.25rem; }");
            sb.AppendLine("        .stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 1.25rem; margin-bottom: 1.75rem; }");
            sb.AppendLine("        .stat-card { background: var(--card-bg); border-radius: var(--radius); padding: 1.25rem 1.5rem; border: 1px solid var(--border); box-shadow: 0 2px 6px rgba(0,0,0,0.04); }");
            sb.AppendLine("        .stat-label { font-size: 0.85rem; color: var(--text-muted); font-weight: 600; }");
            sb.AppendLine("        .stat-val { font-size: 2rem; font-weight: 800; margin-top: 0.25rem; }");
            sb.AppendLine("        .main-grid { display: grid; grid-template-columns: 380px 1fr; gap: 1.5rem; align-items: start; }");
            sb.AppendLine("        @media (max-width: 960px) { .main-grid { grid-template-columns: 1fr; } }");
            sb.AppendLine("        .card { background: var(--card-bg); border-radius: var(--radius); border: 1px solid var(--border); padding: 1.5rem; box-shadow: 0 2px 6px rgba(0,0,0,0.04); }");
            sb.AppendLine("        .card-title { font-size: 1.1rem; font-weight: 700; margin-bottom: 1rem; padding-bottom: 0.75rem; border-bottom: 1px solid var(--border); display: flex; justify-content: space-between; align-items: center; }");
            sb.AppendLine("        .clock-box { background: linear-gradient(135deg, #1e3a8a, #2563eb); color: #fff; text-align: center; padding: 1.25rem; border-radius: 10px; margin-bottom: 1.25rem; }");
            sb.AppendLine("        .clock-time { font-size: 2.25rem; font-weight: 800; letter-spacing: 2px; }");
            sb.AppendLine("        .clock-date { font-size: 0.88rem; color: #bfdbfe; margin-top: 0.25rem; }");
            sb.AppendLine("        .form-group { margin-bottom: 1rem; }");
            sb.AppendLine("        .form-group label { display: block; font-size: 0.85rem; font-weight: 600; margin-bottom: 0.35rem; color: #334155; }");
            sb.AppendLine("        .form-control { width: 100%; padding: 0.65rem 0.85rem; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 0.92rem; }");
            sb.AppendLine("        .form-control:user-invalid { border-color: var(--danger); }");
            sb.AppendLine("        .btn-group { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; margin-top: 1.25rem; }");
            sb.AppendLine("        .btn { padding: 0.75rem 1rem; border: none; border-radius: 8px; font-weight: 700; cursor: pointer; font-size: 0.92rem; transition: 0.2s; }");
            sb.AppendLine("        .btn-checkin { background: var(--primary); color: #fff; }");
            sb.AppendLine("        .btn-checkin:hover { background: var(--primary-hover); }");
            sb.AppendLine("        .btn-checkout { background: var(--success); color: #fff; }");
            sb.AppendLine("        .btn-checkout:hover { background: #047857; }");
            sb.AppendLine("        .alert { padding: 0.85rem 1rem; border-radius: 8px; margin-bottom: 1rem; font-size: 0.9rem; font-weight: 600; background: #dcfce7; color: #166534; border: 1px solid #86efac; }");
            sb.AppendLine("        table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }");
            sb.AppendLine("        th, td { padding: 0.85rem 0.75rem; text-align: left; border-bottom: 1px solid var(--border); }");
            sb.AppendLine("        th { background: #f8fafc; font-weight: 700; color: #475569; font-size: 0.8rem; text-transform: uppercase; }");
            sb.AppendLine("        .badge { padding: 0.25rem 0.65rem; border-radius: 999px; font-size: 0.75rem; font-weight: 700; }");
            sb.AppendLine("        .badge-ontime { background: #dcfce7; color: #15803d; }");
            sb.AppendLine("        .badge-late { background: #fef3c7; color: #b45309; }");
            sb.AppendLine("        .quick-chips { display: flex; flex-wrap: wrap; gap: 0.45rem; margin-bottom: 1rem; }");
            sb.AppendLine("        .chip { font-size: 0.78rem; padding: 0.3rem 0.65rem; background: #eff6ff; color: #1d4ed8; border: 1px solid #bfdbfe; border-radius: 999px; cursor: pointer; font-weight: 600; }");
            sb.AppendLine("        .chip:hover { background: #dbeafe; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<header class=\"navbar\">");
            sb.AppendLine("    <div class=\"brand\">");
            sb.AppendLine("        <div class=\"brand-logo\">C#</div>");
            sb.AppendLine("        <div>");
            sb.AppendLine("            <div class=\"brand-title\">TDSOUTH ATTENDANCE SYSTEM (C# ASP.NET)</div>");
            sb.AppendLine("            <div class=\"brand-sub\">Hệ Thống Chấm Công Nhân Viên &amp; Quản Lý Ca Trực Phòng Lab</div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    <div class=\"nav-right\">");
            sb.AppendFormat("        <span class=\"badge-status {0}\">{1}</span>\r\n",
                stats.IsDatabaseConnected ? "status-online" : "status-demo",
                WebUtility.HtmlEncode(stats.DatabaseStatusMessage));
            sb.AppendLine("    </div>");
            sb.AppendLine("</header>");

            sb.AppendLine("<main class=\"container\">");
            if (!string.IsNullOrWhiteSpace(actionMsg))
            {
                sb.AppendFormat("    <div class=\"alert\">{0}</div>\r\n", WebUtility.HtmlEncode(actionMsg));
            }

            sb.AppendLine("    <div class=\"stats-grid\">");
            sb.AppendFormat("        <div class=\"stat-card\"><div class=\"stat-label\">Tổng lượt chấm công</div><div class=\"stat-val\" style=\"color:#1e40af;\">{0}</div></div>\r\n", stats.TotalRecords);
            sb.AppendFormat("        <div class=\"stat-card\"><div class=\"stat-label\">Đúng giờ (Trước 08:15)</div><div class=\"stat-val\" style=\"color:#059669;\">{0}</div></div>\r\n", stats.OnTimeCount);
            sb.AppendFormat("        <div class=\"stat-card\"><div class=\"stat-label\">Đi muộn (Sau 08:15)</div><div class=\"stat-val\" style=\"color:#d97706;\">{0}</div></div>\r\n", stats.LateCount);
            sb.AppendFormat("        <div class=\"stat-card\"><div class=\"stat-label\">Đang trong ca trực</div><div class=\"stat-val\" style=\"color:#2563eb;\">{0}</div></div>\r\n", stats.CurrentlyWorkingCount);
            sb.AppendLine("    </div>");

            sb.AppendLine("    <div class=\"main-grid\">");
            sb.AppendLine("        <section class=\"card\">");
            sb.AppendLine("            <div class=\"card-title\"><span>⏱️ Chấm Công Vào / Ra Ca</span></div>");
            sb.AppendLine("            <div class=\"clock-box\">");
            sb.AppendFormat("                <div class=\"clock-time\" id=\"liveClock\">{0}</div>\r\n", DateTime.Now.ToString("HH:mm:ss"));
            sb.AppendFormat("                <div class=\"clock-date\">Ngày {0} | C# .NET Runtime</div>\r\n", DateTime.Now.ToString("dd/MM/yyyy"));
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div class=\"form-group\"><label>Chọn nhanh thành viên TDSouth:</label><div class=\"quick-chips\">");
            foreach (var member in collaborators)
            {
                sb.AppendFormat("                <span class=\"chip\" onclick=\"fillMember('{0}','{1}','{2}')\">{0} - {1}</span>\r\n",
                    WebUtility.HtmlEncode(member.MaNV),
                    WebUtility.HtmlEncode(member.HoTen),
                    WebUtility.HtmlEncode(member.PhongBan));
            }
            sb.AppendLine("            </div></div>");
            sb.AppendLine("            <form method=\"POST\" action=\"\">");
            sb.AppendLine("                <div class=\"form-group\">");
            sb.AppendLine("                    <label>Mã Nhân Viên (*)</label>");
            sb.AppendLine("                    <input type=\"text\" id=\"manv\" name=\"manv\" class=\"form-control\" placeholder=\"VD: TDS001\" required>");
            sb.AppendLine("                </div>");
            sb.AppendLine("                <div class=\"form-group\">");
            sb.AppendLine("                    <label>Họ và Tên (*)</label>");
            sb.AppendLine("                    <input type=\"text\" id=\"hoten\" name=\"hoten\" class=\"form-control\" placeholder=\"VD: Lê Minh Phúc\" required>");
            sb.AppendLine("                </div>");
            sb.AppendLine("                <div class=\"form-group\">");
            sb.AppendLine("                    <label>Phòng Ban / Vai Trò</label>");
            sb.AppendLine("                    <select id=\"phongban\" name=\"phongban\" class=\"form-control\">");
            sb.AppendLine("                        <option value=\"Quản trị Hệ thống (Admin)\">Quản trị Hệ thống (Admin)</option>");
            sb.AppendLine("                        <option value=\"Kỹ thuật Hạ tầng (Tech Lead)\">Kỹ thuật Hạ tầng (Tech Lead)</option>");
            sb.AppendLine("                        <option value=\"Phát triển Ứng dụng (Dev)\">Phát triển Ứng dụng (Dev)</option>");
            sb.AppendLine("                        <option value=\"Kiểm thử Chất lượng (QA)\">Kiểm thử Chất lượng (QA)</option>");
            sb.AppendLine("                    </select>");
            sb.AppendLine("                </div>");
            sb.AppendLine("                <div class=\"form-group\">");
            sb.AppendLine("                    <label>Ca Làm Việc</label>");
            sb.AppendLine("                    <select id=\"calamviec\" name=\"calamviec\" class=\"form-control\">");
            sb.AppendLine("                        <option value=\"Ca Hành Chính (08:00 - 17:30)\">Ca Hành Chính (08:00 - 17:30)</option>");
            sb.AppendLine("                        <option value=\"Ca Sáng (08:00 - 12:00)\">Ca Sáng (08:00 - 12:00)</option>");
            sb.AppendLine("                        <option value=\"Ca Chiều (13:30 - 17:30)\">Ca Chiều (13:30 - 17:30)</option>");
            sb.AppendLine("                        <option value=\"Ca Trực Server Đêm (18:00 - 22:00)\">Ca Trực Server Đêm (18:00 - 22:00)</option>");
            sb.AppendLine("                    </select>");
            sb.AppendLine("                </div>");
            sb.AppendLine("                <div class=\"btn-group\">");
            sb.AppendLine("                    <button type=\"submit\" name=\"action\" value=\"checkin\" class=\"btn btn-checkin\">✅ CHECK-IN VÀO CA</button>");
            sb.AppendLine("                    <button type=\"submit\" name=\"action\" value=\"checkout\" class=\"btn btn-checkout\">🚪 CHECK-OUT RA CA</button>");
            sb.AppendLine("                </div>");
            sb.AppendLine("            </form>");
            sb.AppendLine("        </section>");

            sb.AppendLine("        <section class=\"card\">");
            sb.AppendLine("            <div class=\"card-title\">");
            sb.AppendLine("                <span>📋 Nhật Ký Chấm Công Thời Gian Thực</span>");
            sb.AppendLine("                <form method=\"GET\" action=\"\" style=\"display:flex;gap:0.5rem;\">");
            sb.AppendFormat("                    <input type=\"text\" name=\"q\" value=\"{0}\" class=\"form-control\" placeholder=\"Tìm mã NV, họ tên...\" style=\"width:210px;padding:0.4rem 0.7rem;\">\r\n",
                WebUtility.HtmlEncode(searchKeyword ?? ""));
            sb.AppendLine("                    <button type=\"submit\" class=\"btn btn-checkin\" style=\"padding:0.4rem 0.85rem;\">Lọc</button>");
            sb.AppendLine("                </form>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div style=\"overflow-x:auto;\">");
            sb.AppendLine("                <table>");
            sb.AppendLine("                    <thead><tr><th>Mã NV</th><th>Họ và Tên</th><th>Phòng Ban</th><th>Ca Trực</th><th>Giờ Vào</th><th>Giờ Ra</th><th>Ngày</th><th>Trạng Thái</th></tr></thead>");
            sb.AppendLine("                    <tbody>");
            foreach (var r in records)
            {
                string badgeClass = string.Equals(r.TrangThai, "Đúng giờ", StringComparison.OrdinalIgnoreCase) ? "badge-ontime" : "badge-late";
                sb.AppendLine("                        <tr>");
                sb.AppendFormat("                            <td><strong>{0}</strong></td>\r\n", WebUtility.HtmlEncode(r.MaNV));
                sb.AppendFormat("                            <td><strong>{0}</strong></td>\r\n", WebUtility.HtmlEncode(r.HoTen));
                sb.AppendFormat("                            <td>{0}</td>\r\n", WebUtility.HtmlEncode(r.PhongBan));
                sb.AppendFormat("                            <td>{0}</td>\r\n", WebUtility.HtmlEncode(r.CaLamViec));
                sb.AppendFormat("                            <td style=\"color:#1e40af;font-weight:700;\">{0}</td>\r\n", WebUtility.HtmlEncode(r.GioVao));
                sb.AppendFormat("                            <td style=\"color:#059669;font-weight:700;\">{0}</td>\r\n", WebUtility.HtmlEncode(r.GioRa));
                sb.AppendFormat("                            <td>{0}</td>\r\n", WebUtility.HtmlEncode(r.NgayCham));
                sb.AppendFormat("                            <td><span class=\"badge {0}\">{1}</span></td>\r\n", badgeClass, WebUtility.HtmlEncode(r.TrangThai));
                sb.AppendLine("                        </tr>");
            }
            sb.AppendLine("                    </tbody>");
            sb.AppendLine("                </table>");
            sb.AppendLine("            </div>");
            sb.AppendLine("        </section>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</main>");
            sb.AppendLine("<script>");
            sb.AppendLine("    function fillMember(id, name, dept) {");
            sb.AppendLine("        document.getElementById('manv').value = id;");
            sb.AppendLine("        document.getElementById('hoten').value = name;");
            sb.AppendLine("        document.getElementById('phongban').value = dept;");
            sb.AppendLine("    }");
            sb.AppendLine("    setInterval(function() {");
            sb.AppendLine("        var d = new Date();");
            sb.AppendLine("        var el = document.getElementById('liveClock');");
            sb.AppendLine("        if (el) el.textContent = d.toTimeString().split(' ')[0];");
            sb.AppendLine("    }, 1000);");
            sb.AppendLine("</script>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}

