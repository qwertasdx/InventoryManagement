using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Models;
using InventoryManagement.Dto;
using InventoryManagement.ViewModel;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using System.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;


namespace InventoryManagement.Controllers
{  
    [Authorize] 
    public class GlobalSettings
    {
        public string employeeName { get; set; }
        public string employeeId { get; set; }
    }

    public class ItemBasicsController : Controller
    {
        private readonly WebContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GlobalSettings _globalSettings;
        private readonly ILogger<ItemBasicsController> _logger;
        public string name = "";
        private object Img;

        public ItemBasicsController(WebContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, GlobalSettings globalSettings,
                                    ILogger<ItemBasicsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _globalSettings = globalSettings;
            _logger = logger;
        }

        // 取得登入的使用者資訊
        public void getUsersInfo()
        {
            var Claim = _httpContextAccessor.HttpContext.User.Claims.ToList();

            // 此段需查看驗證，所設的type與value值(在UserController)
            var employeeName = Claim.Where(x => x.Type == "FullName").First().Value;
            var employeeId = Claim.Where(x => x.Type == "EmployeeId").First().Value;
            TempData["EmployeeName"] = employeeName;
            _globalSettings.employeeName = employeeName.Trim();
            _globalSettings.employeeId = employeeId.Trim();
        }

      
        // GET: ItemBasics
        public async Task<IActionResult> Index(string Status, string Spec , int page = 1, int pageSize = 10)
        {
            getUsersInfo();
            var now = DateTime.Now;
            var ItemBasicSearchViewModel = new ItemBasicSearchViewModel();
            var query = from a in _context.ItemBasic
                               join b in _context.User on a.SystemUser equals b.EmployeeId 
                               join c in _context.ItemStock on a.ItemCode equals c.ItemCode    
                               group new { a,b,c } by new
                               {
                                   a.ItemCode,
                                   a.ItemName,
                                   a.Spec,
                                   a.Status,
                                   a.Img,
                                   a.SystemTime,
                                   b.EmployeeName,
                                   c.TotalQty
                               } into g
                               select new ItemBasics 
                               { 
                                   ItemCode = g.Key.ItemCode,
                                   ItemName = g.Key.ItemName,
                                   Spec = g.Key.Spec,
                                   Status = g.Key.Status,
                                   Img = g.Key.Img,
                                   SystemTime = g.Key.SystemTime,
                                   EmployeeName = g.Key.EmployeeName,
                                   TotalQty  = g.Key.TotalQty
                               };

            if (string.IsNullOrEmpty(Status))
            {
                query = query.Where(x => x.Status == "10");
            }
            else
            {
                query = query.Where(x => x.Status == Status);
            }
  
            if (!string.IsNullOrEmpty(Spec) && Spec != "0")
            {
                query = query.Where(x => x.Spec == Spec);
            }

            // 執行查詢並取得結果
            ItemBasicSearchViewModel.News = await query.OrderByDescending(a => a.SystemTime).ToListAsync();

            ItemBasicSearchViewModel.SpecList.Insert(0, new SelectListItem("全部", "0"));

            //組前端資料
            foreach (var item in ItemBasicSearchViewModel.News) 
            {
                this.StatusName(item);
                item.SpecName = SpecName(item.Spec);
            }

            Pagination(page, pageSize, ItemBasicSearchViewModel);

            ViewBag.selectSpec2 = Spec;
            ViewBag.selectStatus2 = Status;
            return View(ItemBasicSearchViewModel);
        }

        //設定分頁，10筆資料為一頁
        public void Pagination(int page, int pageSize, ItemBasicSearchViewModel ItemBasicSearchViewModel)
        {
            var items = ItemBasicSearchViewModel.News; // 獲取所有項目
            var totalItems = items.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ItemBasicSearchViewModel.News = pagedItems;
            ItemBasicSearchViewModel.CurrentPage = page;
            ItemBasicSearchViewModel.TotalPages = totalPages;        
        }

        public string GetImg(string id)
        {
            var result = (from a in _context.ItemBasic
                          where a.ItemCode == id
                          select  a.Img).SingleOrDefault();
              
            return GetImageBase64(result);
        }

        // GET: ItemBasics/Create
        public IActionResult Create()
        {
            TempData["EmployeeName"] = _globalSettings.employeeName;
            var ItemBasicViewModel = new ItemBasicCreateViewModel();
            return View(ItemBasicViewModel);
        }

        // POST: ItemBasics/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemBasicCreateViewModel ItemBasicCreateViewModel, IFormFile myimg)
        {
            string itemCode = generateItemCode(ItemBasicCreateViewModel.News);
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    ItemBasic insert = new ItemBasic()
                    {
                        ItemCode = itemCode,
                        ItemName = ItemBasicCreateViewModel.News.ItemName.Trim(),
                        Spec = ItemBasicCreateViewModel.News.Spec,
                        Status = ItemBasicCreateViewModel.News.Status,
                        SystemUser = _globalSettings.employeeId.Trim()
                    };

                    // 將圖片能夠儲存在資料庫的 byte[] 型別欄位
                    using (var ms = new MemoryStream())
                    {
                        myimg.CopyTo(ms);
                        insert.Img = ms.ToArray();
                    }
                    
                    ItemStock insert2 = new ItemStock()
                    {
                        ItemCode = itemCode,
                        Unit = ItemBasicCreateViewModel.News.Unit,
                        SafeQty = 0,
                        TotalQty = 0,
                        Status = "10", //啟用10，停用20
                        SystemUser = _globalSettings.employeeId.Trim()
                    };

                    _context.Add(insert);
                    _context.Add(insert2);
                    await _context.SaveChangesAsync();

                    // 交易確認
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    //交易回復
                    transaction.RollbackAsync();
                    _logger.LogWarning("LogWarning-ItemBasics/Create"+ ex.ToString());
                    ModelState.AddModelError("", "存檔失敗，請稍後再試。");
                    return View(ItemBasicCreateViewModel);
                }
            }                       
            return RedirectToAction(nameof(Index));          
    }


    //新建商品時，產生商品貨號
    public string generateItemCode(CreateBasics news)
        {
            var itemCode = "0";
            if (news.Spec != null)
            {
                var result = (from a in _context.ItemBasic
                              where a.Spec == news.Spec
                              orderby a.ItemCode descending
                              select a).FirstOrDefault();

                itemCode = result?.ItemCode;
                if(itemCode != "0" && result != null)
                {
                    int number = Convert.ToInt32(itemCode.Substring(3))+1;
                    itemCode = itemCode.Substring(0,3)+ Convert.ToString(number).PadLeft(5,'0');
                }
                else
                {
                    string tmp = this.getSpecToItemCode(news.Spec).PadRight(7,'0');
                    itemCode = tmp.PadRight(8, '1');
                }
            }
            return itemCode;    
        }

  
        // GET: ItemBasics/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            TempData["EmployeeName"] = _globalSettings.employeeName;
            if (id == null)
            {
                return NotFound();
            }
            var ItemBasicsEditViewModel = new ItemBasicsEditViewModel();

            ItemBasicsEditViewModel.News =  (from a in _context.ItemBasic
                                            where a.ItemCode == id
                                            select new ItemBasicsEdit
                                            {
                                                ItemCode = a.ItemCode,
                                                ItemName = a.ItemName,
                                                Img = a.Img,
                                                Spec = a.Spec,
                                                Status = a.Status
                                            }).SingleOrDefault();

            if (ItemBasicsEditViewModel.News == null)
            {
                return NotFound();
            }

            ViewBag.selectStatus = ItemBasicsEditViewModel.News.Status.Trim();
            ItemBasicsEditViewModel.News.SpecName = SpecName(ItemBasicsEditViewModel.News.Spec);

            ItemBasicsEditViewModel.imageBase64 = GetImageBase64(ItemBasicsEditViewModel.News.Img);

            return View(ItemBasicsEditViewModel);
        }

        // POST: ItemBasics/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemBasicsEdit news, IFormFile myimg)
        {
            var ItemBasicsEditViewModel = new ItemBasicsEditViewModel();
            ItemBasicsEditViewModel.News = news;

            if(myimg == null)
            {
                ItemBasicsEditViewModel.imageBase64 = GetImageBase64(ItemBasicsEditViewModel.News.Img);
            }
   
            
            if (string.IsNullOrEmpty(news.ItemCode))
            {
                ModelState.AddModelError("", "查無此商品，請稍後再試。");
                return View(ItemBasicsEditViewModel);
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var update = _context.ItemBasic.Find(news.ItemCode);
                    var update2 = _context.ItemStock.Find(news.ItemCode);

                    if (update != null && update2 != null)
                    {
                        update.ItemName = news.ItemName;
                        update.Status = news.Status;
                        update.SystemUser = _globalSettings.employeeId.Trim();

                        // 狀態修改，ItemStock也需更動
                        if (update2.Status != news.Status)
                        {
                            update2.Status = news.Status;
                            update2.SystemUser = _globalSettings.employeeId.Trim();
                        }

                        using (var ms = new MemoryStream())
                        {
                            //有更新圖片
                            if (myimg != null)
                            {
                                myimg.CopyTo(ms);
                                update.Img = ms.ToArray();
                            }
                        }

                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex) {
                    //交易回復                
                    transaction.RollbackAsync();
                    ModelState.AddModelError("", "存檔失敗，請稍後再試。");
                    return View(ItemBasicsEditViewModel);
                }
            }
            return View(ItemBasicsEditViewModel);
        }

        //將圖片轉換成Base64，在畫面顯示
        public static string GetImageBase64(byte[] imageBytes)
        {
            if (imageBytes == null)
            {
                return "";
            }

            MemoryStream ms = new MemoryStream();
            Image image = Image.FromStream(new MemoryStream(imageBytes));
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            image.Dispose();
            return Convert.ToBase64String(ms.ToArray());
        }


        // POST: ItemBasics/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var itemBasic = await _context.ItemBasic.FindAsync(id);
                    var itemStock = await _context.ItemStock.FindAsync(id);

                    if (itemBasic != null && itemStock != null)
                    {
                        _context.ItemBasic.Remove(itemBasic);
                        _context.ItemStock.Remove(itemStock);
                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.RollbackAsync();
                    ModelState.AddModelError("", "刪除失敗，請稍後再試。");
                }

                return RedirectToAction(nameof(Index));
            }
        }

        private bool ItemBasicExists(string id)
        {
            return _context.ItemBasic.Any(e => e.ItemCode == id);
        }

        // 對照狀態、分類名稱
        public void StatusName(ItemBasics item)
        {
            switch (item.Status.Trim())
            {
                case "10":
                    item.StatusName = "啟用";
                    break;
                case "20":
                    item.StatusName = "停用";
                    break;
                case "30":
                    item.StatusName = "缺貨";
                    break;
                default:
                    item.StatusName = "啟用";
                    break;
            }
        }

        public string SpecName(string spec)
        {
            var specName = "";
            switch (spec.Trim())
            {
                case "10":
                    specName = "短袖";
                    break;
                case "11":
                    specName = "長袖";
                    break;
                case "12":
                    specName = "背心";
                    break;
                case "20":
                    specName = "短褲";
                    break;
                case "21":
                    specName = "長褲";
                    break;
                case "30":
                    specName = "包包";
                    break;
                case "31":
                    specName = "飾品";
                    break;
                default:
                    specName = "全部";
                    break;
            }
            return specName;
        }

        //依據分類轉為商品貨號(前3碼)
        public string getSpecToItemCode(string spec)
        {
            var num= "0";
            switch (spec.Trim()) {
                case "10":  //短袖
                   num = "A01";
                   break;
                case "11":  //長袖
                    num = "A02";
                    break;
                case "12":  //背心
                    num = "A03";
                    break;
                case "20":  //短褲
                    num = "B01";
                    break;
                case "21":  //長褲
                    num = "B02";
                    break;
                case "30":  //包包
                    num = "C01";
                    break;
                case "31":  //飾品
                    num = "C02";
                    break;
            }
            return num;
        }
    }
}
