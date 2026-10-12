using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TDSouth.Erp.Models;

namespace TDSouth.Erp.Services
{
    /// <summary>
    /// Dịch vụ kết nối CSDL Microsoft SQL Server (TDSouth) và quản trị nguồn lực doanh nghiệp (ERP)
    /// Hỗ trợ đầy đủ CRUD cho 3 phân hệ: Vật tư/Thiết bị, Dự án hạ tầng, và Nhân sự phân quyền
    /// </summary>
    public class ErpService
    {
        public const string DefaultConnectionString =
            @"Data Source=localhost\SQLEXPRESS;Initial Catalog=TDSouth;User ID=sa;Password=admin@123;Connect Timeout=2;MultipleActiveResultSets=True";

        private readonly string _connectionString;
        private static readonly object _syncLock = new object();
        private static List<ThietBiAsset> _fallbackAssets;
        private static List<DuAnProject> _fallbackProjects;
        private static List<ThanhVienRole> _fallbackRoles;

        public bool IsDbConnected { get; private set; }
        public string DbStatusMessage { get; private set; }

        public ErpService(string connectionString = null)
        {
            _connectionString = string.IsNullOrWhiteSpace(connectionString)
                ? DefaultConnectionString
                : connectionString;

            EnsureFallbackSeedData();
            InitializeDatabase();
        }

