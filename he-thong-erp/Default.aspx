<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="TDSouth.Erp.DefaultPage" ResponseEncoding="utf-8" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Hệ Thống ERP (C# ASP.NET) | TDSouth Infrastructure</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet">
    <style>
        :root {
            --primary: #0f172a;
            --primary-accent: #2563eb;
            --sidebar-bg: #1e293b;
            --sidebar-hover: #334155;
            --bg: #f1f5f9;
            --card-bg: #ffffff;
            --text-main: #0f172a;
            --text-muted: #64748b;
            --border: #e2e8f0;
            --radius: 12px;
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
            display: flex;
            min-height: 100vh;
        }

        .sidebar {
            width: 270px;
            background: var(--sidebar-bg);
            color: #ffffff;
            padding: 1.5rem 1rem;
            display: flex;
            flex-direction: column;
            gap: 0.45rem;
            flex-shrink: 0;
        }

        @media (max-width: 920px) {
            body {
                flex-direction: column;
            }
            .sidebar {
                width: 100%;
            }
        }

        .brand {
            padding: 0.5rem 0.75rem 1.25rem;
            border-bottom: 1px solid #334155;
            margin-bottom: 0.75rem;
        }

        .brand-title {
            font-size: 1.2rem;
            font-weight: 800;
            color: #60a5fa;
        }

        .brand-sub {
            font-size: 0.78rem;
            color: #94a3b8;
            margin-top: 0.25rem;
        }

        .nav-item {
            padding: 0.8rem 1rem;
            border-radius: 8px;
            cursor: pointer;
            font-weight: 600;
            font-size: 0.9rem;
            color: #cbd5e1;
            transition: 0.15s;
        }

        .nav-item:hover, .nav-item.active {
            background: var(--primary-accent);
            color: #ffffff;
        }

        .main-content {
            flex: 1;
            padding: 1.75rem 2rem;
            overflow-x: auto;
        }

        .topbar {
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 1rem;
            margin-bottom: 1.5rem;
        }

        .kpi-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
            gap: 1.25rem;
            margin-bottom: 1.75rem;
        }

        .kpi-card {
            background: var(--card-bg);
            border: 1px solid var(--border);
            border-radius: var(--radius);
            padding: 1.35rem 1.5rem;
        }

        .kpi-label {
            font-size: 0.82rem;
            color: var(--text-muted);
            font-weight: 600;
        }

        .kpi-val {
            font-size: 1.65rem;
            font-weight: 800;
            margin-top: 0.35rem;
            color: #1e40af;
        }

        .card {
            background: var(--card-bg);
            border: 1px solid var(--border);
            border-radius: var(--radius);
            padding: 1.5rem;
            margin-bottom: 1.5rem;
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

        .tab-content {
            display: none;
        }

        .tab-content.active {
            display: block;
        }

        .form-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(175px, 1fr));
            gap: 1rem;
            align-items: end;
        }

        .form-control {
            width: 100%;
            padding: 0.62rem 0.8rem;
            border: 1px solid #cbd5e1;
            border-radius: 8px;
            font-size: 0.9rem;
        }

        .btn {
            padding: 0.65rem 1.2rem;
            background: var(--primary-accent);
            color: #ffffff;
            border: none;
            border-radius: 8px;
            font-weight: 700;
            cursor: pointer;
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
            font-size: 0.78rem;
            text-transform: uppercase;
            color: #475569;
            font-weight: 700;
        }

        .badge {
            padding: 0.25rem 0.65rem;
            border-radius: 999px;
            font-size: 0.75rem;
            font-weight: 700;
        }

        .badge-success {
            background: #dcfce7;
            color: #15803d;
        }

        .badge-warning {
            background: #fef3c7;
            color: #b45309;
        }

        .badge-info {
            background: #dbeafe;
            color: #1d4ed8;
        }

        .badge-purple {
            background: #f3e8ff;
            color: #7e22ce;
        }

        .alert {
            padding: 0.9rem 1.15rem;
            background: #dcfce7;
            color: #166534;
            border: 1px solid #86efac;
            border-radius: 8px;
            margin-bottom: 1.25rem;
            font-weight: 600;
        }

        .progress-bar-bg {
            width: 130px;
            height: 8px;
            background: #e2e8f0;
            border-radius: 999px;
            overflow: hidden;
            display: inline-block;
            vertical-align: middle;
            margin-right: 8px;
        }

        .progress-bar-fill {
            height: 100%;
            background: var(--primary-accent);
        }
    </style>
</head>
<body>

<aside class="sidebar">
    <div class="brand">
        <div class="brand-title">TDSOUTH ERP (C#)</div>
        <div class="brand-sub">Hệ Thống Quản Trị Nguồn Lực &bull; ASP.NET</div>
    </div>
    <div class="nav-item active" onclick="switchTab('tab-overview', this)">📊 Tổng Quan Điều Hành</div>
    <div class="nav-item" onclick="switchTab('tab-assets', this)">🖥️ Vật Tư &amp; Thiết Bị IT</div>
    <div class="nav-item" onclick="switchTab('tab-projects', this)">🚀 Dự Án Hạ Tầng Lab</div>
    <div class="nav-item" onclick="switchTab('tab-hr', this)">👥 Nhân Sự &amp; Quyền Hạn</div>
</aside>

<main class="main-content">
    <div class="topbar">
        <div>
            <h1 style="font-size: 1.45rem; font-weight: 800;">Hệ Thống Hoạch Định Nguồn Lực Doanh Nghiệp (C# ASP.NET ERP)</h1>
            <p style="font-size: 0.85rem; color: var(--text-muted);">TDSouth Infrastructure &bull; SQL Server: localhost\SQLEXPRESS (Bảng: ERP_ThietBi)</p>
        </div>
        <span class="badge <%= DbConnected ? "badge-success" : "badge-warning" %>">
            <%= HttpUtility.HtmlEncode(DbStatusMessage) %>
        </span>
    </div>

    <% if (!string.IsNullOrEmpty(ActionMessage)) { %>
        <div class="alert"><%= HttpUtility.HtmlEncode(ActionMessage) %></div>
    <% } %>

    <div class="kpi-grid">
        <div class="kpi-card">
            <div class="kpi-label">Tổng Giá Trị Tài Sản Hạ Tầng</div>
            <div class="kpi-val"><%= FormatCurrency(TotalAssetValue) %></div>
        </div>
        <div class="kpi-card">
            <div class="kpi-label">Tổng Thiết Bị &amp; Vật Tư IT</div>
            <div class="kpi-val"><%= TotalAssetUnits %> đơn vị (<%= AssetList.Count %> loại)</div>
        </div>
        <div class="kpi-card">
            <div class="kpi-label">Dự Án Hạ Tầng Triển Khai</div>
            <div class="kpi-val"><%= ProjectList.Count %> dự án</div>
        </div>
        <div class="kpi-card">
            <div class="kpi-label">Độ Sẵn Sàng Hệ Thống (SLA)</div>
            <div class="kpi-val" style="color: #059669;">99.98%</div>
        </div>
    </div>

    <!-- TAB 1: TỔNG QUAN -->
    <div id="tab-overview" class="tab-content active">
        <div class="card">
            <div class="card-header">
                <h2 class="card-title">🖥️ Tổng Hợp Tài Sản &amp; Thiết Bị Hạ Tầng Chủ Lực</h2>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>Mã TB</th>
                        <th>Tên Thiết Bị / Vật Tư</th>
                        <th>Phân Loại</th>
                        <th>Số Lượng</th>
                        <th>Đơn Giá</th>
                        <th>Thành Tiền</th>
                        <th>Trạng Thái</th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var item in AssetList) { %>
                    <tr>
                        <td><strong><%= HttpUtility.HtmlEncode(item.MaTB) %></strong></td>
                        <td><%= HttpUtility.HtmlEncode(item.TenTB) %></td>
                        <td><span class="badge badge-info"><%= HttpUtility.HtmlEncode(item.LoaiTB) %></span></td>
                        <td><strong><%= item.SoLuong %></strong></td>
                        <td><%= FormatCurrency(item.DonGia) %></td>
                        <td style="font-weight: 700; color: #1e40af;"><%= FormatCurrency(item.ThanhTien) %></td>
                        <td>
                            <span class="badge <%= item.TrangThai == "Đang sử dụng" ? "badge-success" : "badge-warning" %>">
                                <%= HttpUtility.HtmlEncode(item.TrangThai) %>
                            </span>
                        </td>
                    </tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>

    <!-- TAB 2: QUẢN LÝ VẬT TƯ & THIẾT BỊ -->
    <div id="tab-assets" class="tab-content">
        <div class="card">
            <div class="card-header">
                <h2 class="card-title">➕ Nhập Kho Thiết Bị / Vật Tư Mới (C# SQL Server)</h2>
            </div>
            <form method="POST" action="Default.aspx" class="form-grid">
                <div>
                    <label style="font-size: 0.8rem; font-weight: 600;">Mã Thiết Bị (*)</label>
                    <input type="text" name="matb" class="form-control" placeholder="VD: SRV-02" required>
                </div>
                <div>
                    <label style="font-size: 0.8rem; font-weight: 600;">Tên Thiết Bị / Vật Tư (*)</label>
                    <input type="text" name="tentb" class="form-control" placeholder="VD: Máy chủ HP ProLiant DL380" required>
                </div>
                <div>
                    <label style="font-size: 0.8rem; font-weight: 600;">Phân Loại</label>
                    <select name="loaitb" class="form-control">
                        <option value="Máy Chủ (Server)">Máy Chủ (Server)</option>
                        <option value="Thiết Bị Mạng (Switch)">Thiết Bị Mạng (Switch)</option>
                        <option value="Bảo Mật (Firewall)">Bảo Mật (Firewall)</option>
                        <option value="Lưu Trữ (Storage)">Lưu Trữ (Storage)</option>
                        <option value="Bản Quyền (License)">Bản Quyền (License)</option>
                        <option value="Vật Tư Phụ">Vật Tư Phụ</option>
                    </select>
                </div>
                <div>
                    <label style="font-size: 0.8rem; font-weight: 600;">Số Lượng</label>
                    <input type="number" name="soluong" value="1" min="1" class="form-control">
                </div>
                <div>
                    <label style="font-size: 0.8rem; font-weight: 600;">Đơn Giá (VNĐ)</label>
                    <input type="number" name="dongia" value="25000000" min="0" class="form-control">
                </div>
                <div>
                    <label style="font-size: 0.8rem; font-weight: 600;">Trạng Thái</label>
                    <select name="trangthai" class="form-control">
                        <option value="Đang sử dụng">Đang sử dụng</option>
                        <option value="Trong kho">Trong kho</option>
                        <option value="Đang bảo trì">Đang bảo trì</option>
                    </select>
                </div>
                <div>
                    <button type="submit" name="btnThemTB" value="1" class="btn">+ Thêm Thiết Bị</button>
                </div>
            </form>
        </div>

        <div class="card">
            <div class="card-header">
                <h2 class="card-title">📦 Danh Sách Thiết Bị &amp; Vật Tư Hiện Có</h2>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>Mã TB</th>
                        <th>Tên Thiết Bị / Vật Tư</th>
                        <th>Phân Loại</th>
                        <th>Số Lượng</th>
                        <th>Đơn Giá</th>
                        <th>Thành Tiền</th>
                        <th>Trạng Thái</th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var item in AssetList) { %>
                    <tr>
                        <td><strong><%= HttpUtility.HtmlEncode(item.MaTB) %></strong></td>
                        <td><%= HttpUtility.HtmlEncode(item.TenTB) %></td>
                        <td><span class="badge badge-info"><%= HttpUtility.HtmlEncode(item.LoaiTB) %></span></td>
                        <td><strong><%= item.SoLuong %></strong></td>
                        <td><%= FormatCurrency(item.DonGia) %></td>
                        <td style="font-weight: 700; color: #1e40af;"><%= FormatCurrency(item.ThanhTien) %></td>
                        <td>
                            <span class="badge <%= item.TrangThai == "Đang sử dụng" ? "badge-success" : "badge-warning" %>">
                                <%= HttpUtility.HtmlEncode(item.TrangThai) %>
                            </span>
                        </td>
                    </tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>

    <!-- TAB 3: DỰ ÁN HẠ TẦNG -->
    <div id="tab-projects" class="tab-content">
        <div class="card">
            <div class="card-header">
                <h2 class="card-title">🚀 Quản Lý Danh Mục Dự Án Triển Khai IT Lab</h2>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>Mã Dự Án</th>
                        <th>Tên Dự Án</th>
                        <th>Trưởng Nhóm / Phụ Trách</th>
                        <th>Kinh Phí Dự Toán</th>
                        <th>Tiến Độ Thực Hiện</th>
                        <th>Trạng Thái</th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var pj in ProjectList) { %>
                    <tr>
                        <td><strong><%= HttpUtility.HtmlEncode(pj.Code) %></strong></td>
                        <td style="font-weight: 600;"><%= HttpUtility.HtmlEncode(pj.Name) %></td>
                        <td><%= HttpUtility.HtmlEncode(pj.Lead) %></td>
                        <td><%= FormatCurrency(pj.Budget) %></td>
                        <td>
                            <div class="progress-bar-bg">
                                <div class="progress-bar-fill" style="width: <%= pj.Progress %>%;"></div>
                            </div>
                            <strong><%= pj.Progress %>%</strong>
                        </td>
                        <td>
                            <span class="badge <%= pj.Status == "Hoàn thành" ? "badge-success" : "badge-info" %>">
                                <%= HttpUtility.HtmlEncode(pj.Status) %>
                            </span>
                        </td>
                    </tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>

    <!-- TAB 4: NHÂN SỰ & QUYỀN HẠN -->
    <div id="tab-hr" class="tab-content">
        <div class="card">
            <div class="card-header">
                <h2 class="card-title">👥 Ma Trận Phân Quyền Thành Viên Dự Án (COLLABORATORS.md)</h2>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>STT</th>
                        <th>Họ và Tên</th>
                        <th>Vai Trò Dự Án</th>
                        <th>Quyền Hạn GitHub</th>
                        <th>Mô Tả Trách Nhiệm &amp; Phạm Vi</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>1</td>
                        <td><strong>Nguyễn Văn A</strong></td>
                        <td><span class="badge badge-purple">Project Owner / Admin</span></td>
                        <td><strong style="color: #7e22ce;">Admin</strong></td>
                        <td>Quản trị toàn quyền repository, thiết lập Environments, phê duyệt PR chính vào <code>main</code>.</td>
                    </tr>
                    <tr>
                        <td>2</td>
                        <td><strong>Kỳ Nguyễn</strong></td>
                        <td><span class="badge badge-info">Technical Lead</span></td>
                        <td><strong style="color: #0284c7;">Maintain</strong></td>
                        <td>Quản lý review code, kiểm duyệt nhánh <code>test</code>/<code>staging</code>, giải quyết conflict mã nguồn.</td>
                    </tr>
                    <tr>
                        <td>3</td>
                        <td><strong>Trường Nguyễn</strong></td>
                        <td><span class="badge badge-success">Developer</span></td>
                        <td><strong style="color: #16a34a;">Write</strong></td>
                        <td>Lập trình tính năng C# ASP.NET, đẩy code lên nhánh <code>feature/*</code>, tạo Pull Request.</td>
                    </tr>
                    <tr>
                        <td>4</td>
                        <td><strong>Tín Phan</strong></td>
                        <td><span class="badge badge-warning">QA / Tester</span></td>
                        <td><strong style="color: #ca8a04;">Triage / Read</strong></td>
                        <td>Kiểm thử phần mềm, ghi nhận Issue lỗi, xác nhận checklist kiểm thử trước khi release.</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</main>

<script>
    function switchTab(tabId, element) {
        document.querySelectorAll('.tab-content').forEach(function (tab) {
            tab.classList.remove('active');
        });
        document.querySelectorAll('.nav-item').forEach(function (item) {
            item.classList.remove('active');
        });
        var targetTab = document.getElementById(tabId);
        if (targetTab) targetTab.classList.add('active');
        if (element) element.classList.add('active');
    }
</script>

</body>
</html>

