using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TDSouth.ChamCong.Models;

namespace TDSouth.ChamCong.Services
{
    /// <summary>
    /// Dịch vụ kết nối CSDL Microsoft SQL Server (TDSouth) và xử lý nghiệp vụ Chấm Công
    /// Hỗ trợ tự động khởi tạo bảng [ChamCong] và tự động chuyển đổi sang In-Memory Demo khi SQL Server chưa bật
    /// </summary>
    public class ChamCongService
    {
        public const string DefaultConnectionString =
            @"Data Source=localhost\SQLEXPRESS;Initial Catalog=TDSouth;User ID=sa;Password=admin@123;Connect Timeout=2;MultipleActiveResultSets=True";

        private readonly string _connectionString;
        private static readonly object _syncLock = new object();
        private static List<ChamCongRecord> _fallbackMemoryStore;

        public bool IsDbConnected { get; private set; }
        public string DbStatusMessage { get; private set; }

        public ChamCongService(string connectionString = null)
        {
            _connectionString = string.IsNullOrWhiteSpace(connectionString)
                ? DefaultConnectionString
                : connectionString;

            EnsureFallbackStoreInitialized();
            InitializeDatabase();
        }

        private static void EnsureFallbackStoreInitialized()
        {
            lock (_syncLock)
            {
                if (_fallbackMemoryStore != null)
                    return;

                string today = DateTime.Now.ToString("dd/MM/yyyy");
                _fallbackMemoryStore = new List<ChamCongRecord>
                {
                    new ChamCongRecord
                    {
                        ID = 1,
                        MaNV = "TDS001",
                        HoTen = "Lê Minh Phúc",
                        PhongBan = "Quản trị Hệ thống (Admin)",
                        CaLamViec = "Ca Hành Chính (08:00 - 17:30)",
                        GioVao = "07:55:12",
                        GioRa = "17:35:40",
                        TrangThai = "Đúng giờ",
                        NgayCham = today,
                        CreatedDate = DateTime.Now.AddHours(-3)
                    },
                    new ChamCongRecord
                    {
                        ID = 2,
                        MaNV = "TDS002",
                        HoTen = "Kỳ Nguyễn",
                        PhongBan = "Kỹ thuật Hạ tầng (Tech Lead)",
                        CaLamViec = "Ca Hành Chính (08:00 - 17:30)",
                        GioVao = "08:04:20",
                        GioRa = "17:30:10",
                        TrangThai = "Đúng giờ",
                        NgayCham = today,
                        CreatedDate = DateTime.Now.AddHours(-2)
                    },
                    new ChamCongRecord
                    {
                        ID = 3,
                        MaNV = "TDS003",
                        HoTen = "Trường Nguyễn",
                        PhongBan = "Phát triển Ứng dụng (Dev)",
                        CaLamViec = "Ca Sáng (08:00 - 12:00)",
                        GioVao = "08:18:45",
                        GioRa = "12:05:00",
                        TrangThai = "Đi muộn",
                        NgayCham = today,
                        CreatedDate = DateTime.Now.AddHours(-1)
                    },
                    new ChamCongRecord
                    {
                        ID = 4,
                        MaNV = "TDS004",
                        HoTen = "Tín Phan",
                        PhongBan = "Kiểm thử Chất lượng (QA)",
                        CaLamViec = "Ca Hành Chính (08:00 - 17:30)",
                        GioVao = "08:00:05",
                        GioRa = "--:--",
                        TrangThai = "Đúng giờ",
                        NgayCham = today,
                        CreatedDate = DateTime.Now
                    }
                };
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChamCong' AND xtype='U')
                        BEGIN
                            CREATE TABLE ChamCong (
                                ID INT IDENTITY(1,1) PRIMARY KEY,
                                MaNV NVARCHAR(50) NOT NULL,
                                HoTen NVARCHAR(100) NOT NULL,
                                PhongBan NVARCHAR(100) NULL,
                                CaLamViec NVARCHAR(50) NULL,
                                GioVao NVARCHAR(50) NULL,
                                GioRa NVARCHAR(50) NULL,
                                TrangThai NVARCHAR(50) NULL,
                                NgayCham NVARCHAR(50) NULL,
                                CreatedDate DATETIME DEFAULT GETDATE()
                            );
                        END";

                    using (var cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    IsDbConnected = true;
                    DbStatusMessage = "Đã kết nối CSDL SQL Server (localhost\\SQLEXPRESS - DB: TDSouth)";
                }
            }
            catch (Exception)
            {
                IsDbConnected = false;
                DbStatusMessage = "Chế độ C# In-Memory / Demo (SQL Server localhost\\SQLEXPRESS đang tắt)";
            }
        }

        public static string EvaluateAttendanceStatus(DateTime checkInTime)
        {
            // Quy định ca chuẩn 08:00, cho phép trễ tối đa đến 08:15
            if (checkInTime.Hour < 8 || (checkInTime.Hour == 8 && checkInTime.Minute <= 15))
                return "Đúng giờ";
            return "Đi muộn";
        }

        public string CheckIn(string maNV, string hoTen, string phongBan, string caLamViec)
        {
            if (string.IsNullOrWhiteSpace(maNV) || string.IsNullOrWhiteSpace(hoTen))
                return "Vui lòng nhập đầy đủ Mã nhân viên và Họ tên!";

            DateTime now = DateTime.Now;
            string currentTime = now.ToString("HH:mm:ss");
            string currentDate = now.ToString("dd/MM/yyyy");
            string trangThai = EvaluateAttendanceStatus(now);

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string insertSql = @"
                            INSERT INTO ChamCong (MaNV, HoTen, PhongBan, CaLamViec, GioVao, GioRa, TrangThai, NgayCham)
                            VALUES (@MaNV, @HoTen, @PhongBan, @CaLamViec, @GioVao, @GioRa, @TrangThai, @NgayCham)";

                        using (var cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaNV", maNV.Trim().ToUpperInvariant());
                            cmd.Parameters.AddWithValue("@HoTen", hoTen.Trim());
                            cmd.Parameters.AddWithValue("@PhongBan", phongBan ?? "Kỹ thuật Hạ tầng");
                            cmd.Parameters.AddWithValue("@CaLamViec", caLamViec ?? "Ca Hành Chính (08:00 - 17:30)");
                            cmd.Parameters.AddWithValue("@GioVao", currentTime);
                            cmd.Parameters.AddWithValue("@GioRa", "--:--");
                            cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                            cmd.Parameters.AddWithValue("@NgayCham", currentDate);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return string.Format("Check-in thành công (SQL Server): {0} ({1}) lúc {2} - [{3}]",
                        hoTen.Trim(), maNV.Trim().ToUpperInvariant(), currentTime, trangThai);
                }
                catch (Exception ex)
                {
                    return "Lỗi khi ghi Check-in vào SQL Server: " + ex.Message;
                }
            }

            lock (_syncLock)
            {
                int nextId = _fallbackMemoryStore.Count > 0 ? _fallbackMemoryStore[0].ID + 1 : 1;
                foreach (var rec in _fallbackMemoryStore)
                {
                    if (rec.ID >= nextId) nextId = rec.ID + 1;
                }

                _fallbackMemoryStore.Insert(0, new ChamCongRecord
                {
                    ID = nextId,
                    MaNV = maNV.Trim().ToUpperInvariant(),
                    HoTen = hoTen.Trim(),
                    PhongBan = phongBan ?? "Kỹ thuật Hạ tầng",
                    CaLamViec = caLamViec ?? "Ca Hành Chính (08:00 - 17:30)",
                    GioVao = currentTime,
                    GioRa = "--:--",
                    TrangThai = trangThai,
                    NgayCham = currentDate,
                    CreatedDate = now
                });
            }

            return string.Format("Check-in thành công: {0} ({1}) lúc {2} - [{3}]",
                hoTen.Trim(), maNV.Trim().ToUpperInvariant(), currentTime, trangThai);
        }

        public string CheckOut(string maNV, string hoTen)
        {
            if (string.IsNullOrWhiteSpace(maNV))
                return "Vui lòng nhập hoặc chọn Mã nhân viên để Check-out!";

            string normalizedMaNV = maNV.Trim().ToUpperInvariant();
            string currentTime = DateTime.Now.ToString("HH:mm:ss");
            string currentDate = DateTime.Now.ToString("dd/MM/yyyy");

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string updateSql = @"
                            UPDATE ChamCong
                            SET GioRa = @GioRa
                            WHERE ID = (
                                SELECT TOP 1 ID FROM ChamCong
                                WHERE MaNV = @MaNV AND NgayCham = @NgayCham
                                ORDER BY ID DESC
                            )";

                        using (var cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@GioRa", currentTime);
                            cmd.Parameters.AddWithValue("@MaNV", normalizedMaNV);
                            cmd.Parameters.AddWithValue("@NgayCham", currentDate);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                return string.Format("Check-out thành công (SQL Server) cho mã {0} lúc {1}!",
                                    normalizedMaNV, currentTime);
                            }
                        }
                    }
                    return string.Format("Không tìm thấy ca làm việc trong ngày của nhân viên {0} để Check-out!", normalizedMaNV);
                }
                catch (Exception ex)
                {
                    return "Lỗi khi cập nhật Check-out SQL Server: " + ex.Message;
                }
            }

            lock (_syncLock)
            {
                foreach (var rec in _fallbackMemoryStore)
                {
                    if (string.Equals(rec.MaNV, normalizedMaNV, StringComparison.OrdinalIgnoreCase))
                    {
                        rec.GioRa = currentTime;
                        return string.Format("Check-out thành công cho nhân viên {0} ({1}) lúc {2}!",
                            string.IsNullOrWhiteSpace(hoTen) ? rec.HoTen : hoTen, normalizedMaNV, currentTime);
                    }
                }
            }

            return string.Format("Không tìm thấy bản ghi Check-in của mã nhân viên {0}!", normalizedMaNV);
        }

        public List<ChamCongRecord> GetAttendanceRecords(string searchKeyword = null)
        {
            var results = new List<ChamCongRecord>();

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string selectSql = @"
                            SELECT TOP 50 ID, MaNV, HoTen, PhongBan, CaLamViec, GioVao, GioRa, TrangThai, NgayCham, CreatedDate
                            FROM ChamCong
                            ORDER BY ID DESC";

                        using (var cmd = new SqlCommand(selectSql, conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new ChamCongRecord
                                {
                                    ID = reader["ID"] != DBNull.Value ? Convert.ToInt32(reader["ID"]) : 0,
                                    MaNV = reader["MaNV"] != DBNull.Value ? reader["MaNV"].ToString() : "",
                                    HoTen = reader["HoTen"] != DBNull.Value ? reader["HoTen"].ToString() : "",
                                    PhongBan = reader["PhongBan"] != DBNull.Value ? reader["PhongBan"].ToString() : "",
                                    CaLamViec = reader["CaLamViec"] != DBNull.Value ? reader["CaLamViec"].ToString() : "",
                                    GioVao = reader["GioVao"] != DBNull.Value ? reader["GioVao"].ToString() : "--:--",
                                    GioRa = reader["GioRa"] != DBNull.Value ? reader["GioRa"].ToString() : "--:--",
                                    TrangThai = reader["TrangThai"] != DBNull.Value ? reader["TrangThai"].ToString() : "Đúng giờ",
                                    NgayCham = reader["NgayCham"] != DBNull.Value ? reader["NgayCham"].ToString() : DateTime.Now.ToString("dd/MM/yyyy"),
                                    CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : DateTime.Now
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Fallback nếu truy vấn gặp sự cố
                }
            }

            if (results.Count == 0)
            {
                lock (_syncLock)
                {
                    results.AddRange(_fallbackMemoryStore);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string kw = searchKeyword.Trim();
                results = results.FindAll(r =>
                    (r.MaNV != null && r.MaNV.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.HoTen != null && r.HoTen.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.PhongBan != null && r.PhongBan.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            return results;
        }
    }
}

