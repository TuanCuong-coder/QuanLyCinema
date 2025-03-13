using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyCinema.Models;
using System.Transactions;
using QuanLyCinema.Filters;

namespace QuanLyCinema.Controllers
{
    [KiemTraThoiGianGiuGheFilter]
    public class HomeController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=TUANCUONG;Initial Catalog=QLCinema;Persist Security Info=True;User ID=sa;Password=123456;TrustServerCertificate=True");
        
        public ActionResult Index()//lay danh sach toan bo phim cua rap
        {
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            ViewBag.username = Session["username"];

            KiemTraThoiGianGiuGhe();
            var phimList = db.phims.ToList();
          
            return View(phimList);
        }

        
        public ActionResult ChiTietPhim(int id)//hien thi chi tiet phim, danhs sach cac ghe
        {
      
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            

            KiemTraThoiGianGiuGhe();
            var phimList  = db.phims.FirstOrDefault(p => p.id == id);
            if (phimList == null)
            {
                return HttpNotFound();
            }

            var gheList = db.ghes.Where(g => g.phimid == id).ToList();       

            ViewBag.PhimList = phimList;
            ViewBag.Ghe = gheList;

            return View(gheList) ; 
        }
        public ActionResult DatGhe (int gheId)        //dat ghe     
        {
           
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }

            KiemTraThoiGianGiuGhe();

            var ghe = db.ghes.FirstOrDefault(g => g.id == gheId);
            if (ghe!=null && ghe.trangthai == 0) //kiem tra trong 
            {
                ghe.trangthai = 1;// dang giu
                ghe.thoigianGiu = DateTime.Now; //luu thoi gian giu ghe
                db.SubmitChanges();
                Session["GheId"] = gheId; // luu id ghe vao ss

                return RedirectToAction("ChonMonAn");               

            }
            TempData["Error"] = "Ghế đã được giữ hoặc đặt. Vui lòng chọn ghế khác";
            return RedirectToAction("ChonMonAn", new { phimid = ghe.phimid }); 

        }

        public ActionResult ChonMonAn(int? phimid)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("DangNhap", "User");
            }
            KiemTraThoiGianGiuGhe();
            var gheId = (int?)Session["GheId"];
            var ghe = db.ghes.FirstOrDefault(g=>g.id==gheId);
            if (ghe== null|| ghe.trangthai != 1)
            {
                TempData["Error"] = "Hết thời gian giữ ghế. Vui lòng chọn lại ghế.";
                return RedirectToAction("ChiTietPhim", new { id = phimid ?? ghe?.phimid });

            }
            var doAnList = db.doans.ToList();

            // Thoi gian giu ghe 1 phut
            ViewBag.ExpireTime = ghe.thoigianGiu.Value.AddMinutes(1);
            ViewBag.PhimId = phimid ?? ghe?.phimid;

            return View(doAnList);
        }
        [HttpPost]
        public ActionResult ChonMonAn(int[] doAnIds, int[] quantities)
        {
            if (doAnIds != null  && quantities != null)
            {
                var doAnDuocChon = new List<KeyValuePair<int, int>>();
                for (int i = 0; i < doAnIds.Length; i++)
                {
                    if (quantities[i] > 0)
                    {
                        doAnDuocChon.Add(new KeyValuePair<int, int>(doAnIds[i], quantities[i]));
                    }
                }
                Session["doAnDuocChon"] = doAnDuocChon;
            }
          
            return RedirectToAction("ThanhToan"); //chuyen sang thanh toan
        }

        public ActionResult ThanhToan()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("DangNhap", "User");
            }
            KiemTraThoiGianGiuGhe();
            int gheId = (int)Session["GheId"];

            var ghe = db.ghes.FirstOrDefault(g => g.id == gheId);

            if (ghe == null || ghe.trangthai != 1)//het thoi gian 1phut
            {
                TempData["Error"] = "Thanh toán không thành công do hết thời gian chờ thanh toán, vui lòng chọn lại ghế";
                return RedirectToAction("ChiTietPhim", new { id = ghe?.phimid });
            }


         
            ViewBag.ExpireTime = ghe.thoigianGiu.Value.AddMinutes(1);  //thoi gian giu ghe 1phut


            var doAnDuocChon = Session["doAnDuocChon"] as List<KeyValuePair<int, int>>;
            var tongTien = ghe.phim.giave;
            var doAnList = db.doans.ToList();//lay danh sach mon an
            if (doAnDuocChon != null)
            {
                foreach(var item in doAnDuocChon)
                {
                    var doAn= db.doans.FirstOrDefault(d=>d.id == item.Key);
                    if (doAn != null)
                    {
                        tongTien += doAn.gia * item.Value;
                    }
                }
            }

            var viewModel = new ThanhToanViewModel
            {
                Ghe = ghe,
                ThoiGianGiu = ghe.thoigianGiu ?? DateTime.Now,
                DoAnDuocChon = doAnDuocChon,
                TongTien = tongTien,
                DoAnList = doAnList
            };

           
            return View(viewModel);

        }
     
        [HttpPost]
        public ActionResult XacNhanThanhToan() // xu ly thanh toan
        {
           
            if (Session["username"] == null || Session["GheId"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            int gheId = (int)Session["GheId"];
            var ghe= db.ghes.FirstOrDefault(g=>g.id== gheId);
            if (ghe == null || ghe.trangthai != 1)
            {
                TempData["Error"] = "Ghế không hợp lệ hoặc đã hết thời gian giữ.";
                return RedirectToAction("ChiTietPhim", new { id = ghe?.phimid });
            }

            int userId = (int)Session["userId"];

            using (var transactionScope = new TransactionScope())
            {
                try
                {
                    ghe.trangthai = 2;//da dat

                    //tao hoa don
                    var hoaDon = new hoadon
                    {
                        userid = userId,
                        ngaytao = DateTime.Now,
                        trangthai = 1 //da thanh toan
                    };

                    db.hoadons.InsertOnSubmit(hoaDon);
                    db.SubmitChanges();

                    //luu chi tiet hoa don vao ghe 
                    var chiTietHoaDon = new chitiethoadon
                    {
                        hoadonid = hoaDon.id,
                        gheid = gheId,
                        gia = ghe.phim.giave
                    };
                    db.chitiethoadons.InsertOnSubmit(chiTietHoaDon);

                    //luu chi tiet mon an neu co
                    var doAnDuocChon = Session["DoAnDuocChon"] as List<KeyValuePair<int, int>>;
                    if (doAnDuocChon != null)
                    {
                        foreach( var item in doAnDuocChon)
                        {
                            var doAn = db.doans.FirstOrDefault(d => d.id == item.Key);
                            if (doAn != null)
                            {
                                db.chitietdoans.InsertOnSubmit(new chitietdoan
                                {
                                    hoadonid = hoaDon.id,
                                    doandid = doAn.id,
                                    soluong = item.Value,
                                    gia = doAn.gia * item.Value
                                });
                            }
                        }
                    }
                    db.SubmitChanges();
                    transactionScope.Complete();
                    return RedirectToAction("HienThiHoaDon", new { id = hoaDon.id });
                }                  
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                    return RedirectToAction("ChiTietPhim", new { id = ghe?.phimid });
                }
            
            }
        }


        public ActionResult HienThiHoaDon(int id)
        {
            var hoaDon = db.hoadons.FirstOrDefault(h => h.id == id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }

            var chiTietHoaDon = db.chitiethoadons
                                      .Where(ct => ct.hoadonid == hoaDon.id)
                                      .ToList();

           var chiTietDoAn= db.chitietdoans.Where(ct => ct.hoadonid==id).ToList();

            decimal tongTien = 0;
            tongTien += chiTietHoaDon.Sum(ct => ct.gia); //tong tien ghe
            tongTien += chiTietDoAn.Sum(ct => ct.gia);   //tong tien mon an

            var phim = chiTietHoaDon.FirstOrDefault()?.ghe.phim;

            ViewBag.ChiTietHoaDon = chiTietHoaDon;
            ViewBag.ChiTietDoAn = chiTietDoAn;
            ViewBag.TongTien = tongTien;
            ViewBag.TenPhim = phim?.tenphim ?? "Không xác định";
            ViewBag.NgayChieu = phim?.thoigian.ToString("dd/MM/yyyy") ?? "Không xác định";
            ViewBag.TrangThai = hoaDon.trangthai == 1 ? "Đã thanh toán" : "Chưa thanh toán";
            return View(hoaDon);
        }

        //huy thanh toan
        [HttpPost]
        public ActionResult HuyThanhToan(int[] gheIds)
        {
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
           
            foreach (var gheId in gheIds)
            {
                var ghe = db.ghes.FirstOrDefault(g => g.id == gheId);

                if (ghe!= null && ghe.trangthai==1)
                {
                    ghe.trangthai = 0; //trong
                    ghe.thoigianGiu = null;
                    ghe.hoadonid = null;
                }
            }
            db.SubmitChanges();
            TempData["Success"] = "Hủy đặt ghế thành công.";
            return RedirectToAction("Index");
        }

        public void KiemTraThoiGianGiuGhe()
        {
            var now = DateTime.Now;
            var gheList = db.ghes.Where(g => g.trangthai == 1).ToList(); //lay cac ghe dang giu

            foreach (var ghe in gheList)
            {
                if (ghe.thoigianGiu.HasValue && (now - ghe.thoigianGiu.Value).TotalMinutes > 1) //1 phut thanh toan
                {
                    ghe.trangthai = 0; //ghe trong
                    ghe.thoigianGiu = null; // Xoa tgian giu ghe
                    ghe.hoadonid = null; // xoa lien ket vs hoa don              
                }
            }
            db.SubmitChanges();
        }

        public ActionResult TimKiemPhim(string TuKhoa)
        {
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            ViewBag.username = Session["username"];
            KiemTraThoiGianGiuGhe();

            List<phim> phimList;
            if (string.IsNullOrWhiteSpace(TuKhoa))
            {
                phimList = db.phims.ToList();
            }
            else
            {
                phimList = db.phims.Where(p => p.tenphim.Contains(TuKhoa)).ToList();
            }

            ViewBag.TuKhoa = TuKhoa;
            return View("Index", phimList);
        }
    }
}