using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyCinema.Models;

namespace QuanLyCinema.Controllers
{
    public class HoaDonController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=TUANCUONG;Initial Catalog=QLCinema;Persist Security Info=True;User ID=sa;Password=123456;TrustServerCertificate=True");
        public ActionResult DanhSachHoaDon()
        {
            var userId = (int)Session["userId"];
            var danhSachHoaDon = (from h in db.hoadons
                                  join n in db.nguoidungs on h.userid equals n.id
                                  where h.userid == userId
                                  select new HoaDonViewModel
                                  {
                                      HoaDonId = h.id,
                                      TenNguoiDung = n.username,
                                      NgayTao = h.ngaytao,
                                      TrangThai = h.trangthai == 1 ? "Đã thanh toán" :
                                      h.trangthai == 0 ? "Hủy" : "Chưa thanh toán"                                     
                                  }).ToList();

            return View(danhSachHoaDon);
        }
    }
}