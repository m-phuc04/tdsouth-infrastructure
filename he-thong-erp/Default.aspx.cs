using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace TDSouth.Erp
{
    public class AssetViewModel
    {
        public int ID { get; set; }
        public string MaTB { get; set; }
        public string TenTB { get; set; }
        public string LoaiTB { get; set; }
        public int SoLuong { get; set; }
        public long DonGia { get; set; }
        public string TrangThai { get; set; }

        public long ThanhTien
        {
            get { return (long)SoLuong * DonGia; }
        }
    }

    public class ProjectViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Lead { get; set; }
        public int Progress { get; set; }
        public string Status { get; set; }
        public long Budget { get; set; }
    }

    /// <summary>
    /// Code-Behind C# (ASP.NET WebForms) cho Hệ Thống ERP Doanh Nghiệp TDSouth Infrastructure
    /// Kết nối trực tiếp tới SQL Server (localhost\SQLEXPRESS - DB: TDSouth - Bảng: ERP_ThietBi)
    /// </summary>
    public partial class DefaultPage : Page
    {
        protected bool DbConnected = false;
        protected string DbStatusMessage = "";
        protected string ActionMessage = "";
        protected List<AssetViewModel> AssetList = new List<AssetViewModel>();
        protected List<ProjectViewModel> ProjectList = new List<ProjectViewModel>();

        protected long TotalAssetValue = 0;
        protected int TotalAssetUnits = 0;

        private string GetConnectionString()
        {
            if (ConfigurationManager.ConnectionStrings["TDSouthDb"] != null)
            {
                return ConfigurationManager.ConnectionStrings["TDSouthDb"].ConnectionString;
            }
            return @"Data Source=localhost\SQLEXPRESS;Initial Catalog=TDSouth;User ID=sa;Password=admin@123;Connect Timeout=2;MultipleActiveResultSets=True";
        }

        protected string FormatCurrency(long amount)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:N0} ₫", amount).Replace(",", ".");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            string connStr = GetConnectionString();

            InitializeSqlDatabase(connStr);

            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                if (Request.Form["btnThemTB"] != null)
                {
                    HandleAddAsset(connStr);
                }
            }

            LoadAssets(connStr);
            LoadProjects();
        }

        private void InitializeSqlDatabase(string connStr)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string checkTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ERP_ThietBi' AND xtype='U')
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
                    using (SqlCommand cmd = new SqlCommand(checkTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    DbConnected = true;
                    DbStatusMessage = "C# SQL Server Connected (localhost\\SQLEXPRESS - TDSouth)";
                }
            }
            catch (Exception)
            {
                DbConnected = false;
                DbStatusMessage = "C# ASP.NET Demo Mode (SQL Server Offline)";
            }
        }

        private List<AssetViewModel> GetSessionFallbackAssets()
        {
            List<AssetViewModel> list = Session["TDSouth_Erp_Assets"] as List<AssetViewModel>;
            if (list == null)
            {
                list = new List<AssetViewModel>
                {
                    new AssetViewModel { ID = 1, MaTB = "SRV-01", TenTB = "Dell PowerEdge R740 Rack Server (2x Xeon Gold, 128GB RAM)", LoaiTB = "Máy Chủ (Server)", SoLuong = 2, DonGia = 145000000L, TrangThai = "Đang sử dụng" },
                    new AssetViewModel { ID = 2, MaTB = "SW-CORE", TenTB = "Cisco Catalyst 2960-X 48 Port Gigabit Switch", LoaiTB = "Thiết Bị Mạng (Switch)", SoLuong = 4, DonGia = 28500000L, TrangThai = "Đang sử dụng" },
                    new AssetViewModel { ID = 3, MaTB = "FW-01", TenTB = "Tường Lửa Fortinet FortiGate 60F UTM Appliance", LoaiTB = "Bảo Mật (Firewall)", SoLuong = 1, DonGia = 35000000L, TrangThai = "Đang sử dụng" },
                    new AssetViewModel { ID = 4, MaTB = "NAS-01", TenTB = "Synology DiskStation DS920+ 4-Bay (4x 8TB IronWolf)", LoaiTB = "Lưu Trữ (Storage)", SoLuong = 2, DonGia = 32000000L, TrangThai = "Trong kho" },
                    new AssetViewModel { ID = 5, MaTB = "CAB-01", TenTB = "Thùng Cáp Mạng CommScope Cat6 UTP 305m Chính Hãng", LoaiTB = "Vật Tư Phụ", SoLuong = 10, DonGia = 2800000L, TrangThai = "Trong kho" },
                    new AssetViewModel { ID = 6, MaTB = "LIC-01", TenTB = "Microsoft Windows Server 2022 Datacenter 16-Core License", LoaiTB = "Bản Quyền (License)", SoLuong = 3, DonGia = 42000000L, TrangThai = "Đang sử dụng" }
                };
                Session["TDSouth_Erp_Assets"] = list;
            }
            return list;
        }

        private void HandleAddAsset(string connStr)
        {
            string maTB = (Request.Form["matb"] ?? "").Trim().ToUpperInvariant();
            string tenTB = (Request.Form["tentb"] ?? "").Trim();
            string loaiTB = (Request.Form["loaitb"] ?? "Máy Chủ (Server)").Trim();
            string trangThai = (Request.Form["trangthai"] ?? "Đang sử dụng").Trim();

            int soLuong = 1;
            int.TryParse(Request.Form["soluong"], out soLuong);
            if (soLuong <= 0) soLuong = 1;

            long donGia = 0;
            long.TryParse(Request.Form["dongia"], out donGia);
            if (donGia < 0) donGia = 0;

            if (string.IsNullOrEmpty(maTB) || string.IsNullOrEmpty(tenTB))
            {
                ActionMessage = "Vui lòng nhập đầy đủ Mã thiết bị và Tên thiết bị!";
                return;
            }

            if (DbConnected)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string insertSql = @"INSERT INTO ERP_ThietBi (MaTB, TenTB, LoaiTB, SoLuong, DonGia, TrangThai)
                                             VALUES (@MaTB, @TenTB, @LoaiTB, @SoLuong, @DonGia, @TrangThai)";
                        using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaTB", maTB);
                            cmd.Parameters.AddWithValue("@TenTB", tenTB);
                            cmd.Parameters.AddWithValue("@LoaiTB", loaiTB);
                            cmd.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmd.Parameters.AddWithValue("@DonGia", donGia);
                            cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    ActionMessage = string.Format("Đã thêm thiết bị '{0}' ({1}) vào CSDL SQL Server thành công!", tenTB, maTB);
                    return;
                }
                catch (Exception ex)
                {
                    ActionMessage = "Lỗi SQL Server: " + ex.Message;
                    return;
                }
            }

            List<AssetViewModel> mem = GetSessionFallbackAssets();
            mem.Insert(0, new AssetViewModel
            {
                ID = mem.Count + 1,
                MaTB = maTB,
                TenTB = tenTB,
                LoaiTB = loaiTB,
                SoLuong = soLuong,
                DonGia = donGia,
                TrangThai = trangThai
            });
            ActionMessage = string.Format("Đã thêm thiết bị '{0}' ({1}) vào hệ thống C# Session!", tenTB, maTB);
        }

        private void LoadAssets(string connStr)
        {
            AssetList = new List<AssetViewModel>();

            if (DbConnected)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string selectSql = "SELECT ID, MaTB, TenTB, LoaiTB, SoLuong, DonGia, TrangThai FROM ERP_ThietBi ORDER BY ID DESC";
                        using (SqlCommand cmd = new SqlCommand(selectSql, conn))
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AssetList.Add(new AssetViewModel
                                {
                                    ID = reader["ID"] != DBNull.Value ? Convert.ToInt32(reader["ID"]) : 0,
                                    MaTB = Convert.ToString(reader["MaTB"]),
                                    TenTB = Convert.ToString(reader["TenTB"]),
                                    LoaiTB = Convert.ToString(reader["LoaiTB"]),
                                    SoLuong = reader["SoLuong"] != DBNull.Value ? Convert.ToInt32(reader["SoLuong"]) : 1,
                                    DonGia = reader["DonGia"] != DBNull.Value ? Convert.ToInt64(reader["DonGia"]) : 0L,
                                    TrangThai = Convert.ToString(reader["TrangThai"])
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

            if (AssetList.Count == 0)
            {
                AssetList = new List<AssetViewModel>(GetSessionFallbackAssets());
            }

            TotalAssetValue = 0;
            TotalAssetUnits = 0;
            foreach (AssetViewModel a in AssetList)
            {
                TotalAssetValue += a.ThanhTien;
                TotalAssetUnits += a.SoLuong;
            }
        }

        private void LoadProjects()
        {
            ProjectList = new List<ProjectViewModel>
            {
                new ProjectViewModel { Code = "PRJ-AD01", Name = "Triển khai Hệ thống Active Directory Domain Controller & GPO", Lead = "Nguyễn Văn A", Progress = 100, Status = "Hoàn thành", Budget = 45000000L },
                new ProjectViewModel { Code = "PRJ-DNS02", Name = "Cấu hình Dịch vụ Mạng DNS & DHCP Failover Lab", Lead = "Kỳ Nguyễn", Progress = 85, Status = "Đang triển khai", Budget = 30000000L },
                new ProjectViewModel { Code = "PRJ-IIS03", Name = "Xây dựng IIS Web Server (C# ASP.NET) & Chứng chỉ SSL/HTTPS", Lead = "Trường Nguyễn", Progress = 95, Status = "Đang hoàn thiện", Budget = 25000000L },
                new ProjectViewModel { Code = "PRJ-SEC04", Name = "Kiểm thử An toàn Thông tin & Triển khai FTPS/File Server", Lead = "Tín Phan", Progress = 75, Status = "Đang kiểm thử", Budget = 20000000L }
            };
        }
    }
}

