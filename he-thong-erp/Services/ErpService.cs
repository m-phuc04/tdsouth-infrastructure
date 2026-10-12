using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TDSouth.Erp.Models;

namespace TDSouth.Erp.Services
{
    /// <summary>
    /// Dịch vụ kết nối CSDL Microsoft SQL Server (TDSouth) và quản trị nguồn lực doanh nghiệp (ERP)
    /// Tự động khởi tạo bảng [ERP_ThietBi] và hỗ trợ chế độ In-Memory khi SQL Server chưa bật
    /// </summary>
    public class ErpService
    {
        public const string DefaultConnectionString =
            @"Data Source=localhost\SQLEXPRESS;Initial Catalog=TDSouth;User ID=sa;Password=admin@123;Connect Timeout=2;MultipleActiveResultSets=True";

        private readonly string _connectionString;
        private static readonly object _syncLock = new object();
        private static List<ThietBiAsset> _fallbackAssets;

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
                if (_fallbackAssets != null)
                    return;

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
                        MaTB = "FW-01",
                        TenTB = "Tường Lửa Fortinet FortiGate 60F UTM Appliance",
                        LoaiTB = "Bảo Mật (Firewall)",
                        SoLuong = 1,
                        DonGia = 35000000L,
                        TrangThai = "Đang sử dụng"
                    },
                    new ThietBiAsset
                    {
                        ID = 4,
                        MaTB = "NAS-01",
                        TenTB = "Synology DiskStation DS920+ 4-Bay (4x 8TB IronWolf)",
                        LoaiTB = "Lưu Trữ (Storage)",
                        SoLuong = 2,
                        DonGia = 32000000L,
                        TrangThai = "Trong kho"
                    },
                    new ThietBiAsset
                    {
                        ID = 5,
                        MaTB = "CAB-01",
                        TenTB = "Thùng Cáp Mạng CommScope Cat6 UTP 305m Chính Hãng",
                        LoaiTB = "Vật Tư Phụ",
                        SoLuong = 10,
                        DonGia = 2800000L,
                        TrangThai = "Trong kho"
                    },
                    new ThietBiAsset
                    {
                        ID = 6,
                        MaTB = "LIC-01",
                        TenTB = "Microsoft Windows Server 2022 Datacenter 16-Core License",
                        LoaiTB = "Bản Quyền (License)",
                        SoLuong = 3,
                        DonGia = 42000000L,
                        TrangThai = "Đang sử dụng"
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
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ERP_ThietBi' AND xtype='U')
                        BEGIN
                            CREATE TABLE ERP_ThietBi (
                                ID INT IDENTITY(1,1) PRIMARY KEY,
                                MaTB NVARCHAR(50) NOT NULL,
                                TenTB NVARCHAR(150) NOT NULL,
                                LoaiTB NVARCHAR(50) NULL,
                                SoLuong INT DEFAULT 1,
                                DonGia BIGINT DEFAULT 0,
                                TrangThai NVARCHAR(50) NULL,
                                CreatedDate DATETIME DEFAULT GETDATE()
                            );
                        END";

                    using (var cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    IsDbConnected = true;
                    DbStatusMessage = "C# SQL Server Connected (localhost\\SQLEXPRESS - DB: TDSouth)";
                }
            }
            catch (Exception)
            {
                IsDbConnected = false;
                DbStatusMessage = "C# ASP.NET In-Memory Mode (SQL Server localhost\\SQLEXPRESS đang tắt)";
            }
        }

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
                int nextId = _fallbackAssets.Count + 1;
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

            return string.Format("Đã thêm thiết bị [{0}] - {1} (Số lượng: {2}) vào hệ thống C# ERP!", cleanMa, cleanTen, soLuong);
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

        public List<DuAnProject> GetProjects()
        {
            return new List<DuAnProject>
            {
                new DuAnProject
                {
                    MaDuAn = "PRJ-AD01",
                    TenDuAn = "Triển khai Hệ thống Active Directory Domain Controller & GPO",
                    TruongNhom = "Lê Minh Phúc",
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

        public List<ThanhVienRole> GetCollaboratorRoles()
        {
            return new List<ThanhVienRole>
            {
                new ThanhVienRole
                {
                    STT = 1,
                    HoTen = "Lê Minh Phúc",
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

