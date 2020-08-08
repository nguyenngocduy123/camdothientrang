using CamDoAnhTu.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CamDoAnhTu.Controllers
{
    public class XE1Controller : Controller
    {
        private static int update = 0;
        // GET: XE1
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult XE1(int type)
        {
            using (CamdoAnhTuEntities1 ctx = new CamdoAnhTuEntities1())
            {
                ViewBag.type = type;
                List<Customer> list = ctx.Customers.Where(p => p.type == type).ToList();

                return PartialView(list);
            }
        }

        public ActionResult LoadCustomerXE1(int? pageSize, int? type, int page = 1)
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
                var query1 = ctx.Customers.Where(p => p.type == type).ToList();
                int count1 = query1.Count();
                int nPages = count1 / pageSz + (count1 % pageSz > 0 ? 1 : 0);

                List<Customer> list1 = ctx.Customers.Where(p => p.type == 13).ToList();
                List<Customer> list2 = ctx.Customers.Where(p => p.type == 13 && p.CodeSort == null).ToList();

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
                ViewBag.type = type.Value;

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


        public ActionResult AddCustomerXE1(int type)
        {
            Customer cs = new Customer();
            cs.type = type;
            return View(cs);
        }

        [HttpPost]
        public ActionResult AddCustomerXE1(Customer model, HttpPostedFileBase fuMain)
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
                model.type = 13;

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
            return RedirectToAction("LoadCustomerXE1", "XE1", new { type = model.type});
        }

        public ActionResult UpdateXE1(string id)
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
                    return RedirectToAction("LoadCustomerXE1", "XE1", new { type = pro.type });
                }

                return View(pro);
            }
        }

        [HttpPost]
        public ActionResult UpdateXE1(Customer model, HttpPostedFileBase fuMain)
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

            return RedirectToAction("LoadCustomerXE1", "XE1", new { type = model.type });
        }

        public ActionResult SearchXE1(string Code, string Name, string Phone, int type, string Address, int? Noxau, int? hetno, int page = 1)
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
                if (Noxau == 1)
                {
                    List<Customer> lstCus = ctx.Customers.Where(p => p.type == 13).ToList();
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
    }
}