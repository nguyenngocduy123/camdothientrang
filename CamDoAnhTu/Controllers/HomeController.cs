﻿using CamDoAnhTu.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CamDoAnhTu.Controllers
{
    public class HomeController : Controller
    {
        private static int update = 0;

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        public ActionResult Management(int type)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ViewBag.type = type;
                List<Customer> list = new List<Customer>();

                if (type == -1)
                {
                    list = ctx.Customers.Where(p => (p.type < 18)).ToList();
                    return PartialView(list);
                }
                else if (type > -1 && type < 18)
                {
                    list = ctx.Customers.Where(p => p.type == type).ToList();

                    switch (type)
                    {
                        case 1:
                            var list1 = ctx.Customers.Where(p => p.type == type).ToList();

                            break;
                        default:
                            break;
                    }
                }

                return PartialView(list);
            }
        }

        #region Index

        

        public ActionResult XE2()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                List<Customer> list = ctx.Customers.Where(p => p.type == 12).ToList();

                return PartialView(list);
            }
        }

        #endregion Index

        #region LoadCustomer

        public ActionResult LoadCustomer(int? pageSize, int? type ,int page = 1)
        {
            HttpCookie reqCookies = Request.Cookies["userInfo"];
            if (reqCookies == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int pageSz = pageSize ?? 10;

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                var query1 = ctx.Customers.Where(p => p.type == type).ToList();
                int count1 = query1.Count();
                int nPages = count1 / pageSz + (count1 % pageSz > 0 ? 1 : 0);

                List<Customer> list2 = ctx.Customers.Where(p => p.type == type && p.CodeSort == null).ToList();

                foreach (Customer model in list2)
                {
                    if ((model.Code[0] >= 'A' && model.Code[0] <= 'Z') || (model.Code[0] >= 'a' && model.Code[0] <= 'z') && model.CodeSort == null)
                    {
                        int code = Int32.Parse(model.Code.Substring(1, model.Code.Length - 1));

                        if (model.Code[0] == 'A')
                        {
                            model.CodeSort = code + 1000;
                        }
                        else
                        {
                            model.CodeSort = (((model.Code[0] - 'A') + 1) * 1000) + code;
                        }
                    }
                    else
                    {
                        model.CodeSort = Int32.Parse(model.Code);
                    }
                }

                List<Customer> list = query1.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;
                ViewBag.type = type.Value;

                foreach (Customer cs in list)
                {
                    cs.NgayNo = 0;
                    int count = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count++;
                            countMax = count;
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                return View(list);
            }
        }

        public ActionResult LoadCustomerXE2(int? pageSize, int page = 1)
        {
            HttpCookie reqCookies = Request.Cookies["userInfo"];
            if (reqCookies == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int pageSz = pageSize ?? 10;
            StringBuilder str = new StringBuilder();
            decimal? k = 0;

            int todayYear = DateTime.Now.Year;
            int todayMonth = DateTime.Now.Month;
            int todayDay = DateTime.Now.Day;

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                var query1 = ctx.Customers.Where(p => p.type == 12).ToList();
                int count1 = query1.Count();
                int nPages = count1 / pageSz + (count1 % pageSz > 0 ? 1 : 0);

                List<Customer> list1 = ctx.Customers.Where(p => p.type == 12).ToList();
                List<Customer> list2 = ctx.Customers.Where(p => p.type == 12 && p.CodeSort == null).ToList();

                foreach (Customer model in list2)
                {
                    if ((model.Code[0] >= 'A' && model.Code[0] <= 'Z') || (model.Code[0] >= 'a' && model.Code[0] <= 'z') && model.CodeSort == null)
                    {
                        int code = Int32.Parse(model.Code.Substring(2, model.Code.Length - 2));

                        if (model.Code[0] == 'A')
                        {
                            model.CodeSort = code + 1000;
                        }
                        else
                        {
                            model.CodeSort = (((model.Code[0] - 'A') + 1) * 1000) + code;
                        }
                    }
                    else
                    {
                        model.CodeSort = Int32.Parse(model.Code);
                    }
                }

                List<Customer> list = query1.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;

                var summoney = (from l in ctx.Loans
                                join cs in ctx.Customers on l.IDCus equals cs.Code
                                where cs.type == 13 && l.Date.Year == todayYear && l.Date.Month == todayMonth && l.Date.Day == todayDay
                                select new
                                {
                                    cs.Price
                                }).ToList();

                foreach (var x in summoney)
                {
                    k += x.Price;
                }

                foreach (Customer cs in list)
                {
                    cs.NgayNo = 0;
                    int count = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count++;
                            countMax = count;
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                str.Append("Số tiền phải thu trong ngày " + DateTime.Now.Date.ToShortDateString() + " : " + k.ToString());
                ViewBag.Message = str.ToString();

                return View(list);
            }
        }

        #endregion LoadCustomer

        #region Search

        public ActionResult Search(string Code, string Name, int type,string Phone, string Address, int? Noxau, int? hetno,
            int page = 1)
        {
            int pageSz = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);
            List<Loan> lstLoan = new List<Loan>();

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                var list = ctx.Customers.Where(p => p.type == type).ToList();
                List<Customer> lsttrave = new List<Customer>();

                if (!string.IsNullOrEmpty(Code))
                {
                    lsttrave = list.Where(p => p.Code == Code).ToList();
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    lsttrave = list.Where(p => p.Name.Contains(Name)).ToList();
                }
                if (!string.IsNullOrEmpty(Phone))
                {
                    lsttrave = list.Where(p => p.Phone.Contains(Phone)).ToList();
                }
                if (!string.IsNullOrEmpty(Address))
                {
                    lsttrave = list.Where(p => p.Address.Contains(Address)).ToList();
                }
                if (Noxau == 1)
                {
                    List<Customer> lstCus = ctx.Customers.Where(p => p.type == 1).ToList();
                    foreach (Customer p in lstCus)
                    {
                        if (p.NgayNo >= 3 + Int32.Parse(p.DayBonus.ToString()))
                        {
                            lsttrave.Add(p);
                        }
                    }
                }

                if (hetno == 1)
                {
                    List<Customer> lstCus = ctx.Customers.Where(p => p.type == 1).ToList();
                    foreach (Customer p in lstCus)
                    {
                        int day = 0;
                        if (Int32.Parse(p.Price.ToString()) == 0)
                        {
                            day = 0;
                        }
                        else
                            day = Int32.Parse(p.Loan.ToString()) / Int32.Parse(p.Price.ToString());

                        if (p.DayPaids == day || p.Description == "End")
                        {
                            lsttrave.Add(p);
                        }
                    }
                }

                int count = lsttrave.Count();
                int nPages = count / pageSz + (count % pageSz > 0 ? 1 : 0);
                List<Customer> lsttrave1 = lsttrave.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;
                ViewBag.Code = Code;
                ViewBag.Name = Name;
                ViewBag.Phone = Phone;
                ViewBag.Noxau = Noxau;
                ViewBag.hetno = hetno;
                ViewBag.Address = Address;
                ViewBag.type = type;

                foreach (Customer cs in lsttrave1)
                {
                    cs.NgayNo = 0;
                    int count1 = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count1++;
                            countMax = count1;
                        }
                        else
                        {
                            count1 = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                return View(lsttrave1);
            }
        }

        public ActionResult SearchXE2(string Code, string Name, string Phone, string Address, int? Noxau, int? hetno, int page = 1)
        {
            int pageSz = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);
            List<Loan> lstLoan = new List<Loan>();

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                var list = ctx.Customers.Where(p => p.type == 12).ToList();
                List<Customer> lsttrave = new List<Customer>();

                if (!string.IsNullOrEmpty(Code))
                {
                    lsttrave = list.Where(p => p.Code == Code).ToList();
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    lsttrave = list.Where(p => p.Name.Contains(Name)).ToList();
                }
                if (!string.IsNullOrEmpty(Phone))
                {
                    lsttrave = list.Where(p => p.Phone.Contains(Phone)).ToList();
                }
                if (Noxau == 1)
                {
                    List<Customer> lstCus = ctx.Customers.Where(p => p.type == 12).ToList();
                    foreach (Customer p in lstCus)
                    {
                        if (p.NgayNo >= 3 + Int32.Parse(p.DayBonus.ToString()))
                        {
                            lsttrave.Add(p);
                        }
                    }
                }

                if (hetno == 1)
                {
                    List<Customer> lstCus = ctx.Customers.Where(p => p.type == 13).ToList();
                    foreach (Customer p in lstCus)
                    {
                        int day = 0;
                        if (Int32.Parse(p.Price.ToString()) == 0)
                        {
                            day = 0;
                        }
                        else
                            day = Int32.Parse(p.Loan.ToString()) / Int32.Parse(p.Price.ToString());

                        if (p.DayPaids == day || p.Description == "End")
                        {
                            lsttrave.Add(p);
                        }
                    }
                }

                int count = lsttrave.Count();
                int nPages = count / pageSz + (count % pageSz > 0 ? 1 : 0);
                List<Customer> lsttrave1 = lsttrave.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;
                ViewBag.Code = Code;
                ViewBag.Name = Name;
                ViewBag.Phone = Phone;
                ViewBag.Noxau = Noxau;
                ViewBag.hetno = hetno;
                ViewBag.Address = Address;

                foreach (Customer cs in lsttrave1)
                {
                    cs.NgayNo = 0;
                    int count1 = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count1++;
                            countMax = count1;
                        }
                        else
                        {
                            count1 = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                return View(lsttrave1);
            }
        }


        #endregion Search

        public ActionResult Refresh(int type)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                var query1 = ctx.Customers.ToList();

                foreach (Customer cs in query1)
                {
                    cs.NgayNo = 0;
                    int count = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count++;
                            countMax = count;
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }
            }
            return RedirectToAction("LoadCustomer", "Home", new { type = type });
        }

        #region AddCustomer

        public ActionResult AddCustomer(int type)
        {
            Customer model = new Customer();
            model.type = type;

            return View(model);
        }

        [HttpPost]
        public ActionResult AddCustomer(Customer model, HttpPostedFileBase fuMain)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                if (model.DayBonus == null)
                {
                    model.DayBonus = 0;
                }
                model.DayPaids = 0;
                model.AmountPaid = 0;
                model.RemainingAmount = 0;
                model.type = 1;

                if ((model.Code[0] >= 'A' && model.Code[0] <= 'Z') || (model.Code[0] >= 'a' && model.Code[0] <= 'z') && model.CodeSort == null)
                {
                    int code = Int32.Parse(model.Code.Substring(1, model.Code.Length - 1));

                    if (model.Code[0] == 'A')
                    {
                        model.CodeSort = code + 1000;
                    }
                    else
                    {
                        model.CodeSort = (((model.Code[0] - 'A') + 1) * 1000) + code;
                    }
                }
                else
                {
                    model.CodeSort = Int32.Parse(model.Code);
                }

                ctx.Customers.Add(model);
                ctx.SaveChanges();
                int day = Int32.Parse(model.Loan.ToString()) / Int32.Parse(model.Price.ToString());

                for (int i = 1; i <= day; i++)
                {
                    Loan temp = new Loan();
                    temp.Date = model.StartDate.AddDays(i);
                    temp.IDCus = model.Code;
                    temp.Status = 0;
                    ctx.Loans.Add(temp);
                    ctx.SaveChanges();
                }
            }

            if (fuMain != null && fuMain.ContentLength > 0)
            {
                string spDirPath = Server.MapPath("~/image");
                string targetDirPath = Path.Combine(spDirPath, model.Code.ToString());
                Directory.CreateDirectory(targetDirPath);

                string mainFileName = Path.Combine(targetDirPath, "main.jpg");
                fuMain.SaveAs(mainFileName);
            }
            return RedirectToAction("LoadCustomer", "Home", new { type = model.type });
        }

        #endregion AddCustomer

        #region UpdateCustomer

        public ActionResult Update(string id)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                update = 0;
                var pro = ctx.Customers.Where(p => p.Code == id).ToList().FirstOrDefault();

                if (pro.Loan == null || pro.Price == null)
                {
                    update = 1;
                }

                if (pro == null)
                {
                    return RedirectToAction("LoadCustomer", "Home");
                }

                return View(pro);
            }
        }

        [HttpPost]
        public ActionResult Update(Customer model, HttpPostedFileBase fuMain)
        {
            int i = 1;

            if (model.DayBonus == null)
            {
                model.DayBonus = 0;
            }

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                Customer pro = ctx.Customers.Where(p => p.Code == model.Code).FirstOrDefault();
                pro.Name = model.Name;
                pro.Phone = model.Phone;
                pro.Address = model.Address;
                pro.Loan = model.Loan;
                pro.Price = model.Price;
                pro.DayPaids = model.DayPaids;
                pro.AmountPaid = model.AmountPaid;
                pro.RemainingAmount = model.RemainingAmount;
                pro.DayBonus = model.DayBonus;
                pro.OldCode = model.OldCode;
                pro.Note = model.Note;
                if (pro.StartDate != model.StartDate)
                {
                    pro.StartDate = model.StartDate;
                    int t = Int32.Parse(model.Loan.ToString()) / Int32.Parse(model.Price.ToString());
                    List<Loan> l = ctx.Loans.Where(p => p.IDCus == model.Code).ToList();

                    foreach (Loan temp in l)
                    {
                        temp.Date = model.StartDate.AddDays(i);
                        temp.IDCus = model.Code;
                        temp.Status = 0;
                        i++;
                        ctx.SaveChanges();
                    }
                }

                ctx.SaveChanges();

                if (update == 1)
                {
                    pro = ctx.Customers.Where(p => p.Code == pro.Code).FirstOrDefault();

                    int day = Int32.Parse(pro.Loan.ToString()) / Int32.Parse(pro.Price.ToString());

                    for (int s = 1; s <= day; s++)
                    {
                        Loan temp = new Loan();
                        temp.Date = pro.StartDate.AddDays(s);
                        temp.IDCus = pro.Code;
                        temp.Status = 0;
                        ctx.Loans.Add(temp);
                        ctx.SaveChanges();
                    }
                    ctx.SaveChanges();
                }
            }

            if (fuMain != null && fuMain.ContentLength > 0)
            {
                string spDirPath = Server.MapPath("~/image");
                string targetDirPath = Path.Combine(spDirPath, model.Code.ToString());
                Directory.CreateDirectory(targetDirPath);

                string mainFileName = Path.Combine(targetDirPath, "main.jpg");
                fuMain.SaveAs(mainFileName);
            }

            return RedirectToAction("LoadCustomer", "Home", new { type = model.type });
        }

        #endregion UpdateCustomer

        public ActionResult Detail(string id)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                Customer model = ctx.Customers.FirstOrDefault(p => p.Code == id);
                List<Loan> list = ctx.Loans.Where(p => p.IDCus == id).ToList();
                ViewData["Loan"] = list;

                return View(model);
            }
        }

        public ActionResult Addday(Loan model)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                ctx.Loans.Add(model);
                ctx.SaveChanges();
            }

            return RedirectToAction("LoadCustomer", "Home");
        }

        public static string timetemp;

        [HttpGet]
        public ActionResult UpdateLoan(int loanid, string songaydong, string idcus)
        {
            decimal? ct = 0;
            int t = -1;
            int songay;
            decimal? amount = 0;
            decimal? remainingamount = 0;
            int? songaydatra;

            if (String.IsNullOrEmpty(songaydong))
            {
                songay = 0;
            }
            else
            {
                songay = Int32.Parse(songaydong);
            }

            if (songay == 0)
            {
                Loan item = new Loan();

                using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                {
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    Customer csCustomer = new Customer();

                    item = ctx.Loans.Where(p => p.ID == loanid).FirstOrDefault();
                    timetemp = item.Date.ToShortDateString();

                    item.Status = item.Status + 1;

                    if (item.Status >= 2)
                    {
                        item.Status = 0;
                    }

                    if (item.Status == 1)
                    {
                        csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                        List<Loan> lstSongaydatra = ctx.Loans.Where(p => p.IDCus == idcus && p.Status == 1).ToList();
                        songaydatra = lstSongaydatra.Count;

                        songaydatra++;
                        csCustomer.AmountPaid = songaydatra * csCustomer.Price;
                        csCustomer.RemainingAmount = csCustomer.Loan - csCustomer.AmountPaid;
                        t = 1;
                        csCustomer.DayPaids = songaydatra;

                        WriteHistory(csCustomer, 0, loanid);
                    }
                    else
                    {
                        csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                        List<Loan> lstSongaydatra = ctx.Loans.Where(p => p.IDCus == csCustomer.Code && p.Status == 1).ToList();
                        songaydatra = lstSongaydatra.Count;
                        songaydatra--;
                        csCustomer.AmountPaid = songaydatra * csCustomer.Price;
                        csCustomer.RemainingAmount = csCustomer.Loan - csCustomer.AmountPaid;
                        t = 0;
                        csCustomer.DayPaids = songaydatra;

                        WriteHistory(csCustomer, 0, loanid);
                    }

                    ct = csCustomer.Price;
                    amount = csCustomer.AmountPaid ?? 0;
                    remainingamount = csCustomer.RemainingAmount ?? 0;

                    ctx.SaveChanges();
                }
            }
            else
            {
                for (int i = 0; i < songay; i++)
                {
                    Loan item = new Loan();

                    using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                    {
                        ctx.Configuration.ValidateOnSaveEnabled = false;
                        Customer csCustomer = new Customer();
                        item = ctx.Loans.Where(p => p.ID == loanid && p.IDCus == idcus).FirstOrDefault();

                        loanid++;
                        item.Status = item.Status + 1;

                        if (item.Status >= 2)
                        {
                            item.Status = 0;
                        }

                        if (item.Status == 1)
                        {
                            csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();

                            List<Loan> lstSongaydatra = ctx.Loans.Where(p => p.IDCus == csCustomer.Code && p.Status == 1).ToList();
                            songaydatra = lstSongaydatra.Count;
                            songaydatra++;
                            csCustomer.AmountPaid = songaydatra * csCustomer.Price;
                            csCustomer.RemainingAmount = csCustomer.Loan - csCustomer.AmountPaid;
                            t = 1;
                            csCustomer.DayPaids = songaydatra;

                            WriteHistory(csCustomer, 0, loanid);
                        }
                        else
                        {
                            csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                            List<Loan> lstSongaydatra = ctx.Loans.Where(p => p.IDCus == csCustomer.Code && p.Status == 1).ToList();
                            songaydatra = lstSongaydatra.Count;
                            songaydatra--;
                            csCustomer.AmountPaid = csCustomer.DayPaids * csCustomer.Price;
                            csCustomer.RemainingAmount = csCustomer.Loan - csCustomer.AmountPaid;
                            t = 0;
                            csCustomer.DayPaids = songaydatra;

                            WriteHistory(csCustomer, 0, loanid);
                        }
                        ct = csCustomer.Price;
                        amount = csCustomer.AmountPaid ?? 0;
                        remainingamount = csCustomer.RemainingAmount ?? 0;

                        ctx.SaveChanges();
                    }
                }
            }

            return Json(new { success = true, oldval = loanid, status = t, songay = songay, amount = amount, remainingamount = remainingamount, ct = ct },
                JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult UpdateNodung(int loanid, string songaydong, string idcus)
        {
            int t = -1;
            int songay;

            if (String.IsNullOrEmpty(songaydong))
                songay = 0;
            else
                songay = Int32.Parse(songaydong);

            if (songay == 0)
            {
                Loan item = new Loan();

                using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                {
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    Customer csCustomer = new Customer();

                    item = ctx.Loans.Where(p => p.ID == loanid && p.IDCus == idcus).FirstOrDefault();
                    timetemp = item.Date.ToShortDateString();

                    item.Status = item.Status + 1;

                    if (item.Status >= 2)
                    {
                        item.Status = 0;
                    }

                    if (item.Status == 1)
                    {
                        csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                        t = 1;
                        WriteHistory(csCustomer, 0, loanid);
                    }
                    else
                    {
                        csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                        t = 0;
                        WriteHistory(csCustomer, 0, loanid);
                    }

                    ctx.SaveChanges();
                }
            }
            else
            {
                for (int i = 0; i < songay; i++)
                {
                    Loan item = new Loan();


                    using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                    {
                        ctx.Configuration.ValidateOnSaveEnabled = false;
                        Customer csCustomer = new Customer();
                        item = ctx.Loans.Where(p => p.ID == loanid && p.IDCus == idcus).FirstOrDefault();
                        timetemp = item.Date.ToShortDateString();
                        item.Status = item.Status + 1;

                        if (item.Status >= 2)
                        {
                            item.Status = 0;
                        }

                        if (item.Status == 1)
                        {
                            csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                            t = 1;
                            WriteHistory(csCustomer, 0, loanid);
                        }
                        else
                        {
                            csCustomer = ctx.Customers.Where(p => p.Code == item.IDCus).FirstOrDefault();
                            t = 0;
                            WriteHistory(csCustomer, 0, loanid);
                        }

                        loanid++;
                        ctx.SaveChanges();
                    }
                }
            }

            return Json(new { success = true, oldval = loanid, status = t, songay = songay },
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Reset(string id)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                Customer cs = ctx.Customers.Where(p => p.Code == id).FirstOrDefault();

                cs.Description = "End";
                ctx.SaveChanges();
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public int GetUserByUsername(string uname)
        {
            Customer user = null;
            int a = 0;
            string code = uname;
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                user = ctx.Customers.Where(u => u.Code == code).FirstOrDefault();
            }
            if (user == null)
            {
                a = 1;
            }
            return a;
        }

        #region Tim kiem chan le

        public ActionResult TimKiemNoKhachHang()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 1).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHang1()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 1).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenAT()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 2).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddAT()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 2).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenAL()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 3).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddAL()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 3).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenAM()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 4).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddAM()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 4).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenAN()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 5).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddAN()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 5).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenAI()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 6).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddAI()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 6).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenAH()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 7).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddAH()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 7).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenAX()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 8).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddAX()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 8).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenAD()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 9).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddAD()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 9).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenAB()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 10).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddAB()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 10).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenAC()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 11).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddAC()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 11).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenXE1()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 13).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddXE1()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 13).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangEvenXE2()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 12).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        public ActionResult TimKiemNoKhachHangOddXE2()
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                int result;
                List<Customer> lst = ctx.Customers.Where(p => p.type == 12).ToList();
                List<Customer> lst1 = new List<Customer>();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1 && CheckHetNo(cus) == false)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }
                return View(lst1);
            }
        }

        #endregion Tim kiem chan le

        public ActionResult LoadCustomerEven(int page = 1)
        {
            int pageSz = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;

                List<Customer> lst = ctx.Customers.ToList();
                List<Customer> lst1 = new List<Customer>();

                int result;
                //lst1 = lst.Where(p => (p.Code[p.Code.Length - 1] % 2 == 0) && CheckHetNo(p) == false).ToList();
                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 0)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }

                int count1 = lst1.Count();
                int nPages = count1 / pageSz + (count1 % pageSz > 0 ? 1 : 0);

                lst1 = lst1.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;

                foreach (Customer cs in lst1)
                {
                    cs.NgayNo = 0;
                    int count = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count++;
                            countMax = count;
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                return View(lst1);
            }
        }

        public ActionResult LoadCustomerOdd(int page = 1)
        {
            int pageSz = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;

                List<Customer> lst = ctx.Customers.ToList();
                List<Customer> lst1 = new List<Customer>();

                int result;

                foreach (var cus in lst)
                {
                    string t = cus.Code[cus.Code.Length - 1].ToString();
                    if (int.TryParse(t, out result))
                    {
                        int id = Int32.Parse(t);
                        if (id % 2 == 1)
                        {
                            lst1.Add(cus);
                        }
                    }
                    else
                    {
                        // Not a number, do something else with it.
                    }
                }

                int count1 = lst1.Count();
                int nPages = count1 / pageSz + (count1 % pageSz > 0 ? 1 : 0);

                lst1 = lst1.OrderBy(p => p.CodeSort)
                    .Skip((page - 1) * pageSz)
                     .Take(pageSz).ToList();

                ViewBag.PageCount = nPages;
                ViewBag.CurPage = page;

                foreach (Customer cs in lst1)
                {
                    cs.NgayNo = 0;
                    int count = 0;
                    int countMax = 0;

                    DateTime EndDate = DateTime.Now;

                    List<Loan> t = ctx.Loans.Where(p => p.IDCus == cs.Code).OrderBy(p => p.Date).ToList();

                    Loan t1 = new Loan();

                    if (t.Count != 0)
                    {
                        t1 = t.First();
                    }

                    DateTime StartDate = t1.Date;

                    List<Loan> query = t.Where(p => p.Date >= StartDate && p.Date <= EndDate).ToList();

                    foreach (Loan temp in query)
                    {
                        if (temp.Status == 0)
                        {
                            count++;
                            countMax = count;
                        }
                        else
                        {
                            count = 0;
                        }
                    }

                    cs.NgayNo = countMax;
                    ctx.SaveChanges();
                }

                return View(lst1);
            }
        }

        #region Global

        private List<Customer> GetCustomer(string searchString)
        {
            List<Customer> lst = new List<Customer>();

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                lst = ctx.Customers.Where(p => p.Name.StartsWith(searchString)).ToList();
            }
            return lst;
        }

        public ActionResult SearchName(String term)
        {
            var products = GetCustomer(term).Select(c => new { id = c.Code, value = c.Name });
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        private List<Customer> GetCustomer1(string searchString)
        {
            List<Customer> lst = new List<Customer>();

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                lst = ctx.Customers.Where(p => p.Code.StartsWith(searchString)).ToList();
            }
            return lst;
        }

        public ActionResult SearchCode(String term)
        {
            var products = GetCustomer1(term).Select(c => new { id = c.Code, value = c.Code });
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        private List<Customer> GetCustomer2(string searchString)
        {
            List<Customer> lst = new List<Customer>();

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                lst = ctx.Customers.Where(p => p.Phone.StartsWith(searchString)).ToList();
            }
            return lst;
        }

        public ActionResult SearchPhone(String term)
        {
            var products = GetCustomer2(term).Select(c => new { id = c.Code, value = c.Phone });
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        private List<Customer> GetCustomer3(string searchString)
        {
            List<Customer> lst = new List<Customer>();

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                lst = ctx.Customers.Where(p => p.Address.StartsWith(searchString)).ToList();
            }
            return lst;
        }

        public ActionResult SearchAddress(String term)
        {
            var products = GetCustomer3(term).Select(c => new { id = c.Code, value = c.Address });
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveItem(string proId)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ctx.Configuration.ValidateOnSaveEnabled = false;
                Customer ep = ctx.Customers.Where(p => p.Code == proId).FirstOrDefault();
                List<Loan> lstLoans = ctx.Loans.Where(p => p.IDCus == proId).ToList();

                ctx.Customers.Remove(ep);

                foreach (var item in lstLoans)
                {
                    ctx.Loans.Remove(item);
                }
                ctx.SaveChanges();
            }
            ViewBag.Delete = true;
            return RedirectToAction("LoadCustomer", "Home");
        }

        [HttpPost]
        public JsonResult DeleteCustomer(string id)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
                {
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    var cus = ctx.Customers.Where(o => o.Code == id).FirstOrDefault();
                    List<Loan> lstLoans = ctx.Loans.Where(p => p.IDCus == id).ToList();
                    if (cus != null)
                    {
                        ctx.Customers.Remove(cus);

                        foreach (Loan item in lstLoans)
                        {
                            ctx.Loans.Remove(item);
                        }

                        ctx.SaveChanges();
                        result["status"] = "success";
                    }
                }
            }
            catch (Exception ex)
            {
                result["status"] = "error";
                result["message"] = ex.Message;
            }

            return Json(result);
        }

        public bool CheckHetNo(Customer cs)
        {
            bool kq = false;
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                List<Customer> lstCus = ctx.Customers.ToList();

                int day = 0;
                if (cs.Price == null || Int32.Parse(cs.Price.ToString()) == 0)
                {
                    day = 0;
                }
                else
                    day = Int32.Parse(cs.Loan.ToString()) / Int32.Parse(cs.Price.ToString());

                if (cs.DayPaids == day || cs.Description == "End")
                {
                    kq = true;
                }
            }
            return kq;
        }

        public void WriteHistory(Customer p, int money, int loanid)
        {
            StringBuilder str = new StringBuilder();
            int type = 0;

            

            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {

                if (loanid != -1)
                {
                    var checkhs = ctx.histories.Where(s => s.loanid == loanid).FirstOrDefault();
                    if (checkhs == null)
                    {
                        var checkLoan = ctx.Loans.Where(s => s.ID == loanid).FirstOrDefault();

                        if (checkLoan.Status == 1)
                        {
                            type = 0;
                            history hs = new history();
                            str.Append("Xóa đóng tiền cho ngày: " + timetemp);
                            hs.CustomerId = p.ID;
                            hs.CustomerCode = p.Code;
                            hs.Detail = str.ToString();
                            hs.Ngaydongtien = DateTime.Now ;
                            hs.price = money == 0 ? p.Price : money;
                            hs.status = type;
                            hs.loanid = loanid;
                            ctx.histories.Add(hs);
                        }
                        else
                        {
                            type = 1;
                            history hs = new history();
                            str.Append("Đóng tiền cho ngày: " + timetemp);
                            hs.CustomerId = p.ID;
                            hs.CustomerCode = p.Code;
                            hs.Detail = str.ToString();
                            hs.Ngaydongtien = DateTime.Now;
                            hs.price = money == 0 ? p.Price : money;
                            hs.status = type;
                            hs.loanid = loanid;
                            ctx.histories.Add(hs);
                        }


                    }
                    else
                    {

                        int oldtype = checkhs.status.Value;

                        if (oldtype == 1) // xóa dong tien
                        {
                            str.Append("Xóa đóng tiền cho ngày: " + timetemp);
                            checkhs.Ngaydongtien = DateTime.Now;
                            type = 0;
                            checkhs.status = type;
                            checkhs.Detail = str.ToString();
                        }
                        else // đóng tien
                        {
                            str.Append("Đóng tiền cho ngày: " + timetemp);
                            type = 1;
                            checkhs.Ngaydongtien = DateTime.Now;
                            checkhs.status = type;
                            checkhs.Detail = str.ToString();
                        }

                    }
                }
                else if (loanid == -1)
                {
                    history hs = new history();
                    str.Append("Kết thúc dây nợ ngày : " + timetemp);
                    hs.CustomerId = p.ID;
                    hs.Detail = str.ToString();
                    hs.CustomerCode = p.Code;
                    hs.Ngaydongtien = DateTime.Now;
                    hs.price = money == 0 ? p.Price : money;
                    hs.status = 0;
                    hs.loanid = -1;
                    ctx.histories.Add(hs);
                }

                ctx.SaveChanges();
            }
        }

        #endregion Global
    }
}