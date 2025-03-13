using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyCinema.Models;

namespace QuanLyCinema.Controllers
{
    public class TheLoaiPhimController : Controller
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
            ViewBag.TheLoai = db.theloais.ToList();
            return View();
        }
        public JsonResult PostCreate()
        {

            try
            {
                
                string ten = Request.Form["ten"];           
                theloai theLoai = db.theloais.Where(o => o.ten == ten).FirstOrDefault();
                if (theLoai != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Tên thể loại đã tồn tại "
                    }, JsonRequestBehavior.AllowGet);
                }

                theloai newObj = new theloai();
                newObj.ten = ten;

                db.theloais.InsertOnSubmit(newObj);
                db.SubmitChanges();

                return Json(new
                {
                    success = true,
                    message = "Thêm mới thể loại thành công"
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

        public JsonResult GetListTheLoai()
        {
            try
            {
                var theLoai = db.theloais.Select(d => new {
                    id = d.id,
                    ten = d.ten
                }).ToList();

                return Json(new { success = true, data = theLoai }, JsonRequestBehavior.AllowGet);
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
            theloai theLoai = db.theloais.FirstOrDefault(u => u.id == id);
            if (theLoai == null)
            {
                return HttpNotFound(); // ko tim thay the loai
            }
            return View(theLoai);

        }
        [HttpPost]
        public JsonResult PostEdit(theloai theLoai)
        {
            if (ModelState.IsValid)
            {
                var exisTheLoai = db.theloais.FirstOrDefault(e => e.id == theLoai.id);
                if (exisTheLoai == null)
                {
                    return Json(new
                    {
                        success = false,
                        Message = "Thể loại không tồn tại"
                    });
                }

                //cap nhat thong tin
                exisTheLoai.ten = theLoai.ten;

                db.SubmitChanges();
                return Json(new
                {
                    success = true,
                    Message = "Cập nhật thông tin thành công"
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
            List<theloai> theloais = db.theloais.ToList();
            List<theloai> theloaiPaging = db.theloais
                .Where(o => o.ten.Contains(searchQuery) )
                .OrderBy(o => o.ten) // sap xep ten theo thu tu tu A-z
                .Skip((crrPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            int totalPage = 0;
            if (searchQuery == "")
            {
                totalPage = (int)Math.Ceiling((double)db.theloais.Count() / pageSize);
            }
            else
            {
                totalPage = (int)Math.Ceiling((double)theloais.Count() / pageSize);
            }
            var results = theloaiPaging.Select(theloai => new
            {
                id = theloai.id,
                tentheloai = theloai.ten
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

    }
}