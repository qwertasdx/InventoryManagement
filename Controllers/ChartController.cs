using DocumentFormat.OpenXml.Drawing;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.Controllers
{
    public class ChartController : Controller
    {
        private readonly WebContext _context;
        private readonly GlobalSettings _globalSettings;

        public ChartController(WebContext context, GlobalSettings globalSettings)
        {
            _context = context;
            _globalSettings = globalSettings;
        }

        // GET: Chart
        public ActionResult Index()
        {
            return View();
        }


        //商品各類別數量分布
        public ActionResult Product_Pie()
        {
            TempData["EmployeeName"] = _globalSettings.employeeName;
            List<int> dataList = new List<int>();

            var data = _context.ItemBasic.
                       GroupBy(m => m.Spec).
                       Select(x => new { label = x.Key, data = x.Count() }).ToList();

            foreach (var item in data)
            {
                dataList.Add(item.data);
            }

            //序列化
            ViewBag.data = System.Text.Json.JsonSerializer.Serialize(dataList);
            return View();
        }

        // 列出盤虧數量前五名的商品
        public ActionResult InventoryLosses_Bar(int month)
        {
            TempData["EmployeeName"] = _globalSettings.employeeName;
            List<string> labelList = new List<string>();
            List<int> dataList = new List<int>();
            ViewBag.thisYear = DateTime.Now.Year;
            DateTime startTime = DateTime.Now;

            if (month == 0)
            {
                startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            else
            {
                startTime = new DateTime(DateTime.Now.Year, month, 1);
            }
       
            DateTime endTime = startTime.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);


            var data = (from a in _context.ItemTrans2
                       join b in _context.ItemBasic on a.ItemCode equals b.ItemCode 
                       where a.Type == "out"
                       where a.SystemTime >= startTime && a.SystemTime <= endTime
                       group new { a, b } by new { a.ItemCode, b.ItemName } into g
                       select new
                       {
                           ItemCode = g.Key.ItemCode,
                           ItemName = g.Key.ItemName,
                           TotalTransQty = g.Sum(x => x.a.TransQty)
                       }).OrderBy(x => x.TotalTransQty) // 排序後，取前五名
                         .Take(5).ToList(); ;

            if (data.Count != 0)
            {
                foreach (var item in data)
                {
                    labelList.Add(item.ItemName);
                    dataList.Add(item.TotalTransQty);
                }

                ViewData["Labels"] = System.Text.Json.JsonSerializer.Serialize(labelList);
                ViewData["TotalTransQty"] = System.Text.Json.JsonSerializer.Serialize(dataList);      
            }
            else
            {
                ViewData["Labels"] = "[]";
                ViewData["TotalTransQty"] = "[]";
                ViewBag.Message = "該月份沒有資料。";
            }

            dropdown_month();
            ViewBag.SelectMonth = Convert.ToString(month);
            return View();
        }

        public void dropdown_month()
        {
            List<SelectListItem> MonthList = new List<SelectListItem>();
            for (int month = 1; month <= 12; month++)
            {
                MonthList.Add(new SelectListItem
                {
                    Value = month.ToString(),
                    Text = $"{month} 月"
                });
            }
            ViewData["Month"] = MonthList;
        }

        // POST: Chart/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Chart/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Chart/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Chart/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Chart/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