        private static void EnsureFallbackSeedData()
        {
            lock (_syncLock)
            {
                if (_fallbackAssets == null)
                {
                    _fallbackAssets = new List<ThietBiAsset>
                    {
                        new ThietBiAsset
                        {
                            ID = 1,
                            MaTB = "SRV-01",
                            TenTB = "Dell PowerEdge R740 Rack Server (2x Xeon Gold, 128GB RAM)",
                            LoaiTB = "Máy Chủ (Server)",
                            SoLuong = 2,
                            DonGia = 145000000L,
                            TrangThai = "Đang sử dụng"
                        },
                        new ThietBiAsset
                        {
                            ID = 2,
                            MaTB = "SW-CORE",
                            TenTB = "Cisco Catalyst 2960-X 48 Port Gigabit Switch",
                            LoaiTB = "Thiết Bị Mạng (Switch)",
                            SoLuong = 4,
                            DonGia = 28500000L,
                            TrangThai = "Đang sử dụng"
                        },
                        new ThietBiAsset
                        {
                            ID = 3,
                            MaTB = "FW-FORTI",
                            TenTB = "Fortinet FortiGate 100F Next-Gen Firewall",
                            LoaiTB = "Bảo Mật (Firewall)",
                            SoLuong = 1,
                            DonGia = 68000000L,
                            TrangThai = "Đang sử dụng"
                        },
                        new ThietBiAsset
                        {
                            ID = 4,
                            MaTB = "NAS-SYN",
                            TenTB = "Synology RackStation RS3621xs+ 12-Bay Storage NAS",
                            LoaiTB = "Lưu Trữ (Storage)",
                            SoLuong = 1,
                            DonGia = 85000000L,
                            TrangThai = "Đang bảo trì"
                        }
                    };
                }

                if (_fallbackProjects == null)
                {
                    _fallbackProjects = new List<DuAnProject>
                    {
                        new DuAnProject
                        {
                            MaDuAn = "PRJ-AD01",
                            TenDuAn = "Triển khai Hệ thống Active Directory Domain Controller & GPO",
                            TruongNhom = "Nguyễn Văn A",
                            TienDo = 100,
                            TrangThai = "Hoàn thành",
                            KinhPhiDuToan = 45000000L
                        },
                        new DuAnProject
                        {
                            MaDuAn = "PRJ-DNS02",
                            TenDuAn = "Cấu hình Dịch vụ Mạng DNS & DHCP Failover Lab",
                            TruongNhom = "Kỳ Nguyễn",
                            TienDo = 85,
                            TrangThai = "Đang triển khai",
                            KinhPhiDuToan = 30000000L
                        },
                        new DuAnProject
                        {
                            MaDuAn = "PRJ-IIS03",
                            TenDuAn = "Xây dựng IIS Web Server (C# ASP.NET) & Chứng chỉ SSL/HTTPS",
                            TruongNhom = "Trường Nguyễn",
                            TienDo = 95,
                            TrangThai = "Đang hoàn thiện",
                            KinhPhiDuToan = 25000000L
                        },
                        new DuAnProject
                        {
                            MaDuAn = "PRJ-SEC04",
                            TenDuAn = "Kiểm thử An toàn Thông tin & Triển khai FTPS/File Server",
                            TruongNhom = "Tín Phan",
                            TienDo = 75,
                            TrangThai = "Đang kiểm thử",
                            KinhPhiDuToan = 20000000L
                        }
                    };
                }

                if (_fallbackRoles == null)
                {
                    _fallbackRoles = new List<ThanhVienRole>
                    {
                        new ThanhVienRole
                        {
                            STT = 1,
                            HoTen = "Nguyễn Văn A",
                            VaiTroDuAn = "Project Owner / Admin",
                            QuyenGitHub = "Admin",
                            MoTaTrachNhiem = "Quản trị toàn quyền repository, thiết lập Environments, phê duyệt PR chính vào main."
                        },
                        new ThanhVienRole
                        {
                            STT = 2,
                            HoTen = "Kỳ Nguyễn",
                            VaiTroDuAn = "Technical Lead",
                            QuyenGitHub = "Maintain",
                            MoTaTrachNhiem = "Quản lý review code, kiểm duyệt nhánh test/staging, giải quyết conflict mã nguồn."
                        },
                        new ThanhVienRole
                        {
                            STT = 3,
                            HoTen = "Trường Nguyễn",
                            VaiTroDuAn = "Developer",
                            QuyenGitHub = "Write",
                            MoTaTrachNhiem = "Lập trình tính năng C# ASP.NET, đẩy code lên nhánh feature/*, tạo Pull Request."
                        },
                        new ThanhVienRole
                        {
                            STT = 4,
                            HoTen = "Tín Phan",
                            VaiTroDuAn = "QA / Tester",
                            QuyenGitHub = "Triage / Read",
                            MoTaTrachNhiem = "Kiểm thử phần mềm, ghi nhận Issue lỗi, xác nhận checklist kiểm thử trước khi release."
                        }
                    };
                }
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string createTablesSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ERP_ThietBi' AND xtype='U')
                        BEGIN
                            CREATE TABLE ERP_ThietBi (
                                ID INT IDENTITY(1,1) PRIMARY KEY,
                                MaTB NVARCHAR(50) NOT NULL,
                                TenTB NVARCHAR(200) NOT NULL,
                                LoaiTB NVARCHAR(100) NULL,
                                SoLuong INT DEFAULT 1,
                                DonGia BIGINT DEFAULT 0,
                                TrangThai NVARCHAR(50) NULL,
                                CreatedDate DATETIME DEFAULT GETDATE()
                            );
                        END;

                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ERP_DuAn' AND xtype='U')
                        BEGIN
                            CREATE TABLE ERP_DuAn (
                                MaDuAn NVARCHAR(50) PRIMARY KEY,
                                TenDuAn NVARCHAR(200) NOT NULL,
                                TruongNhom NVARCHAR(100) NULL,
                                TienDo INT DEFAULT 0,
                                TrangThai NVARCHAR(50) NULL,
                                KinhPhiDuToan BIGINT DEFAULT 0
                            );
                        END;";

                    using (var cmd = new SqlCommand(createTablesSql, conn))
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
                DbStatusMessage = "C# ASP.NET In-Memory Mode (SQL Server localhost\\SQLEXPRESS đang tắt)";
            }
        }

        // ======================== CRUD THIẾT BỊ / VẬT TƯ ========================

        public string AddAsset(string maTB, string tenTB, string loaiTB, int soLuong, long donGia, string trangThai)
        {
            if (string.IsNullOrWhiteSpace(maTB) || string.IsNullOrWhiteSpace(tenTB))
                return "Vui lòng nhập đầy đủ Mã thiết bị và Tên thiết bị/vật tư!";

            if (soLuong <= 0) soLuong = 1;
            if (donGia < 0) donGia = 0;

            string cleanMa = maTB.Trim().ToUpperInvariant();
            string cleanTen = tenTB.Trim();
            string cleanLoai = string.IsNullOrWhiteSpace(loaiTB) ? "Máy Chủ (Server)" : loaiTB.Trim();
            string cleanStatus = string.IsNullOrWhiteSpace(trangThai) ? "Đang sử dụng" : trangThai.Trim();

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string insertSql = @"
                            INSERT INTO ERP_ThietBi (MaTB, TenTB, LoaiTB, SoLuong, DonGia, TrangThai)
                            VALUES (@MaTB, @TenTB, @LoaiTB, @SoLuong, @DonGia, @TrangThai)";

                        using (var cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaTB", cleanMa);
                            cmd.Parameters.AddWithValue("@TenTB", cleanTen);
                            cmd.Parameters.AddWithValue("@LoaiTB", cleanLoai);
                            cmd.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmd.Parameters.AddWithValue("@DonGia", donGia);
                            cmd.Parameters.AddWithValue("@TrangThai", cleanStatus);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return string.Format("Đã nhập kho CSDL SQL Server thiết bị [{0}] - {1} (SL: {2})!", cleanMa, cleanTen, soLuong);
                }
                catch (Exception ex)
                {
                    return "Lỗi khi lưu thiết bị vào SQL Server: " + ex.Message;
                }
            }

            lock (_syncLock)
            {
                int nextId = _fallbackAssets.Count > 0 ? _fallbackAssets[0].ID + 1 : 1;
                foreach (var a in _fallbackAssets)
                {
                    if (a.ID >= nextId) nextId = a.ID + 1;
                }

                _fallbackAssets.Insert(0, new ThietBiAsset
                {
                    ID = nextId,
                    MaTB = cleanMa,
                    TenTB = cleanTen,
                    LoaiTB = cleanLoai,
                    SoLuong = soLuong,
                    DonGia = donGia,
                    TrangThai = cleanStatus,
                    CreatedDate = DateTime.Now
                });
            }

            return string.Format("Đã thêm thiết bị [{0}] - {1} (Số lượng: {2}) vào hệ thống!", cleanMa, cleanTen, soLuong);
        }

        public string UpdateAsset(int id, string maTB, string tenTB, string loaiTB, int soLuong, long donGia, string trangThai)
        {
            if (id <= 0) return "ID thiết bị không hợp lệ!";
            if (string.IsNullOrWhiteSpace(maTB) || string.IsNullOrWhiteSpace(tenTB))
                return "Vui lòng nhập đầy đủ Mã và Tên thiết bị!";

            if (soLuong <= 0) soLuong = 1;
            if (donGia < 0) donGia = 0;

            string cleanMa = maTB.Trim().ToUpperInvariant();
            string cleanTen = tenTB.Trim();
            string cleanLoai = string.IsNullOrWhiteSpace(loaiTB) ? "Máy Chủ (Server)" : loaiTB.Trim();
            string cleanStatus = string.IsNullOrWhiteSpace(trangThai) ? "Đang sử dụng" : trangThai.Trim();

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string updateSql = @"
                            UPDATE ERP_ThietBi
                            SET MaTB = @MaTB, TenTB = @TenTB, LoaiTB = @LoaiTB, SoLuong = @SoLuong, DonGia = @DonGia, TrangThai = @TrangThai
                            WHERE ID = @ID";

                        using (var cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", id);
                            cmd.Parameters.AddWithValue("@MaTB", cleanMa);
                            cmd.Parameters.AddWithValue("@TenTB", cleanTen);
                            cmd.Parameters.AddWithValue("@LoaiTB", cleanLoai);
                            cmd.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmd.Parameters.AddWithValue("@DonGia", donGia);
                            cmd.Parameters.AddWithValue("@TrangThai", cleanStatus);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                                return string.Format("Đã cập nhật thiết bị [{0}] trên CSDL SQL Server!", cleanMa);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return "Lỗi cập nhật thiết bị SQL Server: " + ex.Message;
                }
            }

            lock (_syncLock)
            {
                var target = _fallbackAssets.Find(x => x.ID == id);
                if (target != null)
                {
                    target.MaTB = cleanMa;
                    target.TenTB = cleanTen;
                    target.LoaiTB = cleanLoai;
                    target.SoLuong = soLuong;
                    target.DonGia = donGia;
                    target.TrangThai = cleanStatus;
                    return string.Format("Đã cập nhật thông tin thiết bị [{0}] - {1} thành công!", cleanMa, cleanTen);
                }
            }

