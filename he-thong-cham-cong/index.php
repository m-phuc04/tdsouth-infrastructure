<?php
// ============================================================
// HỆ THỐNG CHẤM CÔNG NHÂN VIÊN - TDSOUTH INFRASTRUCTURE
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

// Thử kết nối SQL Server nếu có extension sqlsrv
if (function_exists('sqlsrv_connect')) {
    $conn = @sqlsrv_connect($serverName, $connectionOptions);
    if ($conn) {
        $dbConnected = true;
        // Kiểm tra và tạo bảng ChamCong nếu chưa tồn tại
        $checkTableSql = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChamCong' AND xtype='U')
        CREATE TABLE ChamCong (
            ID INT IDENTITY(1,1) PRIMARY KEY,
            MaNV NVARCHAR(50),
            HoTen NVARCHAR(100),
            PhongBan NVARCHAR(100),
            CaLamViec NVARCHAR(50),
            GioVao NVARCHAR(50),
            GioRa NVARCHAR(50),
            TrangThai NVARCHAR(50),
            NgayCham NVARCHAR(50),
            CreatedDate DATETIME DEFAULT GETDATE()
        )";
        @sqlsrv_query($conn, $checkTableSql);
    } else {
        $dbMessage = "Không kết nối được SQL Server (Đang chuyển sang chế độ Demo/Offline)";
    }
} else {
    $dbMessage = "PHP chưa nạp extension sqlsrv (Đang chạy ở chế độ Demo/Offline)";
}

// Xử lý gửi Form Chấm Công qua PHP khi có DB
$actionMsg = "";
if ($_SERVER["REQUEST_METHOD"] === "POST" && isset($_POST["action"])) {
    $maNV = trim($_POST["manv"] ?? "");
    $hoTen = trim($_POST["hoten"] ?? "");
    $phongBan = trim($_POST["phongban"] ?? "");
    $caLamViec = trim($_POST["calamviec"] ?? "");
    $actionType = $_POST["action"]; // 'checkin' hoặc 'checkout'
    $currentTime = date("H:i:s");
    $currentDate = date("d/m/Y");

    if ($actionType === "checkin") {
        // Kiểm tra đúng giờ hay đi trễ (quy định mốc 08:15)
        $hour = (int)date("H");
        $minute = (int)date("i");
        $trangThai = ($hour < 8 || ($hour == 8 && $minute <= 15)) ? "Đúng giờ" : "Đi muộn";

        if ($dbConnected && $conn) {
            $insertSql = "INSERT INTO ChamCong (MaNV, HoTen, PhongBan, CaLamViec, GioVao, GioRa, TrangThai, NgayCham)
                          VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
            $params = array($maNV, $hoTen, $phongBan, $caLamViec, $currentTime, "--:--", $trangThai, $currentDate);
            $stmt = sqlsrv_query($conn, $insertSql, $params);
            if ($stmt) {
                $actionMsg = "Check-in thành công cho nhân viên $hoTen lúc $currentTime ($trangThai)!";
            } else {
                $actionMsg = "Lỗi khi lưu check-in vào CSDL!";
            }
        } else {
            $actionMsg = "Check-in thành công (Demo): $hoTen lúc $currentTime ($trangThai)";
        }
    } elseif ($actionType === "checkout") {
        if ($dbConnected && $conn) {
            // Cập nhật giờ ra cho bản ghi mới nhất trong ngày của NV này
            $updateSql = "UPDATE ChamCong SET GioRa = ? WHERE ID = (
                            SELECT TOP 1 ID FROM ChamCong 
                            WHERE MaNV = ? AND NgayCham = ? 
                            ORDER BY ID DESC
                          )";
            $params = array($currentTime, $maNV, $currentDate);
            $stmt = sqlsrv_query($conn, $updateSql, $params);
            if ($stmt) {
                $actionMsg = "Check-out thành công cho nhân viên $hoTen lúc $currentTime!";
            } else {
                $actionMsg = "Lỗi khi cập nhật check-out!";
            }
        } else {
            $actionMsg = "Check-out thành công (Demo): $hoTen lúc $currentTime";
        }
    }
}

// Lấy danh sách chấm công từ DB nếu có
$attendanceList = [];
if ($dbConnected && $conn) {
    $selectSql = "SELECT TOP 50 ID, MaNV, HoTen, PhongBan, CaLamViec, GioVao, GioRa, TrangThai, NgayCham 
                  FROM ChamCong ORDER BY ID DESC";
    $result = sqlsrv_query($conn, $selectSql);
    if ($result) {
        while ($row = sqlsrv_fetch_array($result, SQLSRV_FETCH_ASSOC)) {
            $attendanceList[] = $row;
        }
    }
}

// Nếu không có dữ liệu từ DB, dùng danh sách mẫu thực tế theo COLLABORATORS.md
if (empty($attendanceList)) {
    $attendanceList = [
        [
            "ID" => 1,
            "MaNV" => "TDS001",
            "HoTen" => "Lê Minh Phúc",
            "PhongBan" => "Quản trị Hệ thống (Admin)",
            "CaLamViec" => "Ca Hành Chính (08:00 - 17:30)",
            "GioVao" => "07:55:12",
            "GioRa" => "17:35:40",
            "TrangThai" => "Đúng giờ",
            "NgayCham" => date("d/m/Y")
        ],
        [
            "ID" => 2,
            "MaNV" => "TDS002",
            "HoTen" => "Kỳ Nguyễn",
            "PhongBan" => "Kỹ thuật Hạ tầng (Tech Lead)",
            "CaLamViec" => "Ca Hành Chính (08:00 - 17:30)",
            "GioVao" => "08:04:20",
            "GioRa" => "17:30:10",
            "TrangThai" => "Đúng giờ",
            "NgayCham" => date("d/m/Y")
        ],
        [
            "ID" => 3,
            "MaNV" => "TDS003",
            "HoTen" => "Trường Nguyễn",
            "PhongBan" => "Phát triển Ứng dụng (Dev)",
            "CaLamViec" => "Ca Sáng (08:00 - 12:00)",
            "GioVao" => "08:18:45",
            "GioRa" => "12:05:00",
            "TrangThai" => "Đi muộn",
            "NgayCham" => date("d/m/Y")
        ],
        [
            "ID" => 4,
            "MaNV" => "TDS004",
            "HoTen" => "Tín Phan",
            "PhongBan" => "Kiểm thử Chất lượng (QA)",
            "CaLamViec" => "Ca Hành Chính (08:00 - 17:30)",
            "GioVao" => "08:00:05",
            "GioRa" => "--:--",
            "TrangThai" => "Đúng giờ",
            "NgayCham" => date("d/m/Y")
        ]
    ];
}

