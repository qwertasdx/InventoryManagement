﻿using System;
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
using Humanizer;

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
            var type ="";
          
            if (product != null)
            {
                if (flexRadioDefault == "instock")
                {
                    product.TotalQty += qty;
                    type = "in";
                }
                else if (flexRadioDefault == "outstock")
                {
                    product.TotalQty -= qty;
                    type = "out";
                }
                product.SystemUser = _globalSettings.employeeId;

                //新增出入庫紀錄
                Models.ItemTrans insert = new Models.ItemTrans()
                {
                    ItemCode = itemCode,
                    Type = type,
                    TransQty = qty,
                    Unit = product.Unit,
                    SystemUser = _globalSettings.employeeId
                };
                _context.Update(product);
                _context.Add(insert);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // 查詢所有商品庫存
        public async Task<IActionResult> SearchItemStocks(string itemCode,string Status,int page = 1, int pageSize = 10)
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
            else
            {
                query = query.Where(x => x.Status == "10");
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
                                  
                return RedirectToAction(nameof(SearchItemStocks));
            }
            
            return View(Product);
        }

        //商品出入庫紀錄查詢
        public async Task<IActionResult> SearchItemTrans(string type , DateTime startDate, DateTime endDate,int page = 1, int pageSize = 10)
        {
            var result = from a in _context.ItemTrans
                         join b in _context.ItemBasic on a.ItemCode equals b.ItemCode
                         join c in _context.User on a.SystemUser equals c.EmployeeId
                         group new { a, b, c } by new
                         {
                             ItemCode = a.ItemCode,
                             ItemName = b.ItemName,
                             Unit = a.Unit,
                             TransQty = a.TransQty,
                             EmployeeName = c.EmployeeName,
                             SystemTime = a.SystemTime,
                             Type = a.Type,
                         } into g
                         select new Dto.ItemTrans
                         {
                             ItemCode = g.Key.ItemCode,
                             ItemName = g.Key.ItemName,
                             Unit = g.Key.Unit,
                             TransQty = g.Key.TransQty,
                             EmployeeName = g.Key.EmployeeName,
                             SystemTime = g.Key.SystemTime,
                             Type = g.Key.Type
                         };

            if (!string.IsNullOrEmpty(type))
            {
                result = result.Where(x => x.Type == type);
            }

            if (startDate != DateTime.MinValue && endDate != DateTime.MinValue)
            {
                result = result.Where(x => x.SystemTime >= startDate && x.SystemTime <= endDate);
                @ViewBag.startDate = startDate.ToString("yyyy-MM-dd");
                @ViewBag.endDate = endDate.ToString("yyyy-MM-dd");
            }

            var ItemTransViewModel = new ItemTransViewModel();
            ItemTransViewModel.Trans = await result.ToListAsync();

            foreach (var item in ItemTransViewModel.Trans)
            {
                typeName(item);
            }
            @ViewBag.selectType = type;
            Pagination2(page, pageSize, ItemTransViewModel);

            return View(ItemTransViewModel);
        }

        //設定分頁，10筆資料為一頁
        public void Pagination2(int page, int pageSize, ItemTransViewModel ItemTransViewModel)
        {
            var items = ItemTransViewModel.Trans; // 獲取所有項目
            var totalItems = items.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ItemTransViewModel.Trans = pagedItems;
            ItemTransViewModel.CurrentPage = page;
            ItemTransViewModel.TotalPages = totalPages;
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

        public void typeName(Dto.ItemTrans item)
        {
            switch (item.Type.Trim())
            {
                case "in":
                    item.TypeName = "入貨";
                    break;
                case "out":
                    item.TypeName = "出貨";
                    break;
                default:
                    item.TypeName = "";
                    break;
            }
        }
    }
}
