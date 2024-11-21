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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InventoryManagement.Controllers
{
    public class ItemStocksController : Controller
    {
        private readonly WebContext _context;
        private readonly GlobalSettings _globalSettings;

        public ItemStocksController(WebContext context, GlobalSettings globalSettings)
        {
            _context = context;
            _globalSettings = globalSettings;
        }

 
        // GET: ItemStocks 出入庫
        public async Task<IActionResult> Index(string itemCode)
        {
            // 初始化結果為空集合
            var query = Enumerable.Empty<ItemStocks>().AsQueryable();

            // 只有當 itemCode 不為空時才查詢資料
            if (!string.IsNullOrEmpty(itemCode))
            {
                query = from a in _context.ItemBasic
                         join b in _context.ItemStock on a.ItemCode equals b.ItemCode
                         where b.ItemCode == itemCode // 直接在這裡過濾
                         select new ItemStocks
                         {
                             ItemCode = b.ItemCode,
                             ItemName = a.ItemName,
                             Unit = b.Unit,
                             TotalQty = b.TotalQty,
                             Status = b.Status,
                             EmployeeId = b.SystemUser                            
                         };
            }

            if (!string.IsNullOrEmpty(itemCode))  
            {
                query = query.Where(x => x.ItemCode == itemCode);
                var result = query.FirstOrDefault();

                StatusName(result);
                return View(result);
            }

            return View(query.FirstOrDefault());
        }

        //計算出入庫數量
        [HttpPost]
        public async Task<IActionResult> saveQty(string itemCode, int qty, string flexRadioDefault)
        {
            var product = _context.ItemStock.FirstOrDefault(p => p.ItemCode == itemCode);
            if (product != null)
            {
                if (flexRadioDefault == "instock")
                {
                    product.TotalQty += qty;
                }
                else if (flexRadioDefault == "outstock")
                {
                    product.TotalQty -= qty;
                }
                product.SystemUser = _globalSettings.employeeId;
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> searchItemStocks(string itemCode,string Status,int page = 1, int pageSize = 10)
        {
            var ItemStocksViewModel = new ItemStocksViewModel();
            var query = from a in _context.ItemStock
                                           join b in _context.ItemBasic on a.ItemCode equals b.ItemCode
                                           join c in _context.User on b.SystemUser equals c.EmployeeId
                                           group new { a, b, c } by new
                                           {
                                               a.ItemCode,
                                               b.ItemName,
                                               a.Unit,
                                               a.SafeQty,
                                               a.TotalQty,
                                               a.Status,
                                               c.EmployeeName
                                           } into g
                                           select new ItemStocks
                                           {
                                               ItemCode = g.Key.ItemCode,
                                               ItemName = g.Key.ItemName,
                                               Unit = g.Key.Unit,
                                               SafeQty = g.Key.SafeQty,
                                               TotalQty = g.Key.TotalQty,
                                               Status = g.Key.Status,
                                               EmployeeName = g.Key.EmployeeName
                                           };

            if (!string.IsNullOrEmpty(Status))
            {
                query = query.Where(x=>x.Status == Status);
            }

            if (!string.IsNullOrEmpty(itemCode))
            {
                query = query.Where(x => x.ItemCode == itemCode);
            }

            ItemStocksViewModel.Products = await query.ToListAsync();

            foreach (var item in ItemStocksViewModel.Products)
            {
                StatusName(item);
            }

            Pagination(page, pageSize, ItemStocksViewModel);
            ViewBag.selectStatus = Status;

            return View(ItemStocksViewModel);         
        }

        //設定分頁，10筆資料為一頁
        public void Pagination(int page, int pageSize, ItemStocksViewModel ItemStocksViewModel)
        {
            var items = ItemStocksViewModel.Products; // 獲取所有項目
            var totalItems = items.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ItemStocksViewModel.Products = pagedItems;
            ItemStocksViewModel.CurrentPage = page;
            ItemStocksViewModel.TotalPages = totalPages;
        }

        // GET: ItemStocks/Create
       /* public IActionResult Create()
        {
            return View();
        }

        // POST: ItemStocks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemCode,Unit,SafeQty,TotalQty,Status,SystemUser,SystemTime")] ItemStock itemStock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(itemStock);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(itemStock);
        }*/

        // GET: ItemStocks/Edit/5
        public async Task<IActionResult> Edit(string itemCode, string itemName)
        {
            if (itemCode == null)
            {
                return NotFound();
            }
            var ItemStocksEditViewModel = new ItemStocksEditViewModel();

            // 總庫存量不可改動
            ItemStocksEditViewModel.Product =  await(from a in _context.ItemStock
                                                where a.ItemCode == itemCode
                                                select new ItemStocksEdit
                                                {
                                                    ItemCode = a.ItemCode,
                                                    ItemName = itemName,
                                                    Unit = a.Unit,
                                                    SafeQty = a.SafeQty,
                                                    TotalQty = a.TotalQty,
                                                    Status = a.Status
                                                }).SingleOrDefaultAsync();

            
            if (ItemStocksEditViewModel.Product == null)
            {
                return NotFound();
            }
            return View(ItemStocksEditViewModel);
        }

        // POST: ItemStocks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemStocksEdit Product)
        {
            ModelState["Product.ItemName"].ValidationState = ModelValidationState.Valid; 
            if (ModelState.IsValid)
            {
                var update = _context.ItemStock.Find(Product.ItemCode); 

                if (update != null)
                {
                    update.SafeQty = Product.SafeQty;
                    update.Unit = Product.Unit;
                    update.Status = Product.Status;
                    update.SystemUser = _globalSettings.employeeId.Trim();

                    await _context.SaveChangesAsync();
                }
                                  
                return RedirectToAction(nameof(searchItemStocks));
            }
            return View(Product);
        }

        // GET: ItemStocks/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemStock = await _context.ItemStock
                .FirstOrDefaultAsync(m => m.ItemCode == id);
            if (itemStock == null)
            {
                return NotFound();
            }

            return View(itemStock);
        }

        // POST: ItemStocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var itemStock = await _context.ItemStock.FindAsync(id);
            if (itemStock != null)
            {
                _context.ItemStock.Remove(itemStock);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemStockExists(string id)
        {
            return _context.ItemStock.Any(e => e.ItemCode == id);
        }

        // 對照狀態
        public void StatusName(ItemStocks item)
        {
            switch (item.Status.Trim())
            {
                case "10":
                    item.StatusName = "啟用";
                    break;
                case "20":
                    item.StatusName = "停用";
                    break;
                default:
                    item.StatusName = "啟用";
                    break;
            }
        }
    }
}
