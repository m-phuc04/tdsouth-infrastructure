using System;
using System.Collections.Generic;

namespace TDSouth.Erp.Models
{
    /// <summary>
    /// Thực thể Thiết bị &amp; Vật tư Hạ tầng CNTT (Ánh xạ bảng [dbo].[ERP_ThietBi] trên SQL Server TDSouth)
    /// </summary>
    public class ThietBiAsset
    {
        public int ID { get; set; }
        public string MaTB { get; set; }
        public string TenTB { get; set; }
        public string LoaiTB { get; set; }
        public int SoLuong { get; set; }
        public long DonGia { get; set; }
        public string TrangThai { get; set; }
        public DateTime CreatedDate { get; set; }

        public long ThanhTien
        {
            get { return (long)SoLuong * DonGia; }
        }

        public ThietBiAsset()
        {
            MaTB = string.Empty;
            TenTB = string.Empty;
            LoaiTB = "Máy Chủ (Server)";
            SoLuong = 1;
            DonGia = 0;
            TrangThai = "Đang sử dụng";
            CreatedDate = DateTime.Now;
        }
    }

    /// <summary>
    /// Thực thể Dự án Hạ tầng CNTT thuộc phòng Lab TDSouth Infrastructure
    /// </summary>
    public class DuAnProject
    {
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public string TruongNhom { get; set; }
        public int TienDo { get; set; }
        public string TrangThai { get; set; }
        public long KinhPhiDuToan { get; set; }
    }

    /// <summary>
    /// Thực thể Ma trận phân quyền thành viên dự án (Trích xuất từ COLLABORATORS.md)
    /// </summary>
    public class ThanhVienRole
    {
        public int STT { get; set; }
        public string HoTen { get; set; }
        public string VaiTroDuAn { get; set; }
        public string QuyenGitHub { get; set; }
        public string MoTaTrachNhiem { get; set; }
    }

    /// <summary>
    /// Đối tượng tổng hợp chỉ số KPI điều hành trên Dashboard ERP
    /// </summary>
    public class ErpDashboardSummary
    {
        public int TotalAssetCategories { get; set; }
        public int TotalAssetUnits { get; set; }
        public long TotalAssetValue { get; set; }
        public int TotalProjects { get; set; }
        public string SystemSla { get; set; }
        public bool IsDbConnected { get; set; }
        public string DbStatusMessage { get; set; }

        public static ErpDashboardSummary Calculate(
            List<ThietBiAsset> assets,
            List<DuAnProject> projects,
            bool dbConnected,
            string dbMessage)
        {
            var summary = new ErpDashboardSummary
            {
                TotalAssetCategories = assets != null ? assets.Count : 0,
                TotalProjects = projects != null ? projects.Count : 0,
                SystemSla = "99.98%",
                IsDbConnected = dbConnected,
                DbStatusMessage = dbMessage
            };

            if (assets != null)
            {
                foreach (var a in assets)
                {
                    summary.TotalAssetUnits += a.SoLuong;
                    summary.TotalAssetValue += a.ThanhTien;
                }
            }

            return summary;
        }
    }
}