            return "Không tìm thấy thiết bị có ID: " + id;
        }

        public string DeleteAsset(int id)
        {
            if (id <= 0) return "ID thiết bị không hợp lệ!";

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string deleteSql = "DELETE FROM ERP_ThietBi WHERE ID = @ID";
                        using (var cmd = new SqlCommand(deleteSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", id);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                                return string.Format("Đã xóa thiết bị #{0} khỏi CSDL SQL Server!", id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return "Lỗi xóa thiết bị SQL Server: " + ex.Message;
                }
            }

            lock (_syncLock)
            {
                int removed = _fallbackAssets.RemoveAll(x => x.ID == id);
                if (removed > 0)
                    return string.Format("Đã xóa thiết bị #{0} thành công!", id);
            }

            return "Không tìm thấy thiết bị có ID: " + id;
        }

        public List<ThietBiAsset> GetAssets(string searchKeyword = null)
        {
            var list = new List<ThietBiAsset>();

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string selectSql = "SELECT ID, MaTB, TenTB, LoaiTB, SoLuong, DonGia, TrangThai, CreatedDate FROM ERP_ThietBi ORDER BY ID DESC";
                        using (var cmd = new SqlCommand(selectSql, conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new ThietBiAsset
                                {
                                    ID = reader["ID"] != DBNull.Value ? Convert.ToInt32(reader["ID"]) : 0,
                                    MaTB = Convert.ToString(reader["MaTB"]),
                                    TenTB = Convert.ToString(reader["TenTB"]),
                                    LoaiTB = Convert.ToString(reader["LoaiTB"]),
                                    SoLuong = reader["SoLuong"] != DBNull.Value ? Convert.ToInt32(reader["SoLuong"]) : 1,
                                    DonGia = reader["DonGia"] != DBNull.Value ? Convert.ToInt64(reader["DonGia"]) : 0L,
                                    TrangThai = Convert.ToString(reader["TrangThai"]),
                                    CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : DateTime.Now
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Fallback
                }
            }

            if (list.Count == 0)
            {
                lock (_syncLock)
                {
                    list.AddRange(_fallbackAssets);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string kw = searchKeyword.Trim();
                list = list.FindAll(a =>
                    (a.MaTB != null && a.MaTB.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (a.TenTB != null && a.TenTB.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (a.LoaiTB != null && a.LoaiTB.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            return list;
        }

        // ======================== CRUD DỰ ÁN HẠ TẦNG ========================

        public List<DuAnProject> GetProjects(string searchKeyword = null)
        {
            var list = new List<DuAnProject>();

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "SELECT MaDuAn, TenDuAn, TruongNhom, TienDo, TrangThai, KinhPhiDuToan FROM ERP_DuAn";
                        using (var cmd = new SqlCommand(sql, conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new DuAnProject
                                {
                                    MaDuAn = Convert.ToString(reader["MaDuAn"]),
                                    TenDuAn = Convert.ToString(reader["TenDuAn"]),
                                    TruongNhom = Convert.ToString(reader["TruongNhom"]),
                                    TienDo = reader["TienDo"] != DBNull.Value ? Convert.ToInt32(reader["TienDo"]) : 0,
                                    TrangThai = Convert.ToString(reader["TrangThai"]),
                                    KinhPhiDuToan = reader["KinhPhiDuToan"] != DBNull.Value ? Convert.ToInt64(reader["KinhPhiDuToan"]) : 0L
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Fallback
                }
            }

            if (list.Count == 0)
            {
                lock (_syncLock)
                {
                    list.AddRange(_fallbackProjects);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string kw = searchKeyword.Trim();
                list = list.FindAll(p =>
                    (p.MaDuAn != null && p.MaDuAn.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (p.TenDuAn != null && p.TenDuAn.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (p.TruongNhom != null && p.TruongNhom.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            return list;
        }

        public string AddProject(DuAnProject p)
        {
            if (p == null || string.IsNullOrWhiteSpace(p.MaDuAn) || string.IsNullOrWhiteSpace(p.TenDuAn))
                return "Vui lòng nhập đầy đủ Mã và Tên dự án!";

            string cleanMa = p.MaDuAn.Trim().ToUpperInvariant();
            string cleanTen = p.TenDuAn.Trim();

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            IF NOT EXISTS (SELECT 1 FROM ERP_DuAn WHERE MaDuAn = @MaDuAn)
                            BEGIN
                                INSERT INTO ERP_DuAn (MaDuAn, TenDuAn, TruongNhom, TienDo, TrangThai, KinhPhiDuToan)
                                VALUES (@MaDuAn, @TenDuAn, @TruongNhom, @TienDo, @TrangThai, @KinhPhiDuToan)
                            END";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaDuAn", cleanMa);
                            cmd.Parameters.AddWithValue("@TenDuAn", cleanTen);
                            cmd.Parameters.AddWithValue("@TruongNhom", p.TruongNhom ?? "");
                            cmd.Parameters.AddWithValue("@TienDo", p.TienDo);
                            cmd.Parameters.AddWithValue("@TrangThai", p.TrangThai ?? "Đang triển khai");
                            cmd.Parameters.AddWithValue("@KinhPhiDuToan", p.KinhPhiDuToan);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return string.Format("Đã tạo dự án [{0}] trên SQL Server!", cleanMa);
                }
                catch (Exception ex)
                {
                    return "Lỗi thêm dự án SQL Server: " + ex.Message;
                }
            }

            lock (_syncLock)
            {
                if (_fallbackProjects.Exists(x => string.Equals(x.MaDuAn, cleanMa, StringComparison.OrdinalIgnoreCase)))
                    return string.Format("Mã dự án [{0}] đã tồn tại!", cleanMa);

                _fallbackProjects.Insert(0, new DuAnProject
                {
                    MaDuAn = cleanMa,
                    TenDuAn = cleanTen,
                    TruongNhom = p.TruongNhom ?? "Nguyễn Văn A",
                    TienDo = p.TienDo,
                    TrangThai = p.TrangThai ?? "Đang triển khai",
                    KinhPhiDuToan = p.KinhPhiDuToan
                });
            }

            return string.Format("Đã thêm thành công dự án [{0}] - {1}!", cleanMa, cleanTen);
        }

        public string UpdateProject(DuAnProject p)
        {
            if (p == null || string.IsNullOrWhiteSpace(p.MaDuAn))
                return "Mã dự án cần cập nhật không hợp lệ!";

            string cleanMa = p.MaDuAn.Trim().ToUpperInvariant();

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            UPDATE ERP_DuAn
                            SET TenDuAn = @TenDuAn, TruongNhom = @TruongNhom, TienDo = @TienDo, 
                                TrangThai = @TrangThai, KinhPhiDuToan = @KinhPhiDuToan
                            WHERE MaDuAn = @MaDuAn";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaDuAn", cleanMa);
                            cmd.Parameters.AddWithValue("@TenDuAn", p.TenDuAn ?? "");
                            cmd.Parameters.AddWithValue("@TruongNhom", p.TruongNhom ?? "");
                            cmd.Parameters.AddWithValue("@TienDo", p.TienDo);
                            cmd.Parameters.AddWithValue("@TrangThai", p.TrangThai ?? "");
                            cmd.Parameters.AddWithValue("@KinhPhiDuToan", p.KinhPhiDuToan);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return string.Format("Đã cập nhật dự án [{0}] trên CSDL SQL Server!", cleanMa);
                }
                catch (Exception ex)
                {
                    return "Lỗi cập nhật dự án SQL Server: " + ex.Message;
                }
            }

            lock (_syncLock)
            {
                var target = _fallbackProjects.Find(x => string.Equals(x.MaDuAn, cleanMa, StringComparison.OrdinalIgnoreCase));
                if (target != null)
                {
                    target.TenDuAn = p.TenDuAn ?? target.TenDuAn;
                    target.TruongNhom = p.TruongNhom ?? target.TruongNhom;
                    target.TienDo = p.TienDo;
                    target.TrangThai = p.TrangThai ?? target.TrangThai;
                    target.KinhPhiDuToan = p.KinhPhiDuToan;
                    return string.Format("Đã cập nhật thông tin dự án [{0}] thành công!", cleanMa);
                }
            }

            return "Không tìm thấy dự án mã: " + cleanMa;
        }

        public string DeleteProject(string maDuAn)
        {
            if (string.IsNullOrWhiteSpace(maDuAn)) return "Mã dự án không hợp lệ!";
            string cleanMa = maDuAn.Trim().ToUpperInvariant();

            if (IsDbConnected)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM ERP_DuAn WHERE MaDuAn = @MaDuAn";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaDuAn", cleanMa);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return string.Format("Đã xóa dự án [{0}] khỏi CSDL SQL Server!", cleanMa);
                }
                catch (Exception ex)
                {
                    return "Lỗi xóa dự án SQL Server: " + ex.Message;
                }
            }

            lock (_syncLock)
            {
                int removed = _fallbackProjects.RemoveAll(x => string.Equals(x.MaDuAn, cleanMa, StringComparison.OrdinalIgnoreCase));
                if (removed > 0)
                    return string.Format("Đã xóa dự án [{0}] thành công!", cleanMa);
            }

            return "Không tìm thấy dự án mã: " + cleanMa;
        }

        // ======================== CRUD THÀNH VIÊN & PHÂN QUYỀN ========================

        public List<ThanhVienRole> GetCollaboratorRoles(string searchKeyword = null)
        {
            var list = new List<ThanhVienRole>();
            lock (_syncLock)
            {
                list.AddRange(_fallbackRoles);
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string kw = searchKeyword.Trim();
                list = list.FindAll(r =>
                    (r.HoTen != null && r.HoTen.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.VaiTroDuAn != null && r.VaiTroDuAn.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.QuyenGitHub != null && r.QuyenGitHub.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            return list;
        }

        public string AddRole(string hoTen, string vaiTro, string quyen, string moTa)
        {
            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(vaiTro))
                return "Vui lòng nhập đầy đủ Họ tên và Vai trò dự án!";

            lock (_syncLock)
            {
                int nextStt = _fallbackRoles.Count > 0 ? _fallbackRoles[_fallbackRoles.Count - 1].STT + 1 : 1;
                foreach (var r in _fallbackRoles)
                {
                    if (r.STT >= nextStt) nextStt = r.STT + 1;
                }

                _fallbackRoles.Add(new ThanhVienRole
                {
                    STT = nextStt,
                    HoTen = hoTen.Trim(),
                    VaiTroDuAn = vaiTro.Trim(),
                    QuyenGitHub = string.IsNullOrWhiteSpace(quyen) ? "Write" : quyen.Trim(),
                    MoTaTrachNhiem = moTa ?? ""
                });
            }

            return string.Format("Đã thêm thành viên [{0}] với vai trò [{1}]!", hoTen.Trim(), vaiTro.Trim());
        }

        public string UpdateRole(int stt, string hoTen, string vaiTro, string quyen, string moTa)
        {
            if (stt <= 0) return "STT thành viên không hợp lệ!";

            lock (_syncLock)
            {
                var target = _fallbackRoles.Find(x => x.STT == stt);
                if (target != null)
                {
                    target.HoTen = hoTen.Trim();
                    target.VaiTroDuAn = vaiTro.Trim();
                    target.QuyenGitHub = string.IsNullOrWhiteSpace(quyen) ? target.QuyenGitHub : quyen.Trim();
                    target.MoTaTrachNhiem = moTa ?? target.MoTaTrachNhiem;
                    return string.Format("Đã cập nhật thông tin thành viên #{0} ({1})!", stt, target.HoTen);
                }
            }

            return "Không tìm thấy thành viên có STT: " + stt;
        }

        public string DeleteRole(int stt)
        {
            if (stt <= 0) return "STT thành viên không hợp lệ!";

            lock (_syncLock)
            {
                int removed = _fallbackRoles.RemoveAll(x => x.STT == stt);
                if (removed > 0)
                    return string.Format("Đã xóa thành viên #{0} khỏi ma trận phân quyền!", stt);
            }

            return "Không tìm thấy thành viên có STT: " + stt;
        }
    }
}