// Thống kê nhanh
$totalRecords = count($attendanceList);
$onTimeCount = 0;
$lateCount = 0;
$currentlyWorking = 0;
foreach ($attendanceList as $item) {
    if ($item["TrangThai"] === "Đúng giờ") $onTimeCount++;
    if ($item["TrangThai"] === "Đi muộn") $lateCount++;
    if ($item["GioRa"] === "--:--" || empty($item["GioRa"])) $currentlyWorking++;
}
?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Hệ Thống Chấm Công | TDSouth Infrastructure</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
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
            --shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1);
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
            padding: 24px;
            min-height: 100vh;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
        }

        /* Top Header */
        header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            background: linear-gradient(135deg, #1e3a8a 0%, #2563eb 100%);
            color: white;
            padding: 24px 32px;
            border-radius: var(--radius);
            box-shadow: var(--shadow);
            margin-bottom: 24px;
            flex-wrap: wrap;
            gap: 16px;
        }

        .header-brand h1 {
            font-size: 1.6rem;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .header-brand p {
            color: #93c5fd;
            font-size: 0.9rem;
            margin-top: 4px;
        }

        .live-clock-box {
            text-align: right;
            background: rgba(255, 255, 255, 0.12);
            padding: 12px 20px;
            border-radius: 10px;
            backdrop-filter: blur(8px);
            border: 1px solid rgba(255, 255, 255, 0.2);
        }

        .live-clock {
            font-size: 1.8rem;
            font-weight: 700;
            letter-spacing: 1px;
            color: #ffffff;
            font-variant-numeric: tabular-nums;
        }

        .live-date {
            font-size: 0.85rem;
            color: #cbd5e1;
            margin-top: 2px;
        }

        /* Status Badge for DB */
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

        .status-online { background-color: #10b981; }
        .status-offline { background-color: #f59e0b; }

        /* KPI Dashboard Stats */
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
            gap: 18px;
            margin-bottom: 24px;
        }

        .stat-card {
            background: var(--card-bg);
            padding: 20px;
            border-radius: var(--radius);
            box-shadow: var(--shadow);
            border: 1px solid var(--border);
            display: flex;
            align-items: center;
            gap: 16px;
        }

        .stat-icon {
            width: 52px;
            height: 52px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.6rem;
        }

        .stat-card.blue .stat-icon { background: #dbeafe; color: #1d4ed8; }
        .stat-card.green .stat-icon { background: #dcfce7; color: #15803d; }
        .stat-card.amber .stat-icon { background: #fef3c7; color: #b45309; }
        .stat-card.purple .stat-icon { background: #f3e8ff; color: #7e22ce; }

        .stat-info h3 {
            font-size: 1.7rem;
            font-weight: 700;
            color: var(--text-main);
        }

        .stat-info p {
            font-size: 0.85rem;
            color: var(--text-muted);
            margin-top: 2px;
        }

        /* Main Content Layout */
        .main-grid {
            display: grid;
            grid-template-columns: 380px 1fr;
            gap: 24px;
        }

        @media (max-width: 992px) {
            .main-grid {
                grid-template-columns: 1fr;
            }
        }

        /* Form Card */
        .card {
            background: var(--card-bg);
            border-radius: var(--radius);
            box-shadow: var(--shadow);
            border: 1px solid var(--border);
            padding: 24px;
        }

        .card-title {
            font-size: 1.15rem;
            font-weight: 700;
            margin-bottom: 18px;
            color: var(--text-main);
            display: flex;
            align-items: center;
            gap: 8px;
            padding-bottom: 12px;
            border-bottom: 1px solid var(--border);
        }

        .form-group {
            margin-bottom: 16px;
        }

        .form-group label {
            display: block;
            font-size: 0.88rem;
            font-weight: 600;
            margin-bottom: 6px;
            color: #334155;
        }

        .form-control {
            width: 100%;
            padding: 10px 14px;
            border-radius: 8px;
            border: 1px solid var(--border);
            font-size: 0.92rem;
            outline: none;
            transition: all 0.2s;
            background: #fff;
        }

        .form-control:focus {
            border-color: var(--primary);
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15);
        }

        .btn-group {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 12px;
            margin-top: 22px;
        }

        .btn {
            padding: 12px 18px;
            border-radius: 8px;
            border: none;
            font-size: 0.95rem;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
        }

        .btn-checkin {
            background-color: var(--success);
            color: white;
        }
        .btn-checkin:hover {
            background-color: #047857;
            transform: translateY(-1px);
        }

        .btn-checkout {
            background-color: var(--danger);
            color: white;
        }
        .btn-checkout:hover {
            background-color: #b91c1c;
            transform: translateY(-1px);
        }

        .quick-select-box {
            margin-top: 18px;
            padding: 14px;
            background: #f8fafc;
            border-radius: 8px;
            border: 1px dashed #cbd5e1;
        }

        .quick-select-title {
            font-size: 0.8rem;
            font-weight: 600;
            color: var(--text-muted);
            margin-bottom: 8px;
            text-transform: uppercase;
        }

        .quick-badges {
            display: flex;
            flex-wrap: wrap;
            gap: 6px;
        }

        .quick-btn {
            background: white;
            border: 1px solid #cbd5e1;
            padding: 5px 10px;
            border-radius: 6px;
            font-size: 0.8rem;
            cursor: pointer;
            color: #334155;
            transition: all 0.15s;
        }

        .quick-btn:hover {
            background: var(--primary-light);
            border-color: var(--primary);
            color: var(--primary);
        }

        /* Table Card */
        .table-toolbar {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 16px;
            flex-wrap: wrap;
            gap: 12px;
        }

        .search-box {
            display: flex;
            gap: 8px;
            align-items: center;
        }

        .table-container {
            overflow-x: auto;
            border: 1px solid var(--border);
            border-radius: 8px;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            text-align: left;
            font-size: 0.9rem;
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
            padding: 12px 16px;
            border-bottom: 1px solid var(--border);
            color: #1e293b;
        }

        tr:last-child td {
            border-bottom: none;
        }

        tr:hover td {
            background-color: #f8fafc;
        }

        .badge {
            display: inline-block;
            padding: 4px 10px;
            border-radius: 9999px;
            font-size: 0.78rem;
            font-weight: 600;
        }

        .badge-ontime {
            background-color: #dcfce7;
            color: #15803d;
        }

        .badge-late {
            background-color: #fef3c7;
            color: #b45309;
        }

        .badge-working {
            background-color: #e0f2fe;
            color: #0369a1;
        }

        /* Alerts */
        .alert {
            padding: 14px 18px;
            border-radius: 8px;
            margin-bottom: 20px;
            font-size: 0.92rem;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .alert-success {
            background-color: #ecfdf5;
            color: #065f46;
            border: 1px solid #a7f3d0;
        }

        /* Footer */
        footer {
            margin-top: 36px;
            text-align: center;
            font-size: 0.85rem;
            color: var(--text-muted);
            padding: 16px;
        }

        .nav-links {
            display: flex;
            gap: 12px;
            align-items: center;
        }

        .nav-btn {
            background: white;
            color: #1e40af;
            padding: 8px 14px;
            border-radius: 6px;
            text-decoration: none;
            font-weight: 600;
            font-size: 0.85rem;
            border: 1px solid var(--border);
        }
        .nav-btn:hover {
            background: #f1f5f9;
        }
    </style>
</head>
<body>

<div class="container">
    <!-- Header -->
    <header>
        <div class="header-brand">
            <h1>⏱️ HỆ THỐNG CHẤM CÔNG</h1>
            <p>TDSouth Infrastructure Lab &bull; IIS Web Server &bull; SQL Server Database</p>
        </div>
        <div style="display: flex; align-items: center; gap: 16px; flex-wrap: wrap;">
            <div class="nav-links">
                <a href="../he-thong-erp/" class="nav-btn">🏢 Chuyển sang ERP</a>
                <a href="../" class="nav-btn">🏠 Trang chủ</a>
            </div>
            <div class="live-clock-box">
                <div class="live-clock" id="liveClock">--:--:--</div>
                <div class="live-date" id="liveDate">Đang tải ngày...</div>
            </div>
        </div>
    </header>

    <!-- DB Status Alert -->
    <div class="db-status-bar">
        <div>
            <?php if ($dbConnected): ?>
                <span class="status-dot status-online"></span>
                <strong>Trạng thái:</strong> Đã kết nối cơ sở dữ liệu <code>SQL Server: TDSouth (SQLEXPRESS)</code>
            <?php else: ?>
                <span class="status-dot status-offline"></span>
                <strong>Trạng thái:</strong> <?php echo htmlspecialchars($dbMessage); ?>
            <?php endif; ?>
        </div>
        <div>
            <span>Thời gian máy chủ: <strong><?php echo date("H:i:s d/m/Y"); ?></strong></span>
        </div>
    </div>

    <?php if (!empty($actionMsg)): ?>
        <div class="alert alert-success">
            ✅ <?php echo htmlspecialchars($actionMsg); ?>
        </div>
    <?php endif; ?>

    <!-- KPI Dashboard Cards -->
    <div class="stats-grid">
        <div class="stat-card blue">
            <div class="stat-icon">👥</div>
            <div class="stat-info">
                <h3 id="statTotal"><?php echo $totalRecords; ?></h3>
                <p>Tổng lượt chấm công</p>
            </div>
        </div>
        <div class="stat-card green">
            <div class="stat-icon">⏰</div>
            <div class="stat-info">
                <h3 id="statOnTime"><?php echo $onTimeCount; ?></h3>
                <p>Đi đúng giờ</p>
            </div>
        </div>
        <div class="stat-card amber">
            <div class="stat-icon">⚠️</div>
            <div class="stat-info">
                <h3 id="statLate"><?php echo $lateCount; ?></h3>
                <p>Đi muộn</p>
            </div>
        </div>
        <div class="stat-card purple">
            <div class="stat-icon">🏢</div>
            <div class="stat-info">
                <h3 id="statWorking"><?php echo $currentlyWorking; ?></h3>
                <p>Đang trong ca làm việc</p>
            </div>
        </div>
    </div>

    <!-- Main Layout -->
    <div class="main-grid">
        <!-- Form Chấm Công -->
        <div class="card">
            <h2 class="card-title">📝 Thao Tác Chấm Công</h2>
            
            <form id="attendanceForm" method="POST" action="">
                <input type="hidden" name="action" id="actionField" value="checkin">
                
                <div class="form-group">
                    <label for="manv">Mã Nhân Viên *</label>
                    <input type="text" id="manv" name="manv" class="form-control" placeholder="Ví dụ: TDS001" required>
                </div>

                <div class="form-group">
                    <label for="hoten">Họ và Tên *</label>
                    <input type="text" id="hoten" name="hoten" class="form-control" placeholder="Ví dụ: Lê Minh Phúc" required>
                </div>

                <div class="form-group">
                    <label for="phongban">Phòng Ban *</label>
                    <select id="phongban" name="phongban" class="form-control" required>
                        <option value="Quản trị Hệ thống (Admin)">Quản trị Hệ thống (Admin)</option>
                        <option value="Kỹ thuật Hạ tầng (Tech Lead)">Kỹ thuật Hạ tầng (Tech Lead)</option>
                        <option value="Phát triển Ứng dụng (Dev)">Phát triển Ứng dụng (Dev)</option>
                        <option value="Kiểm thử Chất lượng (QA)">Kiểm thử Chất lượng (QA)</option>
                    </select>
                </div>

                <div class="form-group">
                    <label for="calamviec">Ca Làm Việc *</label>
                    <select id="calamviec" name="calamviec" class="form-control" required>
                        <option value="Ca Hành Chính (08:00 - 17:30)">Ca Hành Chính (08:00 - 17:30)</option>
                        <option value="Ca Sáng (08:00 - 12:00)">Ca Sáng (08:00 - 12:00)</option>
                        <option value="Ca Chiều (13:00 - 17:00)">Ca Chiều (13:00 - 17:00)</option>
                    </select>
                </div>

                <div class="btn-group">
                    <button type="submit" class="btn btn-checkin" onclick="setAction('checkin')">
                        🟢 Check-in (Vào ca)
                    </button>
                    <button type="submit" class="btn btn-checkout" onclick="setAction('checkout')">
                        🔴 Check-out (Ra ca)
                    </button>
                </div>
            </form>

            <div class="quick-select-box">
                <div class="quick-select-title">Chọn nhanh nhân viên phòng Lab:</div>
                <div class="quick-badges">
                    <button type="button" class="quick-btn" onclick="fillEmployee('TDS001', 'Lê Minh Phúc', 'Quản trị Hệ thống (Admin)')">Lê Minh Phúc</button>
                    <button type="button" class="quick-btn" onclick="fillEmployee('TDS002', 'Kỳ Nguyễn', 'Kỹ thuật Hạ tầng (Tech Lead)')">Kỳ Nguyễn</button>
                    <button type="button" class="quick-btn" onclick="fillEmployee('TDS003', 'Trường Nguyễn', 'Phát triển Ứng dụng (Dev)')">Trường Nguyễn</button>
                    <button type="button" class="quick-btn" onclick="fillEmployee('TDS004', 'Tín Phan', 'Kiểm thử Chất lượng (QA)')">Tín Phan</button>
                </div>
            </div>
        </div>

        <!-- Bảng Nhật Ký Chấm Công -->
        <div class="card">
            <div class="table-toolbar">
                <h2 class="card-title" style="margin-bottom: 0; padding-bottom: 0; border: none;">📋 Nhật Ký Chấm Công</h2>
                <div class="search-box">
                    <input type="text" id="searchInput" class="form-control" placeholder="Tìm theo tên hoặc mã NV..." style="width: 240px; padding: 7px 12px;">
                    <button type="button" class="btn" style="background:#e2e8f0; color:#334155; padding:7px 12px; font-size:0.85rem;" onclick="exportTable()">🖨️ In / Xuất</button>
                </div>
            </div>

            <div class="table-container">
                <table id="attendanceTable">
                    <thead>
                        <tr>
                            <th>Mã NV</th>
                            <th>Họ và Tên</th>
                            <th>Phòng Ban</th>
                            <th>Ca Làm Việc</th>
                            <th>Giờ Vào</th>
                            <th>Giờ Ra</th>
                            <th>Trạng Thái</th>
                            <th>Ngày</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($attendanceList as $row): ?>
                        <tr>
                            <td><strong><?php echo htmlspecialchars($row["MaNV"]); ?></strong></td>
                            <td><?php echo htmlspecialchars($row["HoTen"]); ?></td>
                            <td><?php echo htmlspecialchars($row["PhongBan"]); ?></td>
                            <td><?php echo htmlspecialchars($row["CaLamViec"]); ?></td>
                            <td><span style="color: #059669; font-weight: 600;"><?php echo htmlspecialchars($row["GioVao"]); ?></span></td>
                            <td><span style="color: #dc2626; font-weight: 600;"><?php echo htmlspecialchars($row["GioRa"] ?? "--:--"); ?></span></td>
                            <td>
                                <?php if ($row["TrangThai"] === "Đúng giờ"): ?>
                                    <span class="badge badge-ontime">Đúng giờ</span>
                                <?php elseif ($row["TrangThai"] === "Đi muộn"): ?>
                                    <span class="badge badge-late">Đi muộn</span>
                                <?php else: ?>
                                    <span class="badge badge-working"><?php echo htmlspecialchars($row["TrangThai"]); ?></span>
                                <?php endif; ?>
                            </td>
                            <td><?php echo htmlspecialchars($row["NgayCham"]); ?></td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- Footer -->
    <footer>
        <p><strong>TDSouth Infrastructure Lab</strong> &bull; Quản lý dự án: Lê Minh Phúc | Tech Lead: Kỳ Nguyễn | Dev: Trường Nguyễn | QA: Tín Phan</p>
        <p style="margin-top: 4px;">Phiên bản Hệ thống Chấm công v1.0 &bull; Chạy trên IIS & SQL Server Express</p>
    </footer>
</div>

<script>
    // Đồng hồ thời gian thực (Live ticking clock)
    function updateClock() {
        const now = new Date();
        const hours = String(now.getHours()).padStart(2, '0');
        const minutes = String(now.getMinutes()).padStart(2, '0');
        const seconds = String(now.getSeconds()).padStart(2, '0');
        document.getElementById('liveClock').textContent = `${hours}:${minutes}:${seconds}`;

        const days = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
        const dayName = days[now.getDay()];
        const dateStr = `${dayName}, ${String(now.getDate()).padStart(2, '0')}/${String(now.getMonth() + 1).padStart(2, '0')}/${now.getFullYear()}`;
        document.getElementById('liveDate').textContent = dateStr;
    }
    setInterval(updateClock, 1000);
    updateClock();

    function setAction(type) {
        document.getElementById('actionField').value = type;
    }

    function fillEmployee(id, name, dept) {
        document.getElementById('manv').value = id;
        document.getElementById('hoten').value = name;
        document.getElementById('phongban').value = dept;
    }

    // Tìm kiếm trong bảng
    document.getElementById('searchInput').addEventListener('keyup', function() {
        const query = this.value.toLowerCase();
        const rows = document.querySelectorAll('#attendanceTable tbody tr');
        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            row.style.display = text.includes(query) ? '' : 'none';
        });
    });

    function exportTable() {
        window.print();
    }
</script>

</body>
</html>

