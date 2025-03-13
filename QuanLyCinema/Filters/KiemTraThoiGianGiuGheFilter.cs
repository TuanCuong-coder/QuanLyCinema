using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyCinema.Models;

namespace QuanLyCinema.Filters
{
    public class KiemTraThoiGianGiuGheFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            DataClasses1DataContext db= new DataClasses1DataContext("Data Source=TUANCUONG;Initial Catalog=QLCinema;Persist Security Info=True;User ID=sa;Password=123456;TrustServerCertificate=True");
            var now = DateTime.Now;
            var gheList = db.ghes.Where(g => g.trangthai == 1).ToList();

            foreach (var ghe in gheList)
            {
                if (ghe.thoigianGiu.HasValue && (now - ghe.thoigianGiu.Value).TotalMinutes > 1)
                {
                    ghe.trangthai = 0; 
                    ghe.thoigianGiu = null; 
                    ghe.hoadonid = null;       
                }
            }
            db.SubmitChanges();

            base.OnActionExecuting(filterContext);
        }
    }
}