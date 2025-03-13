using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BC = BCrypt.Net.BCrypt;
using QuanLyCinema.Models;

namespace QuanLyCinema.Controllers
{
    public class UserController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=TUANCUONG;Initial Catalog=QLCinema;Persist Security Info=True;User ID=sa;Password=123456;TrustServerCertificate=True");
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public JsonResult PostDangKy()
        {
            try
            {
                //lay cac gia tri tu form

                string username = Request.Form["username"];
                string password = Request.Form["password"];
                string hashpassword = BC.HashPassword(password);
         
                string email = Request.Form["email"];
                string sodienthoai = Request.Form["sodienthoai"];
                //check xem username ton tai hay chua
                nguoidung user = db.nguoidungs.Where(u => u.username == username).FirstOrDefault();
                if (user != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "username đã tồn tại"
                    }, JsonRequestBehavior.AllowGet);
                }

                nguoidung newobj = new nguoidung();            
                newobj.username = username;
                newobj.password = hashpassword; //luu hashpassword
                newobj.email = email;
                newobj.sodienthoai = sodienthoai;

                db.nguoidungs.InsertOnSubmit(newobj);
                db.SubmitChanges();

                return Json(new
                {
                    success = true,
                    message = "đăng ký thành công"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        public JsonResult PostDangNhap()
        {
            //lay cac gia tri tu form
            string username = Request.Form["username"];
            string password = Request.Form["password"];
            string hashpassword = BC.HashPassword(password);
 
            nguoidung user = db.nguoidungs.FirstOrDefault(u => u.username == username);
            if (user != null && BC.Verify(password, user.password))
            {
                Session["username"] = user.username;
                Session["userId"] = user.id;
                //tra ve thong bao
                return Json(new
                {
                    success = true,
                    message = "Đăng nhập thành công",
                    redirectUrl = Url.Action("Index", "Home")
                }, JsonRequestBehavior.AllowGet);
            }
            //neu khong tim thay hoac sai mk
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Sai thông tin đăng nhập"
                }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Edit(int id)
        {
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }
            var User = db.nguoidungs.FirstOrDefault(u => u.id == id);
            return View(User);
        }

        [HttpPost]
        public JsonResult PostEdit(nguoidung user)
        {
            if (ModelState.IsValid)
            {               
                var exisUser = db.nguoidungs.FirstOrDefault(e => e.id == user.id);
                if (exisUser == null)
                {
                    return Json(new
                    {
                        success = false,
                        Message = "Người dùng không tồn tại"
                    });
                }
                string pass = Request.Form["password"];
                string repass = Request.Form["repassword"];

                //kiem tra va xac nhan mk
                if (pass != repass)
                {
                    return Json(new
                    {
                        success = false,
                        Message = "Mk và xác nhận mk không trùng khớp"
                    });
                }

                //cap nhat thong tin
                exisUser.username = user.username;
                exisUser.sodienthoai = user.sodienthoai;
                exisUser.email = user.email;

                //cap nhat mk moi neu co
                if (!string.IsNullOrEmpty(pass))
                {
                    exisUser.password = BC.HashPassword(pass);
                }
           
                Session["username"] = user.username;
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
                Message = "Cập nhật thông tin thất bại"
            });
        }

      
        [HttpPost]
        public JsonResult GetNguoiDungByUsername()
        {
            try
            {
                //reques cac gia tri tu form
                string username = Request.Form["username"];
                nguoidung user = db.nguoidungs.Where(u => u.username == username).FirstOrDefault();
                return Json(new
                {
                    data = user,
                    success = true,
                    message = "Lấy dữ liệu thành công"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    Mesage = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetUserInfo()
        {
            if (Session["username"] == null)
            {
                return Redirect("/User/DangNhap");
            }

            string username = Session["username"].ToString();
            var user = db.nguoidungs.FirstOrDefault(u => u.username == username);

            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                username = user.username,
                email = user.email,
                sodienthoai = user.sodienthoai,
                id = user.id
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("DangNhap", "User");
        }

    }
}