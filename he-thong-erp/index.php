<?php
// ============================================================
// HỆ THỐNG ERP DOANH NGHIỆP - TDSOUTH INFRASTRUCTURE
// ============================================================

$serverName = "localhost\\SQLEXPRESS";
$connectionOptions = array(
    "Database" => "TDSouth",
    "Uid" => "sa",
    "PWD" => "admin@123",
    "TrustServerCertificate" => true
);

$dbConnected = false;
$conn = null;
$dbMessage = "";

// Kết nối SQL Server
if (function_exists('sqlsrv_connect')) {
    $conn = @sqlsrv_connect($serverName, $connectionOptions);
    if ($conn) {
        $dbConnected = true;
        // Tạo bảng ERP_ThietBi nếu chưa có
        $checkTableSql = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ERP_ThietBi' AND xtype='U')
        CREATE TABLE ERP_ThietBi (
            ID INT IDENTITY(1,1) PRIMARY KEY,
            MaTB NVARCHAR(50),
            TenTB NVARCHAR(150),
            LoaiTB NVARCHAR(50),
            SoLuong INT,
            DonGia BIGINT,
            TrangThai NVARCHAR(50),
            CreatedDate DATETIME DEFAULT GETDATE()
        )";
        @sqlsrv_query($conn, $checkTableSql);
    } else {
        $dbMessage = "Không kết nối được SQL Server (Đang chuyển sang chế độ Demo/Offline)";
    }
} else {
    $dbMessage = "PHP chưa nạp extension sqlsrv (Đang chạy ở chế độ Demo/Offline)";
}

// Xử lý thêm thiết bị/vật tư mới
$actionMsg = "";
if ($_SERVER["REQUEST_METHOD"] === "POST" && isset($_POST["btnThemTB"])) {
    $maTB = trim($_POST["matb"] ?? "");
    $tenTB = trim($_POST["tentb"] ?? "");
    $loaiTB = trim($_POST["loaitb"] ?? "");
    $soLuong = (int)($_POST["soluong"] ?? 1);
    $donGia = (int)($_POST["dongia"] ?? 0);
    $trangThai = trim($_POST["trangthai"] ?? "Đang sử dụng");

    if ($dbConnected && $conn) {
        $insertSql = "INSERT INTO ERP_ThietBi (MaTB, TenTB, LoaiTB, SoLuong, DonGia, TrangThai)
                      VALUES (?, ?, ?, ?, ?, ?)";
        $params = array($maTB, $tenTB, $loaiTB, $soLuong, $donGia, $trangThai);
        $stmt = sqlsrv_query($conn, $insertSql, $params);
        if ($stmt) {
            $actionMsg = "Đã thêm thiết bị/vật tư '$tenTB' vào hệ thống CSDL thành công!";
        } else {
            $actionMsg = "Lỗi khi lưu thiết bị vào CSDL!";
        }
    } else {
        $actionMsg = "Đã thêm thiết bị/vật tư '$tenTB' (Chế độ Demo)";
    }
}

// Lấy danh sách thiết bị từ DB
$assetList = [];
if ($dbConnected && $conn) {
    $selectSql = "SELECT ID, MaTB, TenTB, LoaiTB, SoLuong, DonGia, TrangThai FROM ERP_ThietBi ORDER BY ID DESC";
    $result = sqlsrv_query($conn, $selectSql);
    if ($result) {
        while ($row = sqlsrv_fetch_array($result, SQLSRV_FETCH_ASSOC)) {
            $assetList[] = $row;
        }
    }
}

// Dữ liệu thiết bị mẫu của TDSouth Infrastructure Lab
if (empty($assetList)) {
    $assetList = [
        [
            "ID" => 1,
            "MaTB" => "SRV-01",
            "TenTB" => "Dell PowerEdge R740 Rack Server (2x Xeon Gold, 128GB RAM)",
            "LoaiTB" => "Máy Chủ (Server)",
            "SoLuong" => 2,
            "DonGia" => 145000000,
            "TrangThai" => "Đang sử dụng"
        ],
        [
            "ID" => 2,
            "MaTB" => "SW-CORE",
            "TenTB" => "Cisco Catalyst 2960-X 48 Port Gigabit Switch",
            "LoaiTB" => "Thiết Bị Mạng (Switch)",
            "SoLuong" => 4,
            "DonGia" => 28500000,
            "TrangThai" => "Đang sử dụng"
        ],
        [
            "ID" => 3,
            "MaTB" => "FW-01",
            "TenTB" => "Tường Lửa Fortinet FortiGate 60F UTM Appliance",
            "LoaiTB" => "Bảo Mật (Firewall)",
            "SoLuong" => 1,
            "DonGia" => 35000000,
            "TrangThai" => "Đang sử dụng"
        ],
        [
            "ID" => 4,
            "MaTB" => "NAS-01",
            "TenTB" => "Synology DiskStation DS920+ 4-Bay (4x 8TB IronWolf)",
            "LoaiTB" => "Lưu Trữ (Storage)",
            "SoLuong" => 2,
            "DonGia" => 32000000,
            "TrangThai" => "Trong kho"
        ],
        [
            "ID" => 5,
            "MaTB" => "CAB-01",
            "TenTB" => "Thùng Cáp Mạng CommScope Cat6 UTP 305m Chính Hãng",
            "LoaiTB" => "Vật Tư Phụ",
            "SoLuong" => 10,
            "DonGia" => 2800000,
            "TrangThai" => "Trong kho"
        ],
        [
            "ID" => 6,
            "MaTB" => "LIC-01",
            "TenTB" => "Microsoft Windows Server 2022 Datacenter 16-Core License",
            "LoaiTB" => "Bản Quyền (License)",
            "SoLuong" => 3,
            "DonGia" => 42000000,
            "TrangThai" => "Đang sử dụng"
        ]
    ];
}

// Tính tổng giá trị tài sản
$totalAssetValue = 0;
$totalQuantity = 0;
foreach ($assetList as $asset) {
    $totalAssetValue += ($asset["SoLuong"] * $asset["DonGia"]);
    $totalQuantity += $asset["SoLuong"];
}

// Danh sách Dự án Hạ tầng TDSouth
$projects = [
    [
        "code" => "PRJ-AD01",
        "name" => "Triển khai Hệ thống Active Directory Domain Controller & GPO",
        "lead" => "Lê Minh Phúc",
        "progress" => 100,
        "status" => "Hoàn thành",
        "budget" => 45000000
    ],
    [
        "code" => "PRJ-DNS02",
        "name" => "Cấu hình Dịch vụ Mạng DNS & DHCP Failover Lab",
        "lead" => "Kỳ Nguyễn",
        "progress" => 85,
        "status" => "Đang triển khai",
        "budget" => 30000000
    ],
    [
        "code" => "PRJ-IIS03",
        "name" => "Xây dựng IIS Web Server Multi-site & Chứng chỉ SSL/HTTPS",
        "lead" => "Trường Nguyễn",
        "progress" => 90,
        "status" => "Đang hoàn thiện",
        "budget" => 25000000
    ],
    [
        "code" => "PRJ-SEC04",
        "name" => "Kiểm thử An toàn Thông tin & Triển khai FTPS/File Server",
        "lead" => "Tín Phan",
        "progress" => 70,
        "status" => "Đang kiểm thử",
        "budget" => 20000000
    ]
];
?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Hệ Thống ERP | TDSouth Infrastructure</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet">
    <style>
        :root {
            --primary: #0f172a;
            --primary-accent: #2563eb;
            --primary-light: #eff6ff;
            --sidebar-bg: #1e293b;
            --sidebar-hover: #334155;
            --sidebar-text: #e2e8f0;
            --bg: #f8fafc;
            --card-bg: #ffffff;
            --text-main: #0f172a;
            --text-muted: #64748b;
            --border: #e2e8f0;
            --success: #10b981;
            --warning: #f59e0b;
            --danger: #ef4444;
            --radius: 12px;
            --shadow: 0 4px 6px -1px rgb(0 0 0 / 0.08), 0 2px 4px -2px rgb(0 0 0 / 0.05);
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
            display: flex;
        }

        /* Sidebar */
        aside {
            width: 270px;
            background-color: var(--sidebar-bg);
            color: var(--sidebar-text);
            padding: 24px 18px;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            flex-shrink: 0;
        }

        .brand-box {
            padding-bottom: 24px;
            border-bottom: 1px solid rgba(255, 255, 255, 0.1);
            margin-bottom: 20px;
        }

        .brand-box h2 {
            font-size: 1.35rem;
            font-weight: 800;
            color: #ffffff;
            letter-spacing: 0.5px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .brand-box p {
            font-size: 0.8rem;
            color: #94a3b8;
            margin-top: 4px;
        }

        .nav-menu {
            list-style: none;
            display: flex;
            flex-direction: column;
            gap: 6px;
        }

        .nav-item {
            padding: 12px 16px;
            border-radius: 8px;
            cursor: pointer;
            font-size: 0.92rem;
            font-weight: 500;
            display: flex;
            align-items: center;
            gap: 12px;
            transition: all 0.2s;
            color: #cbd5e1;
        }

        .nav-item:hover {
            background-color: var(--sidebar-hover);
            color: #ffffff;
        }

        .nav-item.active {
            background-color: var(--primary-accent);
            color: #ffffff;
            font-weight: 600;
        }

        .sidebar-footer {
            padding-top: 20px;
            border-top: 1px solid rgba(255, 255, 255, 0.1);
            font-size: 0.8rem;
            color: #94a3b8;
        }

        .sidebar-btn {
            display: block;
            text-align: center;
            padding: 10px;
            background: rgba(255, 255, 255, 0.08);
            color: white;
            text-decoration: none;
            border-radius: 6px;
            margin-top: 10px;
            transition: 0.2s;
        }
        .sidebar-btn:hover {
            background: rgba(255, 255, 255, 0.18);
        }

        /* Main Content */
        main {
            flex-grow: 1;
            padding: 28px 36px;
            overflow-y: auto;
        }

        /* Top Nav Header */
        .topbar {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 28px;
            flex-wrap: wrap;
            gap: 16px;
        }

        .topbar-title h1 {
            font-size: 1.65rem;
            font-weight: 800;
            color: var(--text-main);
        }

        .topbar-title p {
            font-size: 0.88rem;
            color: var(--text-muted);
            margin-top: 2px;
        }

        .topbar-actions {
            display: flex;
            gap: 12px;
            align-items: center;
        }

        .btn {
            padding: 10px 18px;
            border-radius: 8px;
            font-size: 0.9rem;
            font-weight: 600;
            border: none;
            cursor: pointer;
            transition: all 0.2s;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            text-decoration: none;
        }

        .btn-primary {
            background-color: var(--primary-accent);
            color: white;
        }
        .btn-primary:hover {
            background-color: #1d4ed8;
        }

        .btn-outline {
            background-color: white;
            border: 1px solid var(--border);
            color: var(--text-main);
        }
        .btn-outline:hover {
            background-color: #f1f5f9;
        }

        /* DB Status bar */
        .db-status-bar {
            display: flex;
            align-items: center;
            justify-content: space-between;
            background: white;
            padding: 12px 20px;
            border-radius: 8px;
            margin-bottom: 24px;
            border: 1px solid var(--border);
            font-size: 0.88rem;
        }

        .status-dot {
            display: inline-block;
            width: 10px;
            height: 10px;
            border-radius: 50%;
            margin-right: 8px;
        }
        .status-online { background-color: var(--success); }
        .status-offline { background-color: var(--warning); }

        /* KPI Stat Cards */
        .kpi-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(230px, 1fr));
            gap: 20px;
            margin-bottom: 28px;
        }

        .kpi-card {
            background: var(--card-bg);
            padding: 22px;
            border-radius: var(--radius);
            box-shadow: var(--shadow);
            border: 1px solid var(--border);
            position: relative;
            overflow: hidden;
        }

        .kpi-card::after {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 4px;
        }

        .kpi-blue::after { background-color: #2563eb; }
        .kpi-green::after { background-color: #10b981; }
        .kpi-amber::after { background-color: #f59e0b; }
        .kpi-purple::after { background-color: #8b5cf6; }

        .kpi-label {
            font-size: 0.85rem;
            font-weight: 600;
            color: var(--text-muted);
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .kpi-value {
            font-size: 1.8rem;
            font-weight: 800;
            color: var(--text-main);
            margin: 8px 0 4px 0;
        }

        .kpi-sub {
            font-size: 0.8rem;
            color: var(--text-muted);
        }

        /* Card Section */
        .card {
            background: var(--card-bg);
            border-radius: var(--radius);
            box-shadow: var(--shadow);
            border: 1px solid var(--border);
            padding: 24px;
            margin-bottom: 28px;
        }

        .card-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            padding-bottom: 14px;
            border-bottom: 1px solid var(--border);
            flex-wrap: wrap;
            gap: 12px;
        }

        .card-title {
            font-size: 1.2rem;
            font-weight: 700;
            color: var(--text-main);
            display: flex;
            align-items: center;
            gap: 10px;
        }

        /* Table */
        .table-responsive {
            overflow-x: auto;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            font-size: 0.9rem;
            text-align: left;
        }

        th {
            background-color: #f8fafc;
            color: #475569;
            font-weight: 600;
            padding: 12px 16px;
            border-bottom: 1px solid var(--border);
            white-space: nowrap;
        }

        td {
            padding: 14px 16px;
            border-bottom: 1px solid var(--border);
            color: #1e293b;
        }

        tr:last-child td {
            border-bottom: none;
        }

        tr:hover td {
            background-color: #f8fafc;
        }

        /* Badges */
        .badge {
            display: inline-block;
            padding: 4px 10px;
            border-radius: 9999px;
            font-size: 0.76rem;
            font-weight: 600;
        }

        .badge-success { background: #dcfce7; color: #15803d; }
        .badge-warning { background: #fef3c7; color: #b45309; }
        .badge-info { background: #e0f2fe; color: #0369a1; }
        .badge-purple { background: #f3e8ff; color: #7e22ce; }

        /* Progress Bar */
        .progress-bar-bg {
            background: #e2e8f0;
            border-radius: 999px;
            height: 8px;
            width: 120px;
            overflow: hidden;
            display: inline-block;
            vertical-align: middle;
            margin-right: 8px;
        }

        .progress-bar-fill {
            height: 100%;
            border-radius: 999px;
            background: var(--primary-accent);
        }

        /* Forms */
        .form-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 16px;
            margin-bottom: 16px;
        }

        .form-group label {
            display: block;
            font-size: 0.85rem;
            font-weight: 600;
            margin-bottom: 6px;
            color: #334155;
        }

        .form-control {
            width: 100%;
            padding: 9px 12px;
            border-radius: 8px;
            border: 1px solid var(--border);
            font-size: 0.9rem;
            outline: none;
        }

        .form-control:focus {
            border-color: var(--primary-accent);
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15);
        }

        /* Tabs display */
        .tab-content {
            display: none;
        }
        .tab-content.active {
            display: block;
        }

        /* Alert */
        .alert {
            padding: 14px 18px;
            border-radius: 8px;
            margin-bottom: 20px;
            font-size: 0.92rem;
            background-color: #ecfdf5;
            color: #065f46;
            border: 1px solid #a7f3d0;
        }

        @media (max-width: 900px) {
            body {
                flex-direction: column;
            }
            aside {
                width: 100%;
            }
        }
    </style>
</head>
<body>

<!-- Sidebar Navigation -->
<aside>
    <div>
        <div class="brand-box">
            <h2>🏢 TDSOUTH ERP</h2>
            <p>Hệ Thống Quản Trị Tổng Thể Lab &bull; v1.0</p>
        </div>

        <ul class="nav-menu">
            <li class="nav-item active" onclick="switchTab('tab-overview', this)">
                📊 Tổng Quan Điều Hành
            </li>
            <li class="nav-item" onclick="switchTab('tab-inventory', this)">
                📦 Vật Tư & Thiết Bị IT
            </li>
            <li class="nav-item" onclick="switchTab('tab-projects', this)">
                🚀 Dự Án Hạ Tầng
            </li>
            <li class="nav-item" onclick="switchTab('tab-hr', this)">
                👥 Nhân Sự & Quyền Hạn
            </li>
        </ul>
    </div>

    <div class="sidebar-footer">
        <p><strong>Cơ sở hạ tầng TDSouth</strong></p>
        <p>IIS &bull; SQL Server &bull; AD DS</p>
        <a href="../he-thong-cham-cong/" class="sidebar-btn">⏱️ Sang Hệ Thống Chấm Công</a>
        <a href="../" class="sidebar-btn">🏠 Trang Quản Lý Chính</a>
    </div>
</aside>

<!-- Main Workspace -->
<main>
    <!-- Topbar -->
    <div class="topbar">
        <div class="topbar-title">
            <h1 id="pageTitle">Hệ Thống Quản Trị Tổng Thể Doanh Nghiệp (ERP)</h1>
            <p>Phòng thí nghiệm Quản trị Hạ tầng & Công nghệ Thông tin TDSouth</p>
        </div>
        <div class="topbar-actions">
            <a href="javascript:window.print()" class="btn btn-outline">🖨️ Xuất Báo Cáo</a>
            <button class="btn btn-primary" onclick="switchTab('tab-inventory', document.querySelectorAll('.nav-item')[1])">➕ Nhập Vật Tư Mới</button>
        </div>
    </div>

    <!-- Status Alert -->
    <div class="db-status-bar">
        <div>
            <?php if ($dbConnected): ?>
                <span class="status-dot status-online"></span>
                <strong>Trạng thái CSDL:</strong> Đã kết nối cơ sở dữ liệu <code>SQL Server: TDSouth (SQLEXPRESS)</code>
            <?php else: ?>
                <span class="status-dot status-offline"></span>
                <strong>Trạng thái CSDL:</strong> <?php echo htmlspecialchars($dbMessage); ?>
            <?php endif; ?>
        </div>
        <div>
            <span>Máy chủ ERP hoạt động: <strong><?php echo date("d/m/Y H:i:s"); ?></strong></span>
        </div>
    </div>

    <?php if (!empty($actionMsg)): ?>
        <div class="alert">
            ✅ <?php echo htmlspecialchars($actionMsg); ?>
        </div>
    <?php endif; ?>

    <!-- KPI Cards -->
    <div class="kpi-grid">
        <div class="kpi-card kpi-blue">
            <div class="kpi-label">Tổng Giá Trị Tài Sản IT</div>
            <div class="kpi-value"><?php echo number_format($totalAssetValue / 1000000, 1); ?> tr ₫</div>
            <div class="kpi-sub"><?php echo count($assetList); ?> chủng loại thiết bị (Tổng <?php echo $totalQuantity; ?> món)</div>
        </div>
        <div class="kpi-card kpi-green">
            <div class="kpi-label">Dự Án Hạ Tầng Đang Chạy</div>
            <div class="kpi-value"><?php echo count($projects); ?> Dự án</div>
            <div class="kpi-sub">Tiến độ trung bình đạt 86%</div>
        </div>
        <div class="kpi-card kpi-amber">
            <div class="kpi-label">Nhân Sự Quản Trị Phụ Trách</div>
            <div class="kpi-value">4 Thành viên</div>
            <div class="kpi-sub">Theo ma trận COLLABORATORS.md</div>
        </div>
        <div class="kpi-card kpi-purple">
            <div class="kpi-label">Độ Sẵn Sàng Hệ Thống (SLA)</div>
            <div class="kpi-value">99.98%</div>
            <div class="kpi-sub">Active Directory, DNS, IIS Web, FTPS</div>
        </div>
    </div>

    <!-- TAB 1: TỔNG QUAN (OVERVIEW) -->
    <div id="tab-overview" class="tab-content active">
        <!-- Dự án nổi bật -->
        <div class="card">
            <div class="card-header">
                <h2 class="card-title">🚀 Tiến Độ Các Dự Án Hạ Tầng Trọng Điểm</h2>
                <span style="font-size: 0.85rem; color: var(--text-muted);">Cập nhật Quý 4/2026</span>
            </div>
            <div class="table-responsive">
                <table>
                    <thead>
                        <tr>
                            <th>Mã Dự Án</th>
                            <th>Tên Dự Án</th>
                            <th>Phụ Trách (Lead)</th>
                            <th>Ngân Sách Triển Khai</th>
                            <th>Tiến Độ Hoàn Thành</th>
                            <th>Trạng Thái</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($projects as $pj): ?>
                        <tr>
                            <td><strong><?php echo htmlspecialchars($pj["code"]); ?></strong></td>
                            <td><?php echo htmlspecialchars($pj["name"]); ?></td>
                            <td><?php echo htmlspecialchars($pj["lead"]); ?></td>
                            <td><?php echo number_format($pj["budget"]); ?> ₫</td>
                            <td>
                                <div class="progress-bar-bg">
                                    <div class="progress-bar-fill" style="width: <?php echo $pj["progress"]; ?>%;"></div>
                                </div>
                                <span style="font-weight: 600; font-size: 0.85rem;"><?php echo $pj["progress"]; ?>%</span>
                            </td>
                            <td>
                                <?php if ($pj["status"] === "Hoàn thành"): ?>
                                    <span class="badge badge-success">Hoàn thành</span>
                                <?php elseif ($pj["status"] === "Đang triển khai"): ?>
                                    <span class="badge badge-info">Đang triển khai</span>
                                <?php else: ?>
                                    <span class="badge badge-warning"><?php echo htmlspecialchars($pj["status"]); ?></span>
                                <?php endif; ?>
                            </td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
            </div>
        </div>

        <!-- Thiết bị IT tiêu biểu -->
        <div class="card">
            <div class="card-header">
                <h2 class="card-title">💻 Thiết Bị Hạ Tầng Server & Network</h2>
                <button class="btn btn-outline" style="padding: 6px 12px; font-size: 0.8rem;" onclick="switchTab('tab-inventory', document.querySelectorAll('.nav-item')[1])">Xem toàn bộ kho</button>
            </div>
            <div class="table-responsive">
                <table>
                    <thead>
                        <tr>
                            <th>Mã TB</th>
                            <th>Tên Thiết Bị / Vật Tư</th>
                            <th>Phân Loại</th>
                            <th>Số Lượng</th>
                            <th>Đơn Giá</th>
                            <th>Thành Tiền</th>
                            <th>Tình Trạng</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach (array_slice($assetList, 0, 4) as $item): ?>
                        <tr>
                            <td><strong><?php echo htmlspecialchars($item["MaTB"]); ?></strong></td>
                            <td><?php echo htmlspecialchars($item["TenTB"]); ?></td>
                            <td><span class="badge badge-info"><?php echo htmlspecialchars($item["LoaiTB"]); ?></span></td>
                            <td><strong><?php echo $item["SoLuong"]; ?></strong></td>
                            <td><?php echo number_format($item["DonGia"]); ?> ₫</td>
                            <td style="font-weight: 600; color: #1e40af;"><?php echo number_format($item["SoLuong"] * $item["DonGia"]); ?> ₫</td>
                            <td>
                                <?php if ($item["TrangThai"] === "Đang sử dụng"): ?>
                                    <span class="badge badge-success">Đang sử dụng</span>
                                <?php else: ?>
                                    <span class="badge badge-warning"><?php echo htmlspecialchars($item["TrangThai"]); ?></span>
                                <?php endif; ?>
                            </td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- TAB 2: QUẢN LÝ VẬT TƯ & THIẾT BỊ (INVENTORY) -->
    <div id="tab-inventory" class="tab-content">
        <!-- Form nhập vật tư mới -->
        <div class="card">
            <div class="card-header">
                <h2 class="card-title">➕ Nhập Thiết Bị / Vật Tư Mới Vào CSDL</h2>
            </div>
            <form method="POST" action="">
                <div class="form-row">
                    <div class="form-group">
                        <label for="matb">Mã Thiết Bị *</label>
                        <input type="text" id="matb" name="matb" class="form-control" placeholder="VD: SRV-02, SW-03" required>
                    </div>
                    <div class="form-group" style="grid-column: span 2;">
                        <label for="tentb">Tên Thiết Bị / Chi Tiết Cấu Hình *</label>
                        <input type="text" id="tentb" name="tentb" class="form-control" placeholder="VD: Máy chủ Dell PowerEdge R640 64GB RAM" required>
                    </div>
                    <div class="form-group">
                        <label for="loaitb">Phân Loại *</label>
                        <select id="loaitb" name="loaitb" class="form-control" required>
                            <option value="Máy Chủ (Server)">Máy Chủ (Server)</option>
                            <option value="Thiết Bị Mạng (Switch)">Thiết Bị Mạng (Switch)</option>
                            <option value="Bảo Mật (Firewall)">Bảo Mật (Firewall)</option>
                            <option value="Lưu Trữ (Storage)">Lưu Trữ (Storage)</option>
                            <option value="Vật Tư Phụ">Vật Tư Phụ</option>
                            <option value="Bản Quyền (License)">Bản Quyền (License)</option>
                        </select>
                    </div>
                </div>

                <div class="form-row">
                    <div class="form-group">
                        <label for="soluong">Số Lượng *</label>
                        <input type="number" id="soluong" name="soluong" class="form-control" value="1" min="1" required>
                    </div>
                    <div class="form-group">
                        <label for="dongia">Đơn Giá Dự Toán (VNĐ) *</label>
                        <input type="number" id="dongia" name="dongia" class="form-control" value="10000000" step="500000" required>
                    </div>
                    <div class="form-group">
                        <label for="trangthai">Trạng Thái *</label>
                        <select id="trangthai" name="trangthai" class="form-control" required>
                            <option value="Đang sử dụng">Đang sử dụng</option>
                            <option value="Trong kho">Trong kho</option>
                            <option value="Đang bảo trì">Đang bảo trì</option>
                        </select>
                    </div>
                    <div class="form-group" style="display: flex; align-items: flex-end;">
                        <button type="submit" name="btnThemTB" class="btn btn-primary" style="width: 100%;">
                            💾 Lưu Vào Kho CSDL
                        </button>
                    </div>
                </div>
            </form>
        </div>

        <!-- Bảng danh sách vật tư đầy đủ -->
        <div class="card">
            <div class="card-header">
                <h2 class="card-title">📦 Danh Mục Toàn Bộ Kho Thiết Bị IT & Vật Tư</h2>
                <div style="font-size: 0.9rem; font-weight: 700; color: #1e40af;">
                    Tổng ngân sách: <?php echo number_format($totalAssetValue); ?> ₫
                </div>
            </div>
            <div class="table-responsive">
                <table>
                    <thead>
                        <tr>
                            <th>Mã TB</th>
                            <th>Tên Thiết Bị / Chi Tiết</th>
                            <th>Phân Loại</th>
                            <th>Số Lượng</th>
                            <th>Đơn Giá</th>
                            <th>Thành Tiền</th>
                            <th>Trạng Thái</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($assetList as $item): ?>
                        <tr>
                            <td><strong><?php echo htmlspecialchars($item["MaTB"]); ?></strong></td>
                            <td><?php echo htmlspecialchars($item["TenTB"]); ?></td>
                            <td><span class="badge badge-info"><?php echo htmlspecialchars($item["LoaiTB"]); ?></span></td>
                            <td><strong><?php echo $item["SoLuong"]; ?></strong></td>
                            <td><?php echo number_format($item["DonGia"]); ?> ₫</td>
                            <td style="font-weight: 600; color: #1e40af;"><?php echo number_format($item["SoLuong"] * $item["DonGia"]); ?> ₫</td>
                            <td>
                                <?php if ($item["TrangThai"] === "Đang sử dụng"): ?>
                                    <span class="badge badge-success">Đang sử dụng</span>
                                <?php elseif ($item["TrangThai"] === "Trong kho"): ?>
                                    <span class="badge badge-warning">Trong kho</span>
                                <?php else: ?>
                                    <span class="badge badge-purple"><?php echo htmlspecialchars($item["TrangThai"]); ?></span>
                                <?php endif; ?>
                            </td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- TAB 3: DỰ ÁN HẠ TẦNG (PROJECTS) -->
    <div id="tab-projects" class="tab-content">
        <div class="card">
            <div class="card-header">
                <h2 class="card-title">🚀 Quản Lý Danh Mục Dự Án Triển Khai IT Lab</h2>
            </div>
            <div class="table-responsive">
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
                        <?php foreach ($projects as $pj): ?>
                        <tr>
                            <td><strong><?php echo htmlspecialchars($pj["code"]); ?></strong></td>
                            <td style="font-weight: 600;"><?php echo htmlspecialchars($pj["name"]); ?></td>
                            <td><?php echo htmlspecialchars($pj["lead"]); ?></td>
                            <td><?php echo number_format($pj["budget"]); ?> ₫</td>
                            <td>
                                <div class="progress-bar-bg" style="width: 140px;">
                                    <div class="progress-bar-fill" style="width: <?php echo $pj["progress"]; ?>%;"></div>
                                </div>
                                <span style="font-weight: 700; font-size: 0.9rem;"><?php echo $pj["progress"]; ?>%</span>
                            </td>
                            <td>
                                <?php if ($pj["status"] === "Hoàn thành"): ?>
                                    <span class="badge badge-success">Hoàn thành</span>
                                <?php elseif ($pj["status"] === "Đang triển khai"): ?>
                                    <span class="badge badge-info">Đang triển khai</span>
                                <?php else: ?>
                                    <span class="badge badge-warning"><?php echo htmlspecialchars($pj["status"]); ?></span>
                                <?php endif; ?>
                            </td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- TAB 4: NHÂN SỰ & QUYỀN HẠN (HR MATRIX) -->
    <div id="tab-hr" class="tab-content">
        <div class="card">
            <div class="card-header">
                <h2 class="card-title">👥 Ma Trận Phân Quyền Thành Viên Dự Án (Project Access Matrix)</h2>
                <span style="font-size: 0.85rem; color: var(--text-muted);">Trích xuất theo COLLABORATORS.md</span>
            </div>
            <div class="table-responsive">
                <table>
                    <thead>
                        <tr>
                            <th>STT</th>
                            <th>Họ và Tên</th>
                            <th>Vai Trò Dự Án</th>
                            <th>Quyền Hạn Hệ Thống</th>
                            <th>Mô Tả Trách Nhiệm & Phạm Vi</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>1</td>
                            <td><strong>Lê Minh Phúc</strong></td>
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
                            <td>Lập trình tính năng, đẩy code lên nhánh <code>feature/*</code>, tạo Pull Request.</td>
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
    </div>

</main>

<script>
    function switchTab(tabId, element) {
        document.querySelectorAll('.tab-content').forEach(tab => {
            tab.classList.remove('active');
        });
        document.querySelectorAll('.nav-item').forEach(item => {
            item.classList.remove('active');
        });

        const targetTab = document.getElementById(tabId);
        if (targetTab) {
            targetTab.classList.add('active');
        }
        if (element) {
            element.classList.add('active');
        }
    }
</script>

</body>
</html>

