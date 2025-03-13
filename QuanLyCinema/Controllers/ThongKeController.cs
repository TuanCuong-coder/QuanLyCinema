using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyCinema.Models;

namespace QuanLyCinema.Controllers
{
    public class ThongKeController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=TUANCUONG;Initial Catalog=QLCinema;Persist Security Info=True;User ID=sa;Password=123456;TrustServerCertificate=True");
        public ActionResult Index()
        {
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            var model = new ThongKeViewModel();
            model.DoanhThuBanVe = db.chitiethoadons.Where(ct => ct.hoadon.trangthai == 1)
                .Sum(ct => (decimal?)ct.gia) ?? 0;
            model.DoanhThuDoAn= db.chitietdoans
                .Where(ct => ct.hoadon.trangthai == 1)
                .Sum(ct => (decimal?)(ct.gia * ct.soluong)) ?? 0;
            model.TongDoanhThu = model.DoanhThuBanVe + model.DoanhThuDoAn;

            model.SoLuongVePhims= db.chitiethoadons.Where(ct => ct.hoadon.trangthai == 1).GroupBy(ct=>ct.ghe.phimid).Select(g=> new SoLuongVePhimModel
            {
                PhimId=g.Key,
                TenPhim=g.Select(x=>x.ghe.phim.tenphim).FirstOrDefault(),
                SoLuongVe=g.Count() //dem tong ve
            })
                .ToList();

            model.SoLuongDoAns= db.chitietdoans.Where(ct => ct.hoadon.trangthai == 1)
                .GroupBy(ct=>ct.doandid)
                .Select(g=>new SoLuongDoAnModel
                {
                    DoAnId=g.Key,
                    TenDoAn= g.Select(x=>x.doan.ten).FirstOrDefault(),
                    SoLuong= g.Sum(x=>x.soluong)
                })
                .ToList();

            return View(model);
        }
    }
}