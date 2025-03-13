using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyCinema.Models;

namespace QuanLyCinema.Controllers
{
    public class FoodController : Controller
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
        public ActionResult Create()
        {
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            return View();
        }
        public JsonResult PostCreate(HttpPostedFileBase hinhanh)
        {
            try
            {

                string ten = Request.Form["ten"];
                decimal gia = Convert.ToDecimal(Request.Form["gia"]);
                string Hinhanh = Request.Form["hinhanh"];
                doan doan = db.doans.Where(o => o.ten == ten).FirstOrDefault();
                if (doan != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Tên đồ ăn đã tồn tại "
                    }, JsonRequestBehavior.AllowGet);
                }
                doan newObj = new doan();
                if (hinhanh != null)
                {
                    var filename = Path.GetFileName(hinhanh.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/Food/"), filename);
                    hinhanh.SaveAs(path);
                    newObj.hinhanh = "/Images/Food/" + filename;
                }
               
                newObj.ten = ten;
                newObj.gia = gia;

                db.doans.InsertOnSubmit(newObj);
                db.SubmitChanges();

                return Json(new
                {
                    success = true,
                    message = "Thêm mới món ăn thành công"
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

        public JsonResult GetListdoan()
        {
            try
            {
                var doan = db.doans.Select(d => new {
                    id = d.id,
                    ten = d.ten,
                    gia=d.gia,
                    hinhanh=d.hinhanh
                }).ToList();

                return Json(new { success = true, data = doan }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Edit(int id)
        {
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            doan doan = db.doans.FirstOrDefault(u => u.id == id);
            if (doan == null)
            {
                return HttpNotFound(); // ko tim thay mon an
            }
            return View(doan);

        }
        [HttpPost]
        public JsonResult PostEdit(doan doan, HttpPostedFileBase hinhanh)
        {
            if (ModelState.IsValid)
            {
                var exisdoan = db.doans.FirstOrDefault(e => e.id == doan.id);
                if (exisdoan == null)
                {
                    return Json(new
                    {
                        success = false,
                        Message = "Đồ ăn không tồn tại"
                    });
                }

                //cap nhat thong tin
                exisdoan.ten = doan.ten;
                exisdoan.gia = doan.gia;

                // cap nhat hinh anh neu tai len
                if (hinhanh != null && hinhanh.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(hinhanh.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/Food/"), fileName);
                    hinhanh.SaveAs(path);
                    exisdoan.hinhanh = "/Images/Food/" + fileName;
                }

                db.SubmitChanges();
                return Json(new
                {
                    success = true,
                    message = "Cập nhật thông tin đồ ăn thành công"
                });
            }
            return Json(new
            {
                success = false,
                Message = "Cập nhật thất bại"
            });
        }

        public JsonResult Paging(string searchQuery = "", int pageSize = 2, int crrPage = 1)
        {
            List<doan> doans = db.doans.ToList();
            List<doan> doanPaging = db.doans
                .Where(o => o.ten.Contains(searchQuery))
                .OrderBy(o => o.ten) // sap xep ten theo thu tu tu A-z
                .Skip((crrPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            int totalPage = 0;
            if (searchQuery == "")
            {
                totalPage = (int)Math.Ceiling((double)db.doans.Count() / pageSize);
            }
            else
            {
                totalPage = (int)Math.Ceiling((double)doans.Count() / pageSize);
            }
            var results = doanPaging.Select(doan => new
            {
                id = doan.id,
                ten = doan.ten,
                gia=doan.gia,
                hinhanh=doan.hinhanh

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

        public ActionResult Xoa(int id)
        {
            try
            {
                var doan = db.doans.FirstOrDefault(s => s.id == id);
                if (doan == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Đồ ăn không tồn tại"
                    }, JsonRequestBehavior.AllowGet);
                }
                db.doans.DeleteOnSubmit(doan);
                db.SubmitChanges();
                return Json(new
                {
                    success = true,
                    message = "Xóa đồ ăn thành công"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi khi xóa. chi tiết lỗi: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}