using System;
using System.Collections.Generic;

namespace AdminDashboard.Models.ViewModels
{
    public class LichLamViecViewModel
    {
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }  

        public List<TaiXeLichRow> TaiXes { get; set; }
        public List<ChuyenXeShort> UnassignedTrips { get; set; }
    }

    public class TaiXeLichRow
    {
        public string TaiXeId { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public Dictionary<string, List<ChuyenXeShort>> LichTrinh { get; set; }
    }
    public class ChuyenXeShort
    {
        public string ChuyenId { get; set; }
        public string BienSoXe { get; set; }
        public string Tuyen { get; set; } 
        public string GioDi { get; set; }
        public int TrangThai { get; set; }
    }
}