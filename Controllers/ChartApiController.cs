using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using InventoryManagement.Dto;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.Controllers
{
    [Authorize]
    [Route("ChartApi")] // 控制器基路由
    [ApiController]
    public class ChartApiController : Controller
    {
        private readonly WebContext _context;
        private readonly GlobalSettings _globalSettings;

        public ChartApiController(WebContext context, GlobalSettings globalSettings)
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
        [HttpGet("Product_Pie")]
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
        [HttpGet("InventoryLosses_Bar")]
        public ActionResult InventoryLosses_Bar(int month)
        {
            TempData["EmployeeName"] = _globalSettings.employeeName;
            List<string> labelList = new List<string>();
            List<int> dataList = new List<int>();
            ViewBag.thisYear = DateTime.Now.Year;
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now;    

            if (month == 0)
            {
                (startTime, endTime) = getTime(DateTime.Now.Month);
                //startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            else
            {
                (startTime, endTime) = getTime(month);
                //startTime = new DateTime(DateTime.Now.Year, month, 1);
            }

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
            ViewBag.SelectMonth = (month == 0) ? DateTime.Now.ToString("MM") : Convert.ToString(month);
            return View();
        }

        //月份下拉式選單
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


        //商品每月的出貨量(api 傳Json)
        [HttpGet("GetSalesQty/{itemCode}")]
        public ActionResult<IEnumerable<ItemSales>> GetSalesQty(string itemCode)
        {
            List<ItemSales> SalesList = new List<ItemSales>();
            List<int> qtyList = new List<int>();
            List<string> monthList = new List<string>();

            var data = from a in _context.ItemTrans
                       join b in _context.ItemBasic on a.ItemCode equals b.ItemCode
                       where a.ItemCode == itemCode
                       where a.Type == "out"
                       select new
                       {
                           ItemCode = a.ItemCode,
                           ItemName = b.ItemName,
                           TransQty = a.TransQty,
                           SystemTime = a.SystemTime
                       };

            for (int i = 1; i <= 12; i++)
            {
                var (start, end) = getTime(i);

                // 篩選出當月的資料，並加總 TransQty
                var monthlySales = data
                    .Where(x => x.SystemTime >= start && x.SystemTime <= end)
                    .GroupBy(x => new { x.ItemCode, x.ItemName })
                    .Select(g => new
                    {
                        ItemCode = g.Key.ItemCode,
                        ItemName = g.Key.ItemName,
                        Month = i + "月",
                        TotalQty = g.Sum(x => x.TransQty)
                    }).SingleOrDefault();

                if (monthlySales != null)
                {
                    ViewBag.itemName = monthlySales.ItemName;
                    SalesList.Add(new ItemSales
                    {
                        ItemCode = monthlySales.ItemCode,
                        ItemName = monthlySales.ItemName.Trim(),
                        Month = monthlySales.Month,
                        OutQty = monthlySales.TotalQty
                    });
                }
                else
                {
                    ViewBag.Message = ViewBag.itemName + "沒有出貨資料";
                    SalesList.Add(new ItemSales
                    {
                        ItemCode = itemCode,
                        ItemName = "", // 可根據需求填入預設商品名稱
                        Month = i + "月",
                        OutQty = 0
                    });
                }
            }
            return SalesList;
        }


        [HttpGet("SalesQty_Year")]
        public ActionResult SalesQty_Year()
        {
            TempData["EmployeeName"] = _globalSettings.employeeName;
            ViewBag.thisYear = DateTime.Now.Year;
            dropdown_itemName();

            return View();
        }

        //下拉式選單商品名稱
        public void dropdown_itemName()
        {
            List<SelectListItem> itemList = new List<SelectListItem>();

            var data = (from a in _context.ItemBasic
                        where a.Status == "10"
                        select new ItemBasic
                        {
                            ItemCode = a.ItemCode,
                            ItemName = a.ItemName
                        }).ToList();

            if (data != null)
            {
                foreach (var item in data)
                {
                    {
                        itemList.Add(new SelectListItem
                        {
                            Value = item.ItemCode,
                            Text = item.ItemName
                        });
                    }
                }
                ViewData["item"] = itemList;
            }
            else
            {
                ViewData["item"] = itemList;
            }
        }


        //傳入月份，傳回每月月初、月底時間
        public (DateTime startTime, DateTime endTime) getTime(int month)
        {
            DateTime startTime = new DateTime(DateTime.Now.Year, month, 1);
            DateTime endTime = startTime.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            return (startTime, endTime);
        }
    }
}
