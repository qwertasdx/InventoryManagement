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
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Controllers
{
    [Authorize]
    public class ItemBasicsController : Controller
    {
        private readonly WebContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public string name = "";
        public ItemBasicsController(WebContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        // 取得登入的使用者資訊
        public void getUsersInfo()
        {
            var Claim = _httpContextAccessor.HttpContext.User.Claims.ToList();

            // 此段需查看驗證，所設的type與value值(在UserController)
            var employeeName = Claim.Where(x => x.Type == "FullName").First().Value;
            TempData["EmployeeName"] = employeeName;
            name = employeeName;

        }

        // GET: ItemBasics
        public async Task<IActionResult> Index()
        {
            getUsersInfo();
            var now = DateTime.Now;
            var ItemBasicSearchViewModel = new ItemBasicSearchViewModel();
            ItemBasicSearchViewModel.News = await(from a in _context.ItemBasic
                               join b in _context.Employee on a.SystemUser equals b.EmployeeId
                               where a.StartTime <= now && a.EndTime >= now
                               orderby a.SystemTime descending
                               select new ItemBasics 
                               { 
                                    ItemCode = a.ItemCode,
                                    ItemName = a.ItemName,
                                    Spec = a.Spec,
                                    Status = a.Status,
                                    Unit = a.Unit,
                                    EmployeeName = b.EmployeeName,
                                    StartTime = a.StartTime,
                                    EndTime = a.EndTime,
                               }).ToListAsync();

            ItemBasicSearchViewModel.SpecList.Insert(0, new SelectListItem("全部", "0"));

            //組前端資料
            foreach (var item in ItemBasicSearchViewModel.News)
            {
                this.StatusName(item);
                this.SpecName(item);
            }

            return View(ItemBasicSearchViewModel);
        }

        public async Task<IActionResult> SearchRule(string Status, string Spec)
        {
            var now = DateTime.Now;
            var ItemBasicSearchViewModel = new ItemBasicSearchViewModel();
            var result = from a in _context.ItemBasic
                         join b in _context.Employee on a.SystemUser equals b.EmployeeId
                         where a.Status == Status
                         select new ItemBasics
                         {
                             ItemCode = a.ItemCode,
                             ItemName = a.ItemName,
                             Spec = a.Spec,
                             Status = a.Status,
                             Unit = a.Unit,
                             EmployeeName = b.EmployeeName,
                             StartTime = a.StartTime,
                             EndTime = a.EndTime
                         };

            ItemBasicSearchViewModel.SpecList.Insert(0, new SelectListItem("全部", "0"));
            ViewBag.selectSpec2 = Spec;
            ViewBag.selectStatus2 = Status;

            if (Spec != "0")
            {
                result = result.Where(x => x.Spec == Spec);
            }

            ItemBasicSearchViewModel.News = result.ToList();

            // 組資料
            foreach (var item in ItemBasicSearchViewModel.News)
            {
                this.StatusName(item);
                this.SpecName(item);
            }

            return View("Index", ItemBasicSearchViewModel);          
        }

        // 下架時間到，要將狀態改為停用
        public async Task<IActionResult>  updateStatus()
        {
            List<ItemBasicsEditViewModel> updateList = new List<ItemBasicsEditViewModel>();
            var result = from a in _context.ItemBasic
                         where a.EndTime <= DateTime.Now
                         select a;

            result = result.Where(x => x.Status != "停用");

            if (result != null)
            {
                foreach (var temp in result)
                {
                    temp.Status = "20";
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ItemBasics/Create
        public IActionResult Create()
        {
            var ItemBasicViewModel = new ItemBasicCreateViewModel();          
            return View(ItemBasicViewModel);
        }

        // POST: ItemBasics/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBasics news)
        {
            string itemCode = generateItemCode(news);
            if (ModelState.IsValid)
            {            
                ItemBasic insert = new ItemBasic()
                {
                    ItemCode = itemCode,
                    ItemName = news.ItemName,
                    Spec = news.Spec,
                    Unit = news.Unit,
                    Status = news.Status,
                    StartTime = news.StartTime,
                    EndTime = news.EndTime,
                    SystemUser = "A35455"
                };

                _context.Add(insert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(news);
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
                                                Spec = a.Spec,
                                                Unit = a.Unit,
                                                Status = a.Status,
                                                StartTime = a.StartTime,
                                                EndTime = a.EndTime
                                            }).SingleOrDefault();

            ViewBag.selectUnit = ItemBasicsEditViewModel.News.Unit.Trim();
            ViewBag.selectSpec = ItemBasicsEditViewModel.News.Spec.Trim();
            ViewBag.selectStatus = ItemBasicsEditViewModel.News.Status.Trim();

            if (ItemBasicsEditViewModel.News == null)
            {
                return NotFound();
            }
            return View(ItemBasicsEditViewModel);
        }

        // POST: ItemBasics/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ItemBasicsEdit news)
        {
            if (id.Trim() != news.ItemCode.Trim())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var update = _context.ItemBasic.Find(id);

                if (update != null)
                { 
                    update.ItemName = news.ItemName;
                    update.Spec = news.Spec;
                    update.Unit = news.Unit;
                    update.Status = news.Status;
                    update.StartTime = news.StartTime;
                    update.EndTime = news.EndTime;
                    update.SystemUser = "A35455";

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(news);
        }

        // GET: ItemBasics/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemBasic = await _context.ItemBasic
                .FirstOrDefaultAsync(m => m.ItemCode == id);
            if (itemBasic == null)
            {
                return NotFound();
            }

            return View(itemBasic);
        }

        // POST: ItemBasics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var itemBasic = await _context.ItemBasic.FindAsync(id);
            if (itemBasic != null)
            {
                _context.ItemBasic.Remove(itemBasic);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        public void SpecName(ItemBasics item)
        {
            switch (item.Spec.Trim())
            {
                case "10":
                    item.SpecName = "短袖";
                    break;
                case "11":
                    item.SpecName = "長袖";
                    break;
                case "12":
                    item.SpecName = "背心";
                    break;
                case "20":
                    item.SpecName = "短褲";
                    break;
                case "21":
                    item.SpecName = "長褲";
                    break;
                case "30":
                    item.SpecName = "包包";
                    break;
                case "31":
                    item.SpecName = "飾品";
                    break;
                default:
                    item.SpecName = "全部";
                    break;
            }
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
