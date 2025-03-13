using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyCinema.Models
{
    public class ThongKeViewModel
    {
        public decimal DoanhThuBanVe { get; set; }    
        public decimal DoanhThuDoAn { get; set; }     
        public decimal TongDoanhThu { get; set; }
        public List<SoLuongVePhimModel> SoLuongVePhims { get; set; }
        public List<SoLuongDoAnModel> SoLuongDoAns { get; set; }

    }
    public class SoLuongVePhimModel
    {
        public int PhimId { get; set; }
        public string TenPhim { get; set; }
        public int SoLuongVe { get; set; }
    }
    public class SoLuongDoAnModel
    {
        public int DoAnId { get; set; }
        public string TenDoAn { get; set; }
        public int SoLuong { get; set; }
    }
}