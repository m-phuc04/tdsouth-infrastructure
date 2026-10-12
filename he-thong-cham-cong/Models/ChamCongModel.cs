using System;
using System.Collections.Generic;

namespace TDSouth.ChamCong.Models
{
    /// <summary>
    /// Thực thể bản ghi chấm công nhân viên tại TDSouth Infrastructure
    /// Ánh xạ trực tiếp với bảng [dbo].[ChamCong] trên SQL Server (Database: TDSouth)
    /// </summary>
    public class ChamCongRecord
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
        public DateTime CreatedDate { get; set; }

        public ChamCongRecord()
        {
            MaNV = string.Empty;
            HoTen = string.Empty;
            PhongBan = string.Empty;
            CaLamViec = "Ca Hành Chính (08:00 - 17:30)";
            GioVao = "--:--";
            GioRa = "--:--";
            TrangThai = "Đúng giờ";
            NgayCham = DateTime.Now.ToString("dd/MM/yyyy");
            CreatedDate = DateTime.Now;
        }

        public bool IsWorkingNow()
        {
            return string.IsNullOrEmpty(GioRa) || GioRa == "--:--";
        }
    }

    /// <summary>
    /// Thông tin thành viên dự án theo ma trận phân quyền COLLABORATORS.md
    /// </summary>
    public class NhanVienCollaborator
    {
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public string PhongBan { get; set; }
        public string VaiTroDuAn { get; set; }
        public string QuyenGitHub { get; set; }

        public static List<NhanVienCollaborator> GetDefaultTeam()
        {
            return new List<NhanVienCollaborator>
            {
                new NhanVienCollaborator
                {
                    MaNV = "TDS001",
                    HoTen = "Lê Minh Phúc",
                    PhongBan = "Quản trị Hệ thống (Admin)",
                    VaiTroDuAn = "Project Owner / Admin",
                    QuyenGitHub = "Admin"
                },
                new NhanVienCollaborator
                {
                    MaNV = "TDS002",
                    HoTen = "Kỳ Nguyễn",
                    PhongBan = "Kỹ thuật Hạ tầng (Tech Lead)",
                    VaiTroDuAn = "Technical Lead",
                    QuyenGitHub = "Maintain"
                },
                new NhanVienCollaborator
                {
                    MaNV = "TDS003",
                    HoTen = "Trường Nguyễn",
                    PhongBan = "Phát triển Ứng dụng (Dev)",
                    VaiTroDuAn = "Developer",
                    QuyenGitHub = "Write"
                },
                new NhanVienCollaborator
                {
                    MaNV = "TDS004",
                    HoTen = "Tín Phan",
                    PhongBan = "Kiểm thử Chất lượng (QA)",
                    VaiTroDuAn = "QA / Tester",
                    QuyenGitHub = "Triage / Read"
                }
            };
        }
    }

    /// <summary>
    /// Đối tượng thống kê nhanh trên Dashboard Chấm Công
    /// </summary>
    public class ChamCongStatistics
    {
        public int TotalRecords { get; set; }
        public int OnTimeCount { get; set; }
        public int LateCount { get; set; }
        public int CurrentlyWorkingCount { get; set; }
        public bool IsDatabaseConnected { get; set; }
        public string DatabaseStatusMessage { get; set; }

        public static ChamCongStatistics Calculate(List<ChamCongRecord> records, bool dbConnected, string dbMsg)
        {
            var stats = new ChamCongStatistics
            {
                TotalRecords = records != null ? records.Count : 0,
                IsDatabaseConnected = dbConnected,
                DatabaseStatusMessage = dbMsg
            };

            if (records != null)
            {
                foreach (var item in records)
                {
                    if (string.Equals(item.TrangThai, "Đúng giờ", StringComparison.OrdinalIgnoreCase))
                        stats.OnTimeCount++;
                    else if (string.Equals(item.TrangThai, "Đi muộn", StringComparison.OrdinalIgnoreCase))
                        stats.LateCount++;

                    if (item.IsWorkingNow())
                        stats.CurrentlyWorkingCount++;
                }
            }

            return stats;
        }
    }
}

