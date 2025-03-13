using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyCinema.Models;

namespace QuanLyCinema.Controllers
{
    public class PhimController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=TUANCUONG;Initial Catalog=QLCinema;Persist Security Info=True;User ID=sa;Password=123456;TrustServerCertificate=True");
        public ActionResult Index()
        {
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            return View();
        }

        [HttpPost]
        public JsonResult Paging(string searchQuery = "", int pageSize = 2, int crrPage = 1)
        {
            List<phim> phims = db.phims.ToList();
            List<phim> phimPaging = db.phims
                .Where(o => o.tenphim.Contains(searchQuery))
                 .OrderBy(o => o.tenphim) //sap xep theo ten phim,
                .Skip((crrPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            int totalPage = 0;
            if (searchQuery == "")
            {
                totalPage = (int)Math.Ceiling((double)db.phims.Count() / pageSize);
            }
            else
            {
                totalPage = (int)Math.Ceiling((double)phims.Count() / pageSize);
            }
            var results = phimPaging.Select(phim => new
            {
                phim.id,
                phim.tenphim,
                TheLoai = phim.theloai.ten, // lay ten danh muc thay vi id 
                thoigian = phim.thoigian.ToString("yyyy-MM-ddTHH:mm:ss"),
               
                phim.giave,
                phim.mota,
                phim.hinhanh
                

            });

            return Json(new
            {
                data = results,
                success = true,
                message = "Lấy dữ liệu thành công",
                crrPage = crrPage,
                pageSize = pageSize,
                totalPage = totalPage
            });
        }

        public ActionResult Create()
        {
            if (Session["Username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            ViewBag.TheLoai = db.theloais.ToList(); // lay danh sach the loai
            return View();

        }

        [HttpPost]
        public JsonResult PostCreate(HttpPostedFileBase hinhanh, HttpPostedFileBase filesp)
        {

            try
            {//request cac gia tri tu form
                string tenPhim = Request.Form["tenphim"];
                string thoigian = Request.Form["thoigian"];
                decimal giave = Convert.ToDecimal(Request.Form["giave"]);
                string mota = Request.Form["mota"];
                string Hinhanh = Request.Form["hinhanh"];
                string theloaiid = Request.Form["theloaiid"];

                if (string.IsNullOrEmpty(tenPhim) || string.IsNullOrEmpty(theloaiid))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Tên phim và thể loại không được để trống."
                    }, JsonRequestBehavior.AllowGet);
                }

                phim phim = db.phims.Where(o => o.tenphim == tenPhim).FirstOrDefault();
                if (phim != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Tên phim đã tồn tại"
                    }, JsonRequestBehavior.AllowGet);
                }

                phim newObj = new phim();
                //de xu ly hinh anh
                if (hinhanh != null)
                {
                    var filename = Path.GetFileName(hinhanh.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/phim/"), filename);
                    hinhanh.SaveAs(path);
                    newObj.hinhanh = "/Images/Phim/" + filename;
                }              

                //them moi vao database
                newObj.tenphim = tenPhim;
                if (DateTime.TryParse(thoigian, out DateTime parsedThoigian))
                {
                    newObj.thoigian = parsedThoigian;
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Thời gian không hợp lệ."
                    }, JsonRequestBehavior.AllowGet);
                }
                newObj.giave = giave;
                newObj.mota = mota;
                newObj.theloaiid = int.Parse(theloaiid);

                db.phims.InsertOnSubmit(newObj);
                db.SubmitChanges();

                return Json(new
                {
                    success = true,
                    message = "Thêm mới phim thành công"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Edit(int id)
        {
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            var phim = db.phims.FirstOrDefault(u => u.id == id);
            ViewBag.TheLoai = db.theloais.ToList();
            return View(phim);
        }

        [HttpPost]
        public JsonResult PostEdit(phim phim, HttpPostedFileBase hinhanh, HttpPostedFileBase filesp)
        {
            try
            {
                // K.tra dữ liệu đầu vào
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ"
                    });
                }

                // Tìm sản phẩm theo ID
                var exisPhim = db.phims.FirstOrDefault(e => e.id == phim.id);
                if (exisPhim == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Phim không tồn tại"
                    });
                }

                //cap nhat thong tin phim
                exisPhim.tenphim = phim.tenphim;
                exisPhim.mota = phim.mota;
                exisPhim.giave = phim.giave;
                exisPhim.theloaiid = phim.theloaiid;

                // Cap nhat hinh anh neu tai len
                if (hinhanh != null && hinhanh.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(hinhanh.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/Phim"), fileName);
                    hinhanh.SaveAs(path);
                    exisPhim.hinhanh = "/Images/Phim/" + fileName;
                }

                db.SubmitChanges();
                return Json(new
                {
                    success = true,
                    message = "Cập nhật thông tin Phim thành công",
                    redirectUrl = Url.Action("Index", "Phim") 
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "có lỗi xảy ra . chi tiết lỗi: " + ex.Message
                });
            }
        }

        public ActionResult Xoa(int id)
        {
            try
            {
                var phim = db.phims.FirstOrDefault(s => s.id == id);
                if (phim == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Phim không tồn tại"
                    }, JsonRequestBehavior.AllowGet);
                }
                db.phims.DeleteOnSubmit(phim);
                db.SubmitChanges();
                return Json(new
                {
                    success = true,
                    message = "Xóa phim thành công"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xóa phim. Chi tiết lỗi: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}