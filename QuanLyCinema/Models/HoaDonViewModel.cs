using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyCinema.Models
{
    public class HoaDonViewModel
    {
        public int HoaDonId { get; set; }
        public string TenNguoiDung { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai     { get; set; }   
        public string TongTien { get;set; }
    }
}