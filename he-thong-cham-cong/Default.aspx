<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="TDSouth.ChamCong.DefaultPage" ResponseEncoding="utf-8" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Hệ Thống Chấm Công (C# ASP.NET) | TDSouth Infrastructure</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet">
    <style>
        :root {
            --primary: #1e40af;
            --primary-hover: #1d4ed8;
            --primary-light: #dbeafe;
            --success: #059669;
            --warning: #d97706;
            --danger: #dc2626;
            --bg: #f1f5f9;
            --card-bg: #ffffff;
            --text-main: #0f172a;
            --text-muted: #64748b;
            --border: #e2e8f0;
            --radius: 12px;
            --shadow: 0 4px 6px -1px rgb(0 0 0 / 0.08), 0 2px 4px -2px rgb(0 0 0 / 0.06);
        }

        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
        }

        body {
            background-color: var(--bg);
            color: var(--text-main);
            min-height: 100vh;
            line-height: 1.5;
        }

        .navbar {
            background: linear-gradient(135deg, #0f172a 0%, #1e3a8a 100%);
            color: #ffffff;
            padding: 1rem 2rem;
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 1rem;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }

        .brand {
            display: flex;
            align-items: center;
            gap: 0.9rem;
        }

        .brand-logo {
            width: 44px;
            height: 44px;
            background: linear-gradient(135deg, #3b82f6, #1d4ed8);
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 800;
            font-size: 1.15rem;
            box-shadow: 0 2px 8px rgba(59, 130, 246, 0.5);
        }

        .brand-title {
            font-size: 1.2rem;
            font-weight: 700;
            letter-spacing: -0.01em;
        }

        .brand-sub {
            font-size: 0.8rem;
            color: #93c5fd;
        }

        .nav-right {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            flex-wrap: wrap;
        }

        .badge-status {
            padding: 0.4rem 0.9rem;
            border-radius: 999px;
            font-size: 0.8rem;
            font-weight: 600;
        }

        .status-online {
            background: rgba(16, 185, 129, 0.2);
            color: #6ee7b7;
            border: 1px solid rgba(16, 185, 129, 0.45);
        }

        .status-demo {
            background: rgba(245, 158, 11, 0.2);
            color: #fcd34d;
            border: 1px solid rgba(245, 158, 11, 0.45);
        }

        .container {
            max-width: 1280px;
            margin: 1.75rem auto;
            padding: 0 1.25rem;
        }

        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(230px, 1fr));
            gap: 1.25rem;
            margin-bottom: 1.75rem;
        }

        .stat-card {
            background: var(--card-bg);
            border-radius: var(--radius);
            padding: 1.35rem 1.5rem;
            border: 1px solid var(--border);
            box-shadow: var(--shadow);
        }

        .stat-label {
            font-size: 0.85rem;
            color: var(--text-muted);
            font-weight: 600;
        }

        .stat-val {
            font-size: 2.1rem;
            font-weight: 800;
            margin-top: 0.3rem;
        }

        .main-grid {
            display: grid;
            grid-template-columns: 390px 1fr;
            gap: 1.5rem;
            align-items: start;
        }

        @media (max-width: 980px) {
            .main-grid {
                grid-template-columns: 1fr;
            }
        }

        .card {
            background: var(--card-bg);
            border-radius: var(--radius);
            border: 1px solid var(--border);
            padding: 1.5rem;
            box-shadow: var(--shadow);
        }

        .card-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 0.75rem;
            margin-bottom: 1.15rem;
            padding-bottom: 0.8rem;
            border-bottom: 1px solid var(--border);
        }

        .card-title {
            font-size: 1.1rem;
            font-weight: 700;
        }

        .clock-box {
            background: linear-gradient(135deg, #1e3a8a, #2563eb);
            color: #ffffff;
            text-align: center;
            padding: 1.35rem;
            border-radius: 10px;
            margin-bottom: 1.25rem;
        }

        .clock-time {
            font-size: 2.3rem;
            font-weight: 800;
            letter-spacing: 2px;
        }

        .clock-date {
            font-size: 0.85rem;
            color: #bfdbfe;
            margin-top: 0.25rem;
        }

        .quick-chips {
            display: flex;
            flex-wrap: wrap;
            gap: 0.45rem;
            margin-bottom: 1rem;
        }

        .chip {
            font-size: 0.78rem;
            padding: 0.32rem 0.7rem;
            background: #eff6ff;
            color: #1d4ed8;
            border: 1px solid #bfdbfe;
            border-radius: 999px;
            cursor: pointer;
            font-weight: 600;
            transition: 0.15s;
        }

        .chip:hover {
            background: #dbeafe;
        }

        .form-group {
            margin-bottom: 1rem;
        }

        .form-group label {
            display: block;
            font-size: 0.85rem;
            font-weight: 600;
            margin-bottom: 0.35rem;
            color: #334155;
        }

        .form-control {
            width: 100%;
            padding: 0.65rem 0.85rem;
            border: 1px solid #cbd5e1;
            border-radius: 8px;
            font-size: 0.92rem;
        }

        .form-control:user-invalid {
            border-color: var(--danger);
        }

        .btn-group {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 0.75rem;
            margin-top: 1.25rem;
        }

        .btn {
            padding: 0.75rem 1rem;
            border: none;
            border-radius: 8px;
            font-weight: 700;
            cursor: pointer;
            font-size: 0.9rem;
            transition: 0.2s;
        }

        .btn-checkin {
            background: var(--primary);
            color: #ffffff;
        }

        .btn-checkin:hover {
            background: var(--primary-hover);
        }

        .btn-checkout {
            background: var(--success);
            color: #ffffff;
        }

        .btn-checkout:hover {
            background: #047857;
        }

        .alert {
            padding: 0.9rem 1.15rem;
            border-radius: 8px;
            margin-bottom: 1.25rem;
            font-size: 0.92rem;
            font-weight: 600;
            background: #dcfce7;
            color: #166534;
            border: 1px solid #86efac;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            font-size: 0.9rem;
        }

        th, td {
            padding: 0.85rem 0.75rem;
            text-align: left;
            border-bottom: 1px solid var(--border);
        }

        th {
            background: #f8fafc;
            font-weight: 700;
            color: #475569;
            font-size: 0.78rem;
            text-transform: uppercase;
        }

        .badge {
            padding: 0.25rem 0.65rem;
            border-radius: 999px;
            font-size: 0.75rem;
            font-weight: 700;
        }

        .badge-ontime {
            background: #dcfce7;
            color: #15803d;
        }

        .badge-late {
            background: #fef3c7;
            color: #b45309;
        }
    </style>
</head>
<body>

<header class="navbar">
    <div class="brand">
        <div class="brand-logo">C#</div>
        <div>
            <div class="brand-title">HỆ THỐNG CHẤM CÔNG NHÂN VIÊN (C# ASP.NET)</div>
            <div class="brand-sub">TDSouth Infrastructure Lab &bull; SQL Server: localhost\SQLEXPRESS (DB: TDSouth)</div>
        </div>
    </div>
    <div class="nav-right">
        <span class="badge-status <%= DbConnected ? "status-online" : "status-demo" %>">
            <%= HttpUtility.HtmlEncode(DbStatusMessage) %>
        </span>
    </div>
</header>

<main class="container">
    <% if (!string.IsNullOrEmpty(ActionMessage)) { %>
        <div class="alert"><%= HttpUtility.HtmlEncode(ActionMessage) %></div>
    <% } %>

    <div class="stats-grid">
        <div class="stat-card">
            <div class="stat-label">Tổng lượt chấm công trong ngày</div>
            <div class="stat-val" style="color: #1e40af;"><%= TotalRecords %></div>
        </div>
        <div class="stat-card">
            <div class="stat-label">Nhân sự đi đúng giờ (&le; 08:15)</div>
            <div class="stat-val" style="color: #059669;"><%= OnTimeCount %></div>
        </div>
        <div class="stat-card">
            <div class="stat-label">Nhân sự đi muộn (&gt; 08:15)</div>
            <div class="stat-val" style="color: #d97706;"><%= LateCount %></div>
        </div>
        <div class="stat-card">
            <div class="stat-label">Đang trong ca làm việc</div>
            <div class="stat-val" style="color: #2563eb;"><%= WorkingNowCount %></div>
        </div>
    </div>

    <div class="main-grid">
        <!-- Cột Trái: Form Check-in / Check-out -->
        <section class="card">
            <div class="card-header">
                <h2 class="card-title">⏱️ Ghi Nhận Chấm Công (C#)</h2>
            </div>

            <div class="clock-box">
                <div class="clock-time" id="liveClock"><%= DateTime.Now.ToString("HH:mm:ss") %></div>
                <div class="clock-date">Hôm nay: <%= DateTime.Now.ToString("dd/MM/yyyy") %> &bull; Ca chuẩn: 08:00</div>
            </div>

            <div class="form-group">
                <label>Chọn nhanh thành viên dự án (COLLABORATORS):</label>
                <div class="quick-chips">
                    <span class="chip" onclick="selectMember('TDS001', 'Lê Minh Phúc', 'Quản trị Hệ thống (Admin)')">TDS001 - Lê Minh Phúc</span>
                    <span class="chip" onclick="selectMember('TDS002', 'Kỳ Nguyễn', 'Kỹ thuật Hạ tầng (Tech Lead)')">TDS002 - Kỳ Nguyễn</span>
                    <span class="chip" onclick="selectMember('TDS003', 'Trường Nguyễn', 'Phát triển Ứng dụng (Dev)')">TDS003 - Trường Nguyễn</span>
                    <span class="chip" onclick="selectMember('TDS004', 'Tín Phan', 'Kiểm thử Chất lượng (QA)')">TDS004 - Tín Phan</span>
                </div>
            </div>

            <form method="POST" action="Default.aspx">
                <div class="form-group">
                    <label for="manv">Mã Nhân Viên (*)</label>
                    <input type="text" id="manv" name="manv" class="form-control" placeholder="VD: TDS001" required>
                </div>
                <div class="form-group">
                    <label for="hoten">Họ và Tên Nhân Viên (*)</label>
                    <input type="text" id="hoten" name="hoten" class="form-control" placeholder="VD: Lê Minh Phúc" required>
                </div>
                <div class="form-group">
                    <label for="phongban">Phòng Ban / Chức Vụ</label>
                    <select id="phongban" name="phongban" class="form-control">
                        <option value="Quản trị Hệ thống (Admin)">Quản trị Hệ thống (Admin)</option>
                        <option value="Kỹ thuật Hạ tầng (Tech Lead)">Kỹ thuật Hạ tầng (Tech Lead)</option>
                        <option value="Phát triển Ứng dụng (Dev)">Phát triển Ứng dụng (Dev)</option>
                        <option value="Kiểm thử Chất lượng (QA)">Kiểm thử Chất lượng (QA)</option>
                    </select>
                </div>
                <div class="form-group">
                    <label for="calamviec">Ca Làm Việc</label>
                    <select id="calamviec" name="calamviec" class="form-control">
                        <option value="Ca Hành Chính (08:00 - 17:30)">Ca Hành Chính (08:00 - 17:30)</option>
                        <option value="Ca Sáng (08:00 - 12:00)">Ca Sáng (08:00 - 12:00)</option>
                        <option value="Ca Chiều (13:30 - 17:30)">Ca Chiều (13:30 - 17:30)</option>
                        <option value="Ca Trực Server Đêm (18:00 - 22:00)">Ca Trực Server Đêm (18:00 - 22:00)</option>
                    </select>
                </div>
                <div class="btn-group">
                    <button type="submit" name="action" value="checkin" class="btn btn-checkin">✅ CHECK-IN VÀO CA</button>
                    <button type="submit" name="action" value="checkout" class="btn btn-checkout">🚪 CHECK-OUT RA CA</button>
                </div>
            </form>
        </section>

        <!-- Cột Phải: Bảng dữ liệu chấm công -->
        <section class="card">
            <div class="card-header">
                <h2 class="card-title">📋 Danh Sách Chấm Công Nhân Viên</h2>
                <form method="GET" action="Default.aspx" style="display: flex; gap: 0.5rem;">
                    <input type="text" name="q" value="<%= HttpUtility.HtmlEncode(SearchQuery) %>" class="form-control" placeholder="Tìm mã NV, họ tên..." style="width: 220px; padding: 0.45rem 0.75rem;">
                    <button type="submit" class="btn btn-checkin" style="padding: 0.45rem 0.9rem;">Tìm kiếm</button>
                </form>
            </div>

            <div style="overflow-x: auto;">
                <table>
                    <thead>
                        <tr>
                            <th>Mã NV</th>
                            <th>Họ và Tên</th>
                            <th>Phòng Ban</th>
                            <th>Ca Làm Việc</th>
                            <th>Giờ Vào</th>
                            <th>Giờ Ra</th>
                            <th>Ngày Chấm</th>
                            <th>Trạng Thái</th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var item in AttendanceList) { %>
                        <tr>
                            <td><strong><%= HttpUtility.HtmlEncode(item.MaNV) %></strong></td>
                            <td><strong><%= HttpUtility.HtmlEncode(item.HoTen) %></strong></td>
                            <td><%= HttpUtility.HtmlEncode(item.PhongBan) %></td>
                            <td><%= HttpUtility.HtmlEncode(item.CaLamViec) %></td>
                            <td style="color: #1e40af; font-weight: 700;"><%= HttpUtility.HtmlEncode(item.GioVao) %></td>
                            <td style="color: #059669; font-weight: 700;"><%= HttpUtility.HtmlEncode(item.GioRa) %></td>
                            <td><%= HttpUtility.HtmlEncode(item.NgayCham) %></td>
                            <td>
                                <% if (item.TrangThai == "Đúng giờ") { %>
                                    <span class="badge badge-ontime">Đúng giờ</span>
                                <% } else { %>
                                    <span class="badge badge-late"><%= HttpUtility.HtmlEncode(item.TrangThai) %></span>
                                <% } %>
                            </td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </section>
    </div>
</main>

<script>
    function selectMember(id, name, dept) {
        document.getElementById('manv').value = id;
        document.getElementById('hoten').value = name;
        document.getElementById('phongban').value = dept;
    }

    setInterval(function () {
        var now = new Date();
        var clockEl = document.getElementById('liveClock');
        if (clockEl) {
            clockEl.textContent = now.toTimeString().split(' ')[0];
        }
    }, 1000);
</script>

</body>
</html>

