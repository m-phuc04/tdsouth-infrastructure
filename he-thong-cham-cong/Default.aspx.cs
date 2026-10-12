using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace TDSouth.ChamCong
{
    public class AttendanceItem
    {
        public int ID { get; set; }
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public string PhongBan { get; set; }
        public string CaLamViec { get; set; }
        public string GioVao { get; set; }
        public string GioRa { get; set; }
        public string TrangThai { get; set; }
        public string NgayCham { get; set; }
    }

    /// <summary>
    /// Code-Behind C# (ASP.NET WebForms) cho Hệ Thống Chấm Công TDSouth Infrastructure
    /// Chạy trực tiếp trên IIS Web Server và kết nối SQL Server (localhost\SQLEXPRESS - DB: TDSouth)
    /// </summary>
    public partial class DefaultPage : Page
    {
        protected bool DbConnected = false;
        protected string DbStatusMessage = "";
        protected string ActionMessage = "";
        protected string SearchQuery = "";
        protected List<AttendanceItem> AttendanceList = new List<AttendanceItem>();

        protected int TotalRecords = 0;
        protected int OnTimeCount = 0;
        protected int LateCount = 0;
        protected int WorkingNowCount = 0;

        private string GetConnectionString()
        {
            if (ConfigurationManager.ConnectionStrings["TDSouthDb"] != null)
            {
                return ConfigurationManager.ConnectionStrings["TDSouthDb"].ConnectionString;
            }
            return @"Data Source=localhost\SQLEXPRESS;Initial Catalog=TDSouth;User ID=sa;Password=admin@123;Connect Timeout=2;MultipleActiveResultSets=True";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            SearchQuery = (Request.QueryString["q"] ?? "").Trim();

            string connStr = GetConnectionString();
            InitializeSqlDatabase(connStr);

            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                string action = (Request.Form["action"] ?? "").Trim();
                if (!string.IsNullOrEmpty(action))
                {
                    HandleAttendanceAction(connStr, action);
                }
            }

            LoadAttendanceData(connStr, SearchQuery);
            CalculateStatistics();
        }

        private void InitializeSqlDatabase(string connStr)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string checkTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChamCong' AND xtype='U')
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

        private List<AttendanceItem> GetSessionFallbackList()
        {
            List<AttendanceItem> list = Session["TDSouth_ChamCong_List"] as List<AttendanceItem>;
            if (list == null)
            {
                string today = DateTime.Now.ToString("dd/MM/yyyy");
                list = new List<AttendanceItem>
                {
                    new AttendanceItem { ID = 1, MaNV = "TDS001", HoTen = "Lê Minh Phúc", PhongBan = "Quản trị Hệ thống (Admin)", CaLamViec = "Ca Hành Chính (08:00 - 17:30)", GioVao = "07:55:12", GioRa = "17:35:40", TrangThai = "Đúng giờ", NgayCham = today },
                    new AttendanceItem { ID = 2, MaNV = "TDS002", HoTen = "Kỳ Nguyễn", PhongBan = "Kỹ thuật Hạ tầng (Tech Lead)", CaLamViec = "Ca Hành Chính (08:00 - 17:30)", GioVao = "08:04:20", GioRa = "17:30:10", TrangThai = "Đúng giờ", NgayCham = today },
                    new AttendanceItem { ID = 3, MaNV = "TDS003", HoTen = "Trường Nguyễn", PhongBan = "Phát triển Ứng dụng (Dev)", CaLamViec = "Ca Sáng (08:00 - 12:00)", GioVao = "08:18:45", GioRa = "12:05:00", TrangThai = "Đi muộn", NgayCham = today },
                    new AttendanceItem { ID = 4, MaNV = "TDS004", HoTen = "Tín Phan", PhongBan = "Kiểm thử Chất lượng (QA)", CaLamViec = "Ca Hành Chính (08:00 - 17:30)", GioVao = "08:00:05", GioRa = "--:--", TrangThai = "Đúng giờ", NgayCham = today }
                };
                Session["TDSouth_ChamCong_List"] = list;
            }
            return list;
        }

        private void HandleAttendanceAction(string connStr, string actionType)
        {
            string maNV = (Request.Form["manv"] ?? "").Trim().ToUpperInvariant();
            string hoTen = (Request.Form["hoten"] ?? "").Trim();
            string phongBan = (Request.Form["phongban"] ?? "Quản trị Hệ thống (Admin)").Trim();
            string caLamViec = (Request.Form["calamviec"] ?? "Ca Hành Chính (08:00 - 17:30)").Trim();

            DateTime now = DateTime.Now;
            string currentTime = now.ToString("HH:mm:ss");
            string currentDate = now.ToString("dd/MM/yyyy");

            if (string.Equals(actionType, "checkin", StringComparison.OrdinalIgnoreCase))
            {
                string trangThai = (now.Hour < 8 || (now.Hour == 8 && now.Minute <= 15)) ? "Đúng giờ" : "Đi muộn";

                if (DbConnected)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();
                            string insertSql = @"INSERT INTO ChamCong (MaNV, HoTen, PhongBan, CaLamViec, GioVao, GioRa, TrangThai, NgayCham)
                                                 VALUES (@MaNV, @HoTen, @PhongBan, @CaLamViec, @GioVao, @GioRa, @TrangThai, @NgayCham)";
                            using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@MaNV", maNV);
                                cmd.Parameters.AddWithValue("@HoTen", hoTen);
                                cmd.Parameters.AddWithValue("@PhongBan", phongBan);
                                cmd.Parameters.AddWithValue("@CaLamViec", caLamViec);
                                cmd.Parameters.AddWithValue("@GioVao", currentTime);
                                cmd.Parameters.AddWithValue("@GioRa", "--:--");
                                cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                                cmd.Parameters.AddWithValue("@NgayCham", currentDate);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        ActionMessage = string.Format("Check-in C# SQL Server thành công: {0} ({1}) lúc {2} [{3}]!", hoTen, maNV, currentTime, trangThai);
                        return;
                    }
                    catch (Exception ex)
                    {
                        ActionMessage = "Lỗi SQL Server: " + ex.Message;
                        return;
                    }
                }

                List<AttendanceItem> mem = GetSessionFallbackList();
                mem.Insert(0, new AttendanceItem
                {
                    ID = mem.Count + 1,
                    MaNV = maNV,
                    HoTen = hoTen,
                    PhongBan = phongBan,
                    CaLamViec = caLamViec,
                    GioVao = currentTime,
                    GioRa = "--:--",
                    TrangThai = trangThai,
                    NgayCham = currentDate
                });
                ActionMessage = string.Format("Check-in thành công (C# Session): {0} ({1}) lúc {2} [{3}]!", hoTen, maNV, currentTime, trangThai);
            }
            else if (string.Equals(actionType, "checkout", StringComparison.OrdinalIgnoreCase))
            {
                if (DbConnected)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();
                            string updateSql = @"UPDATE ChamCong SET GioRa = @GioRa WHERE ID = (
                                                    SELECT TOP 1 ID FROM ChamCong
                                                    WHERE MaNV = @MaNV AND NgayCham = @NgayCham
                                                    ORDER BY ID DESC
                                                 )";
                            using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@GioRa", currentTime);
                                cmd.Parameters.AddWithValue("@MaNV", maNV);
                                cmd.Parameters.AddWithValue("@NgayCham", currentDate);
                                int affected = cmd.ExecuteNonQuery();
                                if (affected > 0)
                                    ActionMessage = string.Format("Check-out C# SQL Server thành công cho nhân viên {0} ({1}) lúc {2}!", hoTen, maNV, currentTime);
                                else
                                    ActionMessage = string.Format("Không tìm thấy lượt Check-in hôm nay của mã {0}!", maNV);
                            }
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        ActionMessage = "Lỗi cập nhật Check-out: " + ex.Message;
                        return;
                    }
                }

                List<AttendanceItem> mem = GetSessionFallbackList();
                foreach (AttendanceItem item in mem)
                {
                    if (string.Equals(item.MaNV, maNV, StringComparison.OrdinalIgnoreCase))
                    {
                        item.GioRa = currentTime;
                        ActionMessage = string.Format("Check-out thành công (C# Session): {0} ({1}) lúc {2}!", item.HoTen, maNV, currentTime);
                        return;
                    }
                }
                ActionMessage = string.Format("Không tìm thấy mã nhân viên {0} trong danh sách ca trực!", maNV);
            }
        }

        private void LoadAttendanceData(string connStr, string keyword)
        {
            AttendanceList = new List<AttendanceItem>();

            if (DbConnected)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string selectSql = "SELECT TOP 50 ID, MaNV, HoTen, PhongBan, CaLamViec, GioVao, GioRa, TrangThai, NgayCham FROM ChamCong ORDER BY ID DESC";
                        using (SqlCommand cmd = new SqlCommand(selectSql, conn))
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AttendanceList.Add(new AttendanceItem
                                {
                                    ID = reader["ID"] != DBNull.Value ? Convert.ToInt32(reader["ID"]) : 0,
                                    MaNV = Convert.ToString(reader["MaNV"]),
                                    HoTen = Convert.ToString(reader["HoTen"]),
                                    PhongBan = Convert.ToString(reader["PhongBan"]),
                                    CaLamViec = Convert.ToString(reader["CaLamViec"]),
                                    GioVao = Convert.ToString(reader["GioVao"]),
                                    GioRa = Convert.ToString(reader["GioRa"]),
                                    TrangThai = Convert.ToString(reader["TrangThai"]),
                                    NgayCham = Convert.ToString(reader["NgayCham"])
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Fallback to session list
                }
            }

            if (AttendanceList.Count == 0)
            {
                AttendanceList = new List<AttendanceItem>(GetSessionFallbackList());
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                AttendanceList = AttendanceList.FindAll(delegate(AttendanceItem x)
                {
                    return (x.MaNV != null && x.MaNV.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           (x.HoTen != null && x.HoTen.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           (x.PhongBan != null && x.PhongBan.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
                });
            }
        }

        private void CalculateStatistics()
        {
            TotalRecords = AttendanceList.Count;
            OnTimeCount = 0;
            LateCount = 0;
            WorkingNowCount = 0;

            foreach (AttendanceItem item in AttendanceList)
            {
                if (item.TrangThai == "Đúng giờ") OnTimeCount++;
                if (item.TrangThai == "Đi muộn") LateCount++;
                if (string.IsNullOrEmpty(item.GioRa) || item.GioRa == "--:--") WorkingNowCount++;
            }
        }
    }
}

