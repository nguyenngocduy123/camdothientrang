using CamDoAnhTu.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Web.Mvc;
using System.Web.Security;

namespace CamDoAnhTu.Helper
{
    public class Helper
    {
        public static User GetUserInfo()
        {
            User us = new User();
            //us = (User)Session["CurUser"];
            HttpCookie reqCookies = HttpContext.Current.Request.Cookies["userInfo"];
            if (reqCookies != null)
            {
                us.UserName = reqCookies["username"].ToString();
                us.PassWord = reqCookies["password"].ToString();
                us.Permission = Int32.Parse(reqCookies["permisson"].ToString());
                us.id_cuahang = Int32.Parse(reqCookies["id_cuahang"].ToString());
            }

            return us;
        }
    }
}