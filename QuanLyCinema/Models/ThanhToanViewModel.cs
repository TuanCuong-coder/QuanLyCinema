using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyCinema.Models
{
    public class ThanhToanViewModel
    {
        public ghe Ghe { get; set; }
        public DateTime? ThoiGianGiu { get; set; }
        public List<KeyValuePair<int, int>> DoAnDuocChon { get; set; }
        public List<doan> DoAnList { get; set; } // Thêm danh sách món ăn
        public decimal TongTien { get; set; }
        public string ThoiGianConLai { get; set; }//thoi gian đếm ngược dạng chuỗi 
    }

}