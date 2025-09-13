using AppData.Models;
using AppData.ViewModels;
using AppData.ViewModels.BanOffline;
using AppData.ViewModels.DTO;
using AppData.ViewModels.Mail;
using AppData.ViewModels.QLND;
using AppData.ViewModels.SanPham;
using AppData.ViewModels.VNPay;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2016.Excel;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using TechTalk.SpecFlow.Assist;
using TechTalk.SpecFlow.Infrastructure;
using X.PagedList;


namespace AppView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7095/api/");
        }
        public async Task<IActionResult> Index()
        {
            // lam start
            try
            {
                var session = HttpContext.Session.GetString("LoginInfor");
                if (String.IsNullOrEmpty(session))
                {
                    List<GioHangRequest> lstGioHang = new List<GioHangRequest>();
                    if (Request.Cookies["Cart"] != null)
                    {
                        lstGioHang = JsonConvert.DeserializeObject<List<GioHangRequest>>(Request.Cookies["Cart"]);
                    }
                    // laam them
                    int cout = lstGioHang.Sum(c => c.SoLuong);
                    TempData["SoLuong"] = cout.ToString();

                    if (Request.Cookies["Cart"] != null)
                    {
                        var response = await _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCart?request=" + Request.Cookies["Cart"]);
                        if (response.IsSuccessStatusCode)
                        {
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response.Content.ReadAsStringAsync().Result);


                            // lam end

                            TempData["TrangThai"] = "false";
                            return View(temp.GioHangs);
                        }
                        else return BadRequest();
                    }
                    else
                    {
                        TempData["TongTien"] = "0";
                        return View(new List<GioHangRequest>());
                    }
                }
                else
                {
                    var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    if (loginInfor.vaiTro == 1)
                    {
                        var response = await _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCartLogin?idNguoiDung=" + loginInfor.Id);
                        if (response.IsSuccessStatusCode)
                        {
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response.Content.ReadAsStringAsync().Result);


                            // lam them
                            int cout = temp.GioHangs.Sum(c => c.SoLuong);

                            TempData["SoLuong"] = cout.ToString();
                            // lam end
                            TempData["TrangThai"] = "true";
                            return View(temp.GioHangs);
                        }
                        else return BadRequest();
                    }
                    else
                    {
                        TempData["SoLuong"] = "0";
                        return View(new List<GioHangRequest>());
                    }

                }
            }
            catch
            {
                TempData["TongTien"] = "0";
                return View(new List<GioHangRequest>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult ProductDetail()
        {
            return View();
        }

        #region SanPham
        [HttpPost]
        public JsonResult ShowProduct(FilterData filter)
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/getAll").Result;
                List<SanPhamViewModel> lstSanpham = new List<SanPhamViewModel>();
                if (response.IsSuccessStatusCode)
                {
                    lstSanpham = JsonConvert.DeserializeObject<List<SanPhamViewModel>>(response.Content.ReadAsStringAsync().Result);
                    //
                    List<SanPhamViewModel> lstSanphamfn = new List<SanPhamViewModel>();
                    //price-filter
                    if (filter.priceRange != null && filter.priceRange.Count > 0 && !filter.priceRange.Contains("all"))
                    {
                        foreach (var item in filter.priceRange)
                        {
                            if (item == "1")
                            {
                                foreach (var x in lstSanpham.Where(p => p.GiaBan < 100000 && p.TrangThaiCTSP == 1).ToList())
                                {
                                    lstSanphamfn.Add(x);
                                }
                            }
                            else if (item == "2")
                            {
                                foreach (var x in lstSanpham.Where(p => p.GiaBan >= 100000 && p.GiaBan < 200000 && p.TrangThaiCTSP == 1).ToList())
                                {
                                    lstSanphamfn.Add(x);
                                }
                            }
                            else if (item == "3")
                            {
                                foreach (var x in lstSanpham.Where(p => p.GiaBan >= 200000 && p.GiaBan < 300000 && p.TrangThaiCTSP == 1).ToList())
                                {
                                    lstSanphamfn.Add(x);
                                }
                            }
                            else if (item == "4")
                            {
                                foreach (var x in lstSanpham.Where(p => p.GiaBan >= 300000 && p.GiaBan < 400000 && p.TrangThaiCTSP == 1).ToList())
                                {
                                    lstSanphamfn.Add(x);
                                }
                            }
                            else if (item == "5")
                            {
                                foreach (var x in lstSanpham.Where(p => p.GiaBan >= 400000 && p.GiaBan < 500000 && p.TrangThaiCTSP == 1).ToList())
                                {
                                    lstSanphamfn.Add(x);
                                }
                            }
                            else if (item == "6")
                            {
                                foreach (var x in lstSanpham.Where(p => p.GiaBan > 500000 && p.TrangThaiCTSP == 1).ToList())
                                {
                                    lstSanphamfn.Add(x);
                                }
                            }

                        }
                    }
                    else
                    {
                        lstSanphamfn = lstSanpham;
                    }
                    //loaiSP-filter
                    List<SanPhamViewModel> lsttam = new List<SanPhamViewModel>();
                    List<SanPhamViewModel> lsttam1 = new List<SanPhamViewModel>();
                    if (filter.loaiSP != null && filter.loaiSP.Count() > 0)
                    {
                        var ids = filter.loaiSP; // List<Guid> hoặc List<string> tùy kiểu
                        lstSanphamfn = lstSanphamfn
                            .Where(p => filter.loaiSP.Contains(p.LoaiSP)) // nếu lọc theo ID
                            .ToList();
                    }
                    //Search
                    if (filter.search != null)
                    {
                        lstSanphamfn = lstSanphamfn.Where(p => p.Ten.ToLower().Contains(filter.search.ToLower())).ToList();
                    }

                    List<SanPhamViewModel> lstcotam = new List<SanPhamViewModel>();
                    List<SanPhamViewModel> lstcotam1 = new List<SanPhamViewModel>();
                    if (filter.kichCo != null && filter.kichCo.Count > 0)
                    {
                        foreach (var x in filter.kichCo)
                        {
                            lstcotam = lstSanphamfn.Where(p => p.IDKichCo == x).ToList();
                            foreach (var item in lstcotam)
                            {
                                if (lstcotam1.FirstOrDefault(p => p.ID == item.ID) == null)
                                {
                                    lstcotam1.Add(item);
                                }
                            }
                        }
                        lstSanphamfn = lstcotam1;
                    }
                    //sort
                    if (filter.sortSP != null)
                    {
                        if (filter.sortSP == "2")
                        {
                            lstSanphamfn = lstSanphamfn.OrderBy(p => p.GiaBan).ToList();
                        }
                        else if (filter.sortSP == "3")
                        {
                            lstSanphamfn = lstSanphamfn.OrderByDescending(p => p.GiaBan).ToList();
                        }
                        else if (filter.sortSP == "4")
                        {
                            lstSanphamfn = lstSanphamfn.OrderBy(p => p.GiaBan).ToList();
                        }
                        else if (filter.sortSP == "5")
                        {
                            lstSanphamfn = lstSanphamfn.OrderByDescending(p => p.GiaBan).ToList();
                        }
                        else if (filter.sortSP == "6")
                        {
                            lstSanphamfn = lstSanphamfn.OrderBy(p => p.NgayTao.Value).ToList();
                        }
                        else if (filter.sortSP == "7")
                        {
                            lstSanphamfn = lstSanphamfn.OrderByDescending(p => p.NgayTao.Value).ToList();
                        }
                        else if (filter.sortSP == "9")
                        {
                            lstSanphamfn = lstSanphamfn.OrderByDescending(p => p.SoLuong).ToList();
                        }
                    }
                    List<SanPhamViewModel> lstSanPhamfnR = new List<SanPhamViewModel>();
                    foreach (var item in lstSanphamfn)
                    {
                        if (item.TrangThaiCTSP == 1)
                        {
                            if (lstSanPhamfnR.FirstOrDefault(p => p.ID == item.ID) == null)
                            {
                                lstSanPhamfnR.Add(item);
                            }
                        }
                        else
                        {
                            SanPhamViewModel sanPhamViewModel = lstSanpham.FirstOrDefault(p => p.ID == item.ID && p.TrangThaiCTSP == 1);
                            if (sanPhamViewModel != null)
                            {
                                if (lstSanPhamfnR.FirstOrDefault(p => p.ID == sanPhamViewModel.ID) == null)
                                {
                                    lstSanPhamfnR.Add(sanPhamViewModel);
                                }
                            }
                        }
                    }
                    //

                    var model = lstSanPhamfnR.Skip((filter.page - 1) * filter.pageSize).Take(filter.pageSize).ToList();
                    return Json(new
                    {
                        data = model,
                        total = lstSanPhamfnR.Count,
                        status = true
                    });
                }
                else return Json(new { status = false });
            }
            catch
            {
                return Json(new { status = false });
            }
        }
        [HttpGet]
        public IActionResult Shop()
        {
            HttpResponseMessage responseLoaiSP = _httpClient.GetAsync(_httpClient.BaseAddress + "LoaiSP/getAll").Result;
            if (responseLoaiSP.IsSuccessStatusCode)
            {
                ViewData["listLoaiSP"] = JsonConvert.DeserializeObject<List<LoaiSP>>(responseLoaiSP.Content.ReadAsStringAsync().Result);
            }
            HttpResponseMessage responseMauSac = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllMauSac").Result;
            if (responseMauSac.IsSuccessStatusCode)
            {
                ViewData["listMauSac"] = JsonConvert.DeserializeObject<List<MauSac>>(responseMauSac.Content.ReadAsStringAsync().Result);
            }
            HttpResponseMessage responseKichCo = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllKichCo").Result;
            if (responseKichCo.IsSuccessStatusCode)
            {
                ViewData["listKichCo"] = JsonConvert.DeserializeObject<List<KichCo>>(responseKichCo.Content.ReadAsStringAsync().Result);
            }
            HttpResponseMessage responseChatLieu = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllChatLieu").Result;
            if (responseChatLieu.IsSuccessStatusCode)
            {
                ViewData["listChatLieu"] = JsonConvert.DeserializeObject<List<ChatLieu>>(responseChatLieu.Content.ReadAsStringAsync().Result);
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ProductDetail(string idSanPham)
        { // lam start
            try
            {
                var session = HttpContext.Session.GetString("LoginInfor");
                if (String.IsNullOrEmpty(session))
                {
                    List<GioHangRequest> lstGioHang = new List<GioHangRequest>();
                    if (Request.Cookies["Cart"] != null)
                    {
                        lstGioHang = JsonConvert.DeserializeObject<List<GioHangRequest>>(Request.Cookies["Cart"]);
                    }
                    // laam them
                    int cout = lstGioHang.Sum(c => c.SoLuong);
                    TempData["SoLuong"] = cout.ToString();

                    if (Request.Cookies["Cart"] != null)
                    {
                        var response1 = await _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCart?request=" + Request.Cookies["Cart"]);
                        if (response1.IsSuccessStatusCode)
                        {
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response1.Content.ReadAsStringAsync().Result);


                            // lam end

                            TempData["TrangThai"] = "false";
                            HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllChiTietSanPhamHome?idSanPham=" + idSanPham);
                            var chiTietSanPham = JsonConvert.DeserializeObject<ChiTietSanPhamViewModelHome>(response.Content.ReadAsStringAsync().Result);
                            return View(chiTietSanPham);

                        }
                        else return BadRequest();
                    }
                    else
                    {
                        TempData["TongTien"] = "0";
                        HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllChiTietSanPhamHome?idSanPham=" + idSanPham);
                        var chiTietSanPham = JsonConvert.DeserializeObject<ChiTietSanPhamViewModelHome>(response.Content.ReadAsStringAsync().Result);
                        return View(chiTietSanPham);

                    }
                }
                else
                {
                    var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    if (loginInfor.vaiTro == 1)
                    {
                        var response2 = await _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCartLogin?idNguoiDung=" + loginInfor.Id);
                        if (response2.IsSuccessStatusCode)
                        {
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response2.Content.ReadAsStringAsync().Result);


                            // lam them
                            int cout = temp.GioHangs.Sum(c => c.SoLuong);

                            TempData["SoLuong"] = cout.ToString();
                            // lam end
                            TempData["TrangThai"] = "true";
                            HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllChiTietSanPhamHome?idSanPham=" + idSanPham);
                            var chiTietSanPham = JsonConvert.DeserializeObject<ChiTietSanPhamViewModelHome>(response.Content.ReadAsStringAsync().Result);
                            return View(chiTietSanPham);

                        }
                        else return BadRequest();
                    }
                    else
                    {
                        TempData["SoLuong"] = "0";
                        HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllChiTietSanPhamHome?idSanPham=" + idSanPham);
                        var chiTietSanPham = JsonConvert.DeserializeObject<ChiTietSanPhamViewModelHome>(response.Content.ReadAsStringAsync().Result);
                        return View(chiTietSanPham);

                    }

                }

                // lam end
            }
            catch (Exception)
            {
                return Redirect("https://localhost:5001/");
            }


        }
        [HttpGet]
        public async Task<IActionResult> ProductDetailFromCart(Guid idctsp)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetIDsanPhamByIdCTSP?idctsp=" + idctsp);
                var idsanpham = JsonConvert.DeserializeObject<string>(response.Content.ReadAsStringAsync().Result);
                return RedirectToAction("ProductDetail", "Home", new { idSanPham = idsanpham });
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpGet]
        public JsonResult ShowDanhGiabyIdSP(Guid id, int page, int pageSize)
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + $"DanhGia/getbyIdSp/{id}").Result;
                List<DanhGiaViewModel> lstDanhGiaSanpham = new List<DanhGiaViewModel>();
                if (response.IsSuccessStatusCode)
                {
                    lstDanhGiaSanpham = JsonConvert.DeserializeObject<List<DanhGiaViewModel>>(response.Content.ReadAsStringAsync().Result);
                    var model = lstDanhGiaSanpham.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                    return Json(new
                    {
                        data = model,
                        total = lstDanhGiaSanpham.Count,
                        status = true
                    });
                }
                else return Json(new { status = false });
            }
            catch
            {
                return Json(new { status = false });
            }
        }
        #endregion

        [HttpPost]
        public async Task<ActionResult> TongSoLuong(int? sl)
        {
            try
            {
                var session = HttpContext.Session.GetString("LoginInfor");
                if (String.IsNullOrEmpty(session))
                {
                    List<GioHangRequest> lstGioHang = new List<GioHangRequest>();
                    if (Request.Cookies["Cart"] != null)
                    {
                        lstGioHang = JsonConvert.DeserializeObject<List<GioHangRequest>>(Request.Cookies["Cart"]);

                    }
                    // laam them
                    int cout = lstGioHang.Sum(c => c.SoLuong) + sl.Value;
                    TempData["SoLuong"] = cout.ToString();

                    if (Request.Cookies["Cart"] != null)
                    {
                        var response = await _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCart?request=" + Request.Cookies["Cart"]);
                        if (response.IsSuccessStatusCode)
                        {
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response.Content.ReadAsStringAsync().Result);
                            cout = temp.GioHangs.Sum(c => c.SoLuong) + sl.Value;
                            TempData["SoLuong"] = cout.ToString();
                            // lam end

                            return Json(new { success = true, message = "Add to cart successfully", sl = cout });
                        }
                        else return Json(new { error = true, message = "  Not Add to cart " });
                    }
                    return Json(new { success = true, message = "Add to cart successfully", sl = cout });
                }
                else
                {
                    var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    if (loginInfor.vaiTro == 1)
                    {
                        var response = await _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCartLogin?idNguoiDung=" + loginInfor.Id);
                        if (response.IsSuccessStatusCode)
                        {
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response.Content.ReadAsStringAsync().Result);
                            if (temp.GioHangs.Sum(x => x.SoLuong) == 0)
                            {
                                TempData["SoLuong"] = sl.Value.ToString();
                                return Json(new { success = true, message = "Add to cart successfully", sl = TempData["SoLuong"] });
                            }

                            // lam them
                            int cout = temp.GioHangs.Sum(c => c.SoLuong) + sl.Value;

                            TempData["SoLuong"] = cout.ToString();
                            // lam end
                            TempData["TrangThai"] = "true";
                            return Json(new { success = true, message = "Add to cart successfully", sl = cout });
                        }
                        else return BadRequest();
                    }
                    else
                    {
                        TempData["SoLuong"] = "0";
                        return Json(new { error = false, message = "  Not Add to cart " });
                    }
                }
            }
            catch
            {
                return Json(new { error = false, message = "  Not Add to cart " });
            }
        }

        #region Cart
        [HttpGet]
        public async Task<IActionResult> ShoppingCart()
        {
            try
            {
                var session = HttpContext.Session.GetString("LoginInfor");
                //Không đăng nhập
                if (String.IsNullOrEmpty(session))
                {
                    List<GioHangRequest> lstGioHang = new List<GioHangRequest>();
                    if (Request.Cookies["Cart"] != null)
                    {
                        lstGioHang = JsonConvert.DeserializeObject<List<GioHangRequest>>(Request.Cookies["Cart"]);
                    }
                    // laam them
                    int cout = lstGioHang.Sum(c => c.SoLuong);
                    TempData["SoLuong"] = cout.ToString();


                    if (Request.Cookies["Cart"] != null)
                    {
                        var response = await _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCart?request=" + Request.Cookies["Cart"]);
                        if (response.IsSuccessStatusCode)
                        {
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response.Content.ReadAsStringAsync().Result);
                            TempData["TongTien"] = temp.TongTien.ToString();

                            // lam end
                            TempData["ListBienThe"] = JsonConvert.SerializeObject(temp.GioHangs);
                            TempData["TrangThai"] = "false";
                            TempData["Quantity"] = temp.GioHangs.Sum(x => x.SoLuong).ToString();
                            return View(temp.GioHangs);
                        }
                        else return BadRequest();
                    }
                    else
                    {
                        TempData["TongTien"] = "0";
                        return View(new List<GioHangRequest>());
                    }
                }//Có đăng nhập
                else
                {
                    var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    if (loginInfor.vaiTro == 1)
                    {
                        var response = await _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCartLogin?idNguoiDung=" + loginInfor.Id);
                        if (response.IsSuccessStatusCode)
                        {
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response.Content.ReadAsStringAsync().Result);
                            TempData["TongTien"] = temp.TongTien.ToString();
                            TempData["ListBienThe"] = JsonConvert.SerializeObject(temp.GioHangs);
                            // lam them
                            int cout = temp.GioHangs.Sum(c => c.SoLuong);

                            TempData["SoLuong"] = cout.ToString();
                            // lam end
                            TempData["TrangThai"] = "true";
                            TempData["Quantity"] = temp.GioHangs.Sum(x => x.SoLuong).ToString();
                            return View(temp.GioHangs);
                        }
                        else return BadRequest();
                    }
                    else
                    {
                        TempData["SoLuong"] = "0";
                        return View(new List<GioHangRequest>());
                    }
                }
            }
            catch
            {
                return View(new List<GioHangRequest>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> CheckCart()
        {
            try
            {
                var session = HttpContext.Session.GetString("LoginInfor");
                if (String.IsNullOrEmpty(session))
                {
                    if (Request.Cookies["Cart"] != null)
                    {
                        var response = await _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCart?request=" + Request.Cookies["Cart"]);
                        if (response.IsSuccessStatusCode)
                        {
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response.Content.ReadAsStringAsync().Result);

                            return Json(temp.GioHangs);
                        }
                        else return BadRequest();
                    }
                    else
                    {
                        return View(new List<GioHangRequest>());
                    }
                }
                else
                {
                    var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    if (loginInfor.vaiTro == 1)
                    {
                        var response = await _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCartLogin?idNguoiDung=" + loginInfor.Id);
                        if (response.IsSuccessStatusCode)
                        {
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response.Content.ReadAsStringAsync().Result);
                            return Json(temp.GioHangs);
                        }
                        else return BadRequest();
                    }
                    else
                    {
                        return View(new List<GioHangRequest>());
                    }
                }
            }
            catch
            {
                return View(new List<GioHangRequest>());
            }
        }
        [HttpGet]
        public ActionResult DeleteFromCart(Guid id)
        {
            try
            {
                List<GioHangRequest> bienThes = new List<GioHangRequest>();
                string? result = Request.Cookies["Cart"];
                if (result != null)
                {
                    bienThes = JsonConvert.DeserializeObject<List<GioHangRequest>>(result);
                }
                bienThes.Remove(bienThes.Find(p => p.IDChiTietSanPham == id));
                //
                var session = HttpContext.Session.GetString("LoginInfor");
                if (session == null)
                {
                    CookieOptions cookie = new CookieOptions();
                    cookie.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Append("Cart", JsonConvert.SerializeObject(bienThes), cookie);
                    return RedirectToAction("ShoppingCart");
                }
                else
                {
                    var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    if (loginInfor.vaiTro == 1)
                    {
                        var response = _httpClient.DeleteAsync(_httpClient.BaseAddress + $"GioHang/Deletebyid?idctsp={id}&idNguoiDung={loginInfor.Id}").Result;
                        if (response.IsSuccessStatusCode) return RedirectToAction("ShoppingCart");
                        else return RedirectToAction("ShoppingCart");
                    }
                    else return RedirectToAction("ShoppingCart");
                }
            }
            catch
            {
                return RedirectToAction("ShoppingCart");
            }
        }
        [HttpPost]
        public ActionResult AddToCart(string id, int? sl)
        {
            try
            {
                List<GioHangRequest> lstGioHang;
                string? result = Request.Cookies["Cart"];
                var session = HttpContext.Session.GetString("LoginInfor");
                if (session == null)
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        //chiTietSanPham.SoLuong = (sl != null)?sl.Value:1;
                        lstGioHang = new List<GioHangRequest>() { new GioHangRequest() { IDChiTietSanPham = new Guid(id), SoLuong = (sl != null) ? sl.Value : 1 } };
                    }
                    else
                    {
                        lstGioHang = JsonConvert.DeserializeObject<List<GioHangRequest>>(result);
                        var tempBienThe = lstGioHang.FirstOrDefault(x => x.IDChiTietSanPham == new Guid(id));
                        if (tempBienThe != null)
                        {
                            //Sua 
                            if (sl == null)
                            {
                                tempBienThe.SoLuong++;
                            }
                            else
                            {
                                tempBienThe.SoLuong = tempBienThe.SoLuong + sl.Value;
                            }

                        }
                        else
                        {
                            lstGioHang.Add(new GioHangRequest() { IDChiTietSanPham = new Guid(id), SoLuong = (sl != null) ? sl.Value : 1 });
                        }
                    }
                    CookieOptions cookie = new CookieOptions();
                    cookie.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Append("Cart", JsonConvert.SerializeObject(lstGioHang), cookie);
                    return Json(new { success = true, message = "Thêm vào giỏ hàng thành công" });
                }
                else
                {
                    var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    if (loginInfor.vaiTro == 1)
                    {
                        var chiTietGioHang = new ChiTietGioHang() { ID = Guid.NewGuid(), SoLuong = (sl != null) ? sl.Value : 1, IDCTSP = new Guid(id), IDKhachHang = loginInfor.Id };
                        var response = _httpClient.PostAsJsonAsync(_httpClient.BaseAddress + "GioHang/AddCart", chiTietGioHang).Result;

                        if (response.IsSuccessStatusCode) return Json(new { success = true, message = "Thêm vào giỏ hàng thành công" });
                        else return Json(new { success = false, message = "Thêm vào giỏ hàng thất bại" });
                    }
                    else return Json(new { success = false, message = "Chỉ khách hàng mới thêm được vào giỏ hàng" });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Thêm vào giỏ hàng thất bại" });
            }
        }
        [HttpPost]
        public IActionResult UpdateCart(List<string> dssl)
        {
            try
            {
                int cout = 0;
                var session = HttpContext.Session.GetString("LoginInfor");
                if (session == null)
                {
                    List<GioHangRequest> chiTietSanPhams;
                    string? result = Request.Cookies["Cart"];
                    chiTietSanPhams = JsonConvert.DeserializeObject<List<GioHangRequest>>(result);
                    foreach (var item in dssl)
                    {
                        Guid id = Guid.Parse(item.Substring(0, 36));
                        int sl = Convert.ToInt32(item.Substring(36, item.Length - 36));
                        foreach (var x in chiTietSanPhams)
                        {
                            if (x.IDChiTietSanPham == id)
                            {
                                x.SoLuong = sl;
                            }
                        }
                    }
                    CookieOptions cookie = new CookieOptions();
                    cookie.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Append("Cart", JsonConvert.SerializeObject(chiTietSanPhams), cookie);
                    var response = _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCart?request=" + JsonConvert.SerializeObject(chiTietSanPhams)).Result;
                    var temp = JsonConvert.DeserializeObject<GioHangViewModel>(response.Content.ReadAsStringAsync().Result);
                    TempData["TongTien"] = temp.TongTien.ToString();
                    cout = temp.GioHangs.Sum(x => x.SoLuong);
                    TempData["SoLuong"] = cout.ToString();
                    TempData["ListBienThe"] = JsonConvert.SerializeObject(temp.GioHangs);
                    return Json(new { success = true, message = "Cập nhật giỏ hàng thành công", data = temp.GioHangs, cout });
                }
                else
                {
                    var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    if (loginInfor.vaiTro == 1)
                    {
                        foreach (var item in dssl)
                        {
                            Guid id = Guid.Parse(item.Substring(0, 36));
                            int sl = Convert.ToInt32(item.Substring(36, item.Length - 36));
                            var response = _httpClient.PutAsync(_httpClient.BaseAddress + $"GioHang/UpdateCart?idctsp={id}&soluong={sl}&idNguoiDung={loginInfor.Id}", null).Result;
                            var responses = _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCartLogin?idNguoiDung=" + loginInfor.Id).Result;
                            var temp = JsonConvert.DeserializeObject<GioHangViewModel>(responses.Content.ReadAsStringAsync().Result);
                            TempData["TongTien"] = temp.TongTien.ToString();
                            cout = temp.GioHangs.Sum(x => x.SoLuong);
                            TempData["SoLuong"] = cout.ToString();
                        }
                        var responses1 = _httpClient.GetAsync(_httpClient.BaseAddress + "GioHang/GetCartLogin?idNguoiDung=" + loginInfor.Id).Result;
                        var temp1 = JsonConvert.DeserializeObject<GioHangViewModel>(responses1.Content.ReadAsStringAsync().Result);
                        TempData["ListBienThe"] = JsonConvert.SerializeObject(temp1.GioHangs);
                        return Json(new { success = true, message = "Cập nhật giỏ hàng thành công", data = temp1.GioHangs, cout });
                    }
                    else return Json(new { success = false, message = "Chỉ khách hàng mới thêm được vào giỏ hàng" });
                }

            }
            catch (Exception)
            {

                return Json(new { success = true, message = "Cập nhật giỏ hàng thất bại" });
            }

        }
        [HttpPost]
        public ActionResult BuyNow(string id, int soLuong)
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + $"SanPham/GetChiTietSanPhamByID?id=" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    //moiw
                    List<GioHangRequest> lstGioHang;
                    //ChiTietSanPhamViewModel chiTietSanPham = JsonConvert.DeserializeObject<ChiTietSanPhamViewModel>(response.Content.ReadAsStringAsync().Result);
                    string? result = Request.Cookies["Cart"];
                    if (string.IsNullOrEmpty(result))
                    {
                        //chiTietSanPham.SoLuong = (sl != null)?sl.Value:1;
                        lstGioHang = new List<GioHangRequest>() { new GioHangRequest() { IDChiTietSanPham = new Guid(id), SoLuong = (soLuong != null) ? soLuong : 1 } };
                    }
                    else
                    {
                        lstGioHang = JsonConvert.DeserializeObject<List<GioHangRequest>>(result);
                        var tempBienThe = lstGioHang.FirstOrDefault(x => x.IDChiTietSanPham == new Guid(id));
                        if (tempBienThe != null)
                        {
                            //Sua 
                            if (soLuong == null)
                            {
                                tempBienThe.SoLuong++;
                            }
                            else
                            {
                                tempBienThe.SoLuong = tempBienThe.SoLuong + soLuong;
                            }

                        }
                        else
                        {
                            //chiTietSanPham.SoLuong = (sl != null) ? sl.Value : 1;
                            lstGioHang.Add(new GioHangRequest() { IDChiTietSanPham = new Guid(id), SoLuong = (soLuong != null) ? soLuong : 1 });
                        }
                    }
                    var session = HttpContext.Session.GetString("LoginInfor");
                    if (session == null)
                    {
                        CookieOptions cookie = new CookieOptions();
                        cookie.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Append("Cart", JsonConvert.SerializeObject(lstGioHang), cookie);
                        return Json(new { success = true, message = "Add to cart successfully" });
                    }
                    else
                    {
                        var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                        if (loginInfor.vaiTro == 1)
                        {
                            var chiTietGioHang = new ChiTietGioHang() { ID = Guid.NewGuid(), SoLuong = (soLuong != null) ? soLuong : 1, IDCTSP = new Guid(id), IDKhachHang = loginInfor.Id };
                            var response1 = _httpClient.PostAsJsonAsync(_httpClient.BaseAddress + "GioHang/AddCart", chiTietGioHang).Result;
                            if (response1.IsSuccessStatusCode) return Json(new { success = true, message = "Thêm vào giỏ hàng thành công" });
                            else return Json(new { success = false, message = "Thêm vào giỏ hàng thất bại" });
                        }
                        else return Json(new { success = false, message = "Chỉ khách hàng mới thêm được vào giỏ hàng" });
                    }

                }
                else return Json(new { success = false, message = "Mua hàng thất bại" });
            }
            catch
            {
                return Json(new { success = false, message = "Mua hàng thất bại" });
            }
        }
        #endregion

        #region Login
        [HttpGet]
        public IActionResult Login(string actionName)
        {
            try
            {
                TempData["ActionName"] = actionName;
                return View();
            }
            catch
            {
                TempData["ActionName"] = "Index";
                return View();
            }
        }




        [HttpPost]
        public async Task<IActionResult> Login(string lg, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lg) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin đăng nhập.";
                    return View();
                }

                // Encode @ thành %40 nếu cần (dùng Uri.EscapeDataString luôn an toàn hơn)
                string encodedLg = Uri.EscapeDataString(lg);
                string encodedPw = Uri.EscapeDataString(password);

                // Gọi API login
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}QuanLyNguoiDung/DangNhap?lg={encodedLg}&password={encodedPw}");

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<LoginViewModel>(result);

                    HttpContext.Session.SetString("LoginInfor", result);

                    if (user.vaiTro == 1) // Khách hàng
                    {
                        string actionName = TempData["ActionName"] as string;

                        // Nếu có actionName lưu trước đó, có thể redirect đúng
                        if (!string.IsNullOrEmpty(actionName))
                        {
                            return RedirectToAction(actionName, "TrangChu");
                        }

                        return RedirectToAction("Index", "TrangChu");
                    }

                    // Nhân viên
                    return RedirectToAction("BanHang", "BanHangTaiQuay");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập vào tài khoản này.";
                }
                else
                {
                    ViewBag.ErrorMessage = "Email hoặc mật khẩu không chính xác.";
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Đã xảy ra lỗi hệ thống: " + ex.Message;
                return View();
            }
        }


        [HttpGet]
        public IActionResult XacThucEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> XacThucEmail(XacThucEmailViewModel model)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                _httpClient.BaseAddress + "QuanLyNguoiDung/XacThucEmail", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login", "Home");
            }

            // Lấy lỗi và hiển thị lại view
            var error = await response.Content.ReadAsStringAsync();
            model.ErrorMessage = error;
            return View(model);
        }







        public IActionResult Register()
        {
            return View();
        }





        [HttpPost]
        public async Task<IActionResult> Register(KhachHangViewModel khachHang)
        {
            try
            {
                khachHang.Id = Guid.NewGuid();
                khachHang.DiemTich = 0;
                khachHang.TrangThai = 1;

                // Gửi POST yêu cầu đăng ký
                var response = await _httpClient.PostAsJsonAsync(_httpClient.BaseAddress + "QuanLyNguoiDung/DangKyKhachHang", khachHang);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tài khoản đã được đăng ký thành công! Vui lòng kiểm tra email để xác minh.";
                    return View("XacThucEmail"); // View này có thể ghi: "Vui lòng kiểm tra email để kích hoạt"
                }

                // Nếu thất bại
                var message = await response.Content.ReadAsStringAsync();
                ViewBag.ErrorMessage = !string.IsNullOrEmpty(message) ? message : "Đăng ký không thành công. Vui lòng thử lại.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Đã xảy ra lỗi hệ thống: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Profile()
        {
            try
            {
                var session = HttpContext.Session.GetString("LoginInfor");
                if (session != null)
                {
                    LoginViewModel loginViewModel = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    //LoginViewModel loginViewModel = JsonConvert.DeserializeObject<LoginViewModel>(loginInfor);
                    var response = _httpClient.GetAsync(_httpClient.BaseAddress + $"KhachHang/GetById?id={loginViewModel.Id}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        loginViewModel.DiemTich = JsonConvert.DeserializeObject<KhachHang>(response.Content.ReadAsStringAsync().Result).DiemTich;
                        Console.WriteLine("Session LoginInfor: " + session);
                        return View(loginViewModel);
                    }
                    else
                    {
                        return View(loginViewModel);
                    }
                }
                return Redirect("https://localhost:5001/");
            }
            catch
            {
                return Redirect("https://localhost:5001/");
            }

        }
        [HttpPut]
        public ActionResult UpdateProfile(string ten, string email, string sdt, int? gioitinh, string? ngaysinh, string? diachi)
        {
            try
            {
                // Validate đầu vào
                if (string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(email))
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });

                if (!Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
                    return Json(new { success = false, message = "Email không hợp lệ." });

                if (!Regex.IsMatch(sdt, @"^0\d{9,10}$"))
                    return Json(new { success = false, message = "Số điện thoại không hợp lệ." });

                // Đọc user từ session
                var session = HttpContext.Session.GetString("LoginInfor");
                if (string.IsNullOrWhiteSpace(session))
                    return Json(new { success = false, message = "Không tìm thấy phiên đăng nhập." });

                var user = JsonConvert.DeserializeObject<LoginViewModel>(session);
                if (user == null)
                    return Json(new { success = false, message = "Không thể đọc dữ liệu người dùng." });

                // Parse ngày sinh
                DateOnly? parsedNgaySinh = null;
                if (!string.IsNullOrWhiteSpace(ngaysinh))
                {
                    if (!DateOnly.TryParse(ngaysinh, out var parsed))
                        return Json(new { success = false, message = "Ngày sinh không hợp lệ. Định dạng đúng: yyyy-MM-dd" });

                    parsedNgaySinh = parsed;
                }

                // Gán lại thông tin
                var khachhang = new LoginViewModel
                {
                    Id = user.Id,
                    Ten = ten,
                    Email = email,
                    SDT = sdt,
                    GioiTinh = gioitinh,
                    NgaySinh = ngaysinh,
                    DiaChi = diachi,
                    DiemTich = user.DiemTich,
                    vaiTro = user.vaiTro,
                    IsAccountLocked = user.IsAccountLocked,
                    Message = "Cập nhật thông tin"
                };

                // Gọi API cập nhật
                var response = _httpClient.PutAsJsonAsync(_httpClient.BaseAddress + "QuanLyNguoiDung/UpdateProfile1", khachhang).Result;

                if (response.IsSuccessStatusCode)
                {
                    // Cập nhật session mới
                    var updatedData = response.Content.ReadAsStringAsync().Result;
                    HttpContext.Session.SetString("LoginInfor", updatedData);

                    return Json(new { success = true, message = "Cập nhật thông tin cá nhân thành công." });
                }

                return Json(new { success = false, message = "Cập nhật thất bại. Vui lòng thử lại sau." });
            }
            catch (Exception ex)
            {
                // Nếu muốn log lỗi cụ thể
                Console.WriteLine("Lỗi cập nhật profile: " + ex.Message);

                return Json(new { success = false, message = "Lỗi hệ thống. Vui lòng thử lại sau." });
            }
        }


        public IActionResult PurchaseOrder()
        {
            try
            {
                var session = HttpContext.Session.GetString("LoginInfor");
                if (session != null)
                {
                    LoginViewModel loginViewModel = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    List<DonMuaViewModel> donMuaViewModels = new List<DonMuaViewModel>();
                    //HttpResponseMessage responseDonMua = _httpClient.GetAsync(_httpClient.BaseAddress + $"LichSuTichDiem/GetAllDonMua?IDkhachHang={loginViewModel.Id}").Result;
                    //if (responseDonMua.IsSuccessStatusCode)
                    //{
                    //    donMuaViewModels = JsonConvert.DeserializeObject<List<DonMuaViewModel>>(responseDonMua.Content.ReadAsStringAsync().Result);
                    //}
                    return View("PurchaseOrder", donMuaViewModels);
                }
                else
                {
                    return Redirect("https://localhost:5001/");
                }
            }
            catch (Exception)
            {
                return Redirect("https://localhost:5001/");
            }
        }
        public IActionResult PurchaseOrderDetail(Guid idHoaDon)
        {
            try
            {
                List<DonMuaChiTietViewModel> DonMuaCT = new List<DonMuaChiTietViewModel>();
                HttpResponseMessage responseDonMuaCT = _httpClient.GetAsync(_httpClient.BaseAddress + $"LichSuTichDiem/GetAllDonMuaChiTiet?idHoaDon={idHoaDon}").Result;
                if (responseDonMuaCT.IsSuccessStatusCode)
                {
                    DonMuaCT = JsonConvert.DeserializeObject<List<DonMuaChiTietViewModel>>(responseDonMuaCT.Content.ReadAsStringAsync().Result);
                }
                return View("PurchaseOrderDetail", DonMuaCT);
            }
            catch (Exception)
            {
                return Redirect("https://localhost:5001/");
            }

        }
        public IActionResult LichSuTieuDiemTichDiem()
        {
            return View();
        }
        public IActionResult LichSuTieuDiemTichDiembyuser(HoaDon danhGiaCTHDView, int page, int pageSize)
        {
            try
            {
                var session = HttpContext.Session.GetString("LoginInfor");
                LoginViewModel loginViewModel = JsonConvert.DeserializeObject<LoginViewModel>(session);
                List<LichSuTichDiemTieuDiemViewModel> listLSTD = new List<LichSuTichDiemTieuDiemViewModel>();
                List<LichSuTichDiemTieuDiemViewModel> listLSTDFN = new List<LichSuTichDiemTieuDiemViewModel>();
                HttpResponseMessage responseDonMua = _httpClient.GetAsync(_httpClient.BaseAddress + $"LichSuTichDiem/GetALLLichSuTichDiembyIdUser?IDkhachHang={loginViewModel.Id}").Result;
                if (responseDonMua.IsSuccessStatusCode)
                {
                    listLSTD = JsonConvert.DeserializeObject<List<LichSuTichDiemTieuDiemViewModel>>(responseDonMua.Content.ReadAsStringAsync().Result);
                    listLSTD = listLSTD.OrderBy(p => p.TrangThaiLSTD).ToList();
                    listLSTD = listLSTD.Where(p => p.Diem > 0).ToList();
                    //foreach (var item in listLSTD)
                    //{
                    //    item.Ngaytao1 = item.NgayTao.ToString("dd/MM/yyyy");
                    //    item.Ngaythanhtoan1 = item.NgayThanhToan != null ? item.NgayThanhToan.Value.ToString("dd/MM/yyyy") : null;
                    //    item.Ngaynhanhang1 = item.NgayNhanHang != null ? item.NgayNhanHang.Value.ToString("dd/MM/yyyy") : null;
                    //}
                    if (danhGiaCTHDView.TrangThaiGiaoHang != 2 && danhGiaCTHDView.TrangThaiGiaoHang != null)
                    {
                        if (danhGiaCTHDView.TrangThaiGiaoHang == 0)
                        {
                            List<LichSuTichDiemTieuDiemViewModel> listLSTD1 = new List<LichSuTichDiemTieuDiemViewModel>();
                            listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 0).ToList();
                            foreach (var item in listLSTD1)
                            {
                                listLSTDFN.Add(item);
                            }
                            listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 3 && p.TrangThaiGiaoHang == 5).ToList();
                            foreach (var item in listLSTD1)
                            {
                                listLSTDFN.Add(item);
                            }
                            listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 3 && p.TrangThaiGiaoHang == 4).ToList();
                            foreach (var item in listLSTD1)
                            {
                                listLSTDFN.Add(item);
                            }
                        }
                        else
                        {
                            List<LichSuTichDiemTieuDiemViewModel> listLSTD1 = new List<LichSuTichDiemTieuDiemViewModel>();
                            listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 1 && p.TrangThaiGiaoHang == 6).ToList();
                            foreach (var item in listLSTD1)
                            {
                                listLSTDFN.Add(item);
                            }
                            listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 1 && p.TrangThaiGiaoHang == 9).ToList();
                            foreach (var item in listLSTD1)
                            {
                                listLSTDFN.Add(item);
                            }
                            listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 1 && p.TrangThaiGiaoHang == 4).ToList();
                            foreach (var item in listLSTD1)
                            {
                                listLSTDFN.Add(item);
                            }
                            listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 1 && p.TrangThaiGiaoHang == 5).ToList();
                            foreach (var item in listLSTD1)
                            {
                                listLSTDFN.Add(item);
                            }
                            listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 2 && p.TrangThaiGiaoHang == 7).ToList();
                            foreach (var item in listLSTD1)
                            {
                                listLSTDFN.Add(item);
                            }
                            listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 4 && p.TrangThaiGiaoHang == 5).ToList();
                            foreach (var item in listLSTD1)
                            {
                                listLSTDFN.Add(item);
                            }
                        }
                        listLSTDFN = listLSTDFN.OrderBy(p => p.TrangThaiLSTD).ToList();
                        listLSTDFN = listLSTDFN.OrderBy(p => p.NgayTao).ToList();
                        var model = listLSTDFN.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                        return Json(new
                        {
                            data = model,
                            total = listLSTDFN.Count,
                            status = true
                        });
                    }
                    else
                    {
                        List<LichSuTichDiemTieuDiemViewModel> listLSTD1 = new List<LichSuTichDiemTieuDiemViewModel>();
                        listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 0).ToList();
                        foreach (var item in listLSTD1)
                        {
                            listLSTDFN.Add(item);
                        }
                        listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 3 && p.TrangThaiGiaoHang == 4).ToList();
                        foreach (var item in listLSTD1)
                        {
                            listLSTDFN.Add(item);
                        }
                        listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 3 && p.TrangThaiGiaoHang == 5).ToList();
                        foreach (var item in listLSTD1)
                        {
                            listLSTDFN.Add(item);
                        }
                        listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 1 && p.TrangThaiGiaoHang == 6).ToList();
                        foreach (var item in listLSTD1)
                        {
                            listLSTDFN.Add(item);
                        }
                        listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 1 && p.TrangThaiGiaoHang == 9).ToList();
                        foreach (var item in listLSTD1)
                        {
                            listLSTDFN.Add(item);
                        }
                        listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 1 && p.TrangThaiGiaoHang == 4).ToList();
                        foreach (var item in listLSTD1)
                        {
                            listLSTDFN.Add(item);
                        }
                        listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 1 && p.TrangThaiGiaoHang == 5).ToList();
                        foreach (var item in listLSTD1)
                        {
                            listLSTDFN.Add(item);
                        }
                        listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 2 && p.TrangThaiGiaoHang == 7).ToList();
                        foreach (var item in listLSTD1)
                        {
                            listLSTDFN.Add(item);
                        }
                        listLSTD1 = listLSTD.Where(p => p.TrangThaiLSTD == 4 && p.TrangThaiGiaoHang == 5).ToList();
                        foreach (var item in listLSTD1)
                        {
                            listLSTDFN.Add(item);
                        }
                        listLSTDFN = listLSTDFN.OrderBy(p => p.TrangThaiLSTD).ToList();
                        listLSTDFN = listLSTDFN.OrderBy(p => p.NgayTao).ToList();
                        var model = listLSTDFN.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                        return Json(new
                        {
                            data = model,
                            total = listLSTDFN.Count,
                            status = true
                        });
                    }
                }
                else return Json(new { status = false });

            }
            catch (Exception)
            {
                return Redirect("https://localhost:5001/");
            }

        }
        public IActionResult ReviewProducts(Guid idCTHD)
        {
            try
            {
                ChiTietHoaDonDanhGiaViewModel hdctDanhGia = new ChiTietHoaDonDanhGiaViewModel();
                HttpResponseMessage responseDonMuaCT = _httpClient.GetAsync(_httpClient.BaseAddress + $"LichSuTichDiem/GetCTHDDANHGIA?idhdct={idCTHD}").Result;
                if (responseDonMuaCT.IsSuccessStatusCode)
                {
                    hdctDanhGia = JsonConvert.DeserializeObject<ChiTietHoaDonDanhGiaViewModel>(responseDonMuaCT.Content.ReadAsStringAsync().Result);
                }
                return View("ReviewProducts", hdctDanhGia);
            }
            catch (Exception)
            {
                return Redirect("https://localhost:5001/");
            }

        }
        [HttpPost]
        public IActionResult ChangePassword(string newPassword, string oldPassword)
        {
            try
            {
                var session = HttpContext.Session.GetString("LoginInfor");
                ChangePasswordRequest request = new ChangePasswordRequest();
                request.ID = JsonConvert.DeserializeObject<LoginViewModel>(session).Id;
                request.NewPassword = newPassword;
                request.OldPassword = oldPassword;
                var response = _httpClient.PutAsJsonAsync(_httpClient.BaseAddress + "QuanLyNguoiDung/ChangePassword", request).Result;
                HttpContext.Session.Remove("LoginInfor");
                if (response.IsSuccessStatusCode) return RedirectToAction("Login", new { actionName = "Index" });
                else return BadRequest();
            }
            catch
            {
                return BadRequest();
            }
        }
        public IActionResult ManageDiaChi()
        {
            var session = HttpContext.Session.GetString("LoginInfor");
            if (session == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var model = JsonConvert.DeserializeObject<LoginViewModel>(session);
            return View(model);
        }

        public IActionResult DanhGiaSanPham([FromBody] DanhGiaCTHDViewModel danhGiaCTHDView)
        {
            try
            {
                if (danhGiaCTHDView.danhgia == "")
                {
                    danhGiaCTHDView.danhgia = "Người dùng này không để lại bình luận";
                }
                HttpResponseMessage responseDonMuaCT = _httpClient.PutAsync(_httpClient.BaseAddress + $"DanhGia?idCTHD={danhGiaCTHDView.idCTHD}&soSao={danhGiaCTHDView.soSao}&binhLuan={danhGiaCTHDView.danhgia}", null).Result;
                if (responseDonMuaCT.IsSuccessStatusCode)
                {
                    RedirectToAction("PurchaseOrderDetail", "Home", new { idHoaDon = danhGiaCTHDView.idHD });
                }
                return RedirectToAction("PurchaseOrderDetail", "Home", new { idHoaDon = danhGiaCTHDView.idHD });
            }
            catch (Exception)
            {
                return Redirect("https://localhost:5001/");
            }

        }
        public IActionResult HuyDonHang(Guid idHoaDon)
        {
            try
            {
                HttpResponseMessage responseDonMua = _httpClient.PutAsync(_httpClient.BaseAddress + $"HoaDon?idhoadon={idHoaDon}&trangthai=8", null).Result;
                if (responseDonMua.IsSuccessStatusCode)
                {
                    RedirectToAction("PurchaseOrder");
                }
                return RedirectToAction("PurchaseOrder");
            }
            catch (Exception)
            {
                return RedirectToAction("PurchaseOrder");
            }

        }
        public IActionResult HoanTacHuyDonHang(Guid idHoaDon)
        {
            try
            {
                HttpResponseMessage responseDonMua = _httpClient.PutAsync(_httpClient.BaseAddress + $"HoaDon?idhoadon={idHoaDon}&trangthai=2", null).Result;
                if (responseDonMua.IsSuccessStatusCode)
                {
                    RedirectToAction("PurchaseOrder");
                }
                return RedirectToAction("PurchaseOrder");
            }
            catch (Exception)
            {
                return RedirectToAction("PurchaseOrder");
            }

        }
        public IActionResult DoiTraHang(Guid idHoaDon)
        {
            try
            {
                HttpResponseMessage responseDonMua = _httpClient.PutAsync(_httpClient.BaseAddress + $"HoaDon?idhoadon={idHoaDon}&trangthai=9", null).Result;
                if (responseDonMua.IsSuccessStatusCode)
                {
                    RedirectToAction("PurchaseOrder");
                }
                return RedirectToAction("PurchaseOrder");
            }
            catch (Exception)
            {
                return RedirectToAction("PurchaseOrder");
            }

        }
        public IActionResult HoanTacDoiTraHang(Guid idHoaDon)
        {
            try
            {
                HttpResponseMessage responseDonMua = _httpClient.PutAsync(_httpClient.BaseAddress + $"HoaDon?idhoadon={idHoaDon}&trangthai=6", null).Result;
                if (responseDonMua.IsSuccessStatusCode)
                {
                    RedirectToAction("PurchaseOrder");
                }
                return RedirectToAction("PurchaseOrder");

            }
            catch (Exception)
            {
                return RedirectToAction("PurchaseOrder");
            }

        }
        public IActionResult XacNhanGHTC(Guid idHoaDon)
        {
            try
            {
                HttpResponseMessage responseDonMua = _httpClient.PutAsync(_httpClient.BaseAddress + $"HoaDon?idhoadon={idHoaDon}&trangthai=6", null).Result;
                if (responseDonMua.IsSuccessStatusCode)
                {
                    RedirectToAction("PurchaseOrderDetail", idHoaDon);
                }
                return RedirectToAction("PurchaseOrder");
            }
            catch
            {
                return RedirectToAction("PurchaseOrder");
            }
        }
        public IActionResult GetHoaDonByTrangThai(HoaDon danhGiaCTHDView, int page, int pageSize, string Search)
        {
            try
            {
                var session = HttpContext.Session.GetString("LoginInfor");
                LoginViewModel loginViewModel = JsonConvert.DeserializeObject<LoginViewModel>(session);
                List<DonMuaViewModel> donMuaViewModels = new List<DonMuaViewModel>();
                HttpResponseMessage responseDonMua = _httpClient.GetAsync(_httpClient.BaseAddress + $"LichSuTichDiem/GetAllDonMua?IDkhachHang={loginViewModel.Id}").Result;
                if (responseDonMua.IsSuccessStatusCode)
                {
                    donMuaViewModels = JsonConvert.DeserializeObject<List<DonMuaViewModel>>(responseDonMua.Content.ReadAsStringAsync().Result);
                    donMuaViewModels = donMuaViewModels.OrderByDescending(p => p.NgayTao).ToList();

                    foreach (var item in donMuaViewModels)
                    {
                        item.Ngaytao1 = item.NgayTao.ToString("dd/MM/yyyy");
                        item.Ngaythanhtoan1 = item.NgayThanhToan != null ? item.NgayThanhToan.Value.ToString("dd/MM/yyyy") : null;
                        item.Ngaynhanhang1 = item.NgayNhanHang != null ? item.NgayNhanHang.Value.ToString("dd/MM/yyyy") : null;
                    }
                    if (Search != null)
                    {
                        donMuaViewModels = donMuaViewModels.Where(p => p.MaHD.ToLower().Contains(Search.ToLower())).ToList();
                    }
                    if (danhGiaCTHDView.TrangThaiGiaoHang != 0 && danhGiaCTHDView.TrangThaiGiaoHang != null)
                    {
                        if (danhGiaCTHDView.TrangThaiGiaoHang == 5)
                        {
                            donMuaViewModels = donMuaViewModels.Where(p => p.TrangThaiGiaoHang == 4 || p.TrangThaiGiaoHang == 5 || p.TrangThaiGiaoHang == 9).ToList();
                        }
                        else if (danhGiaCTHDView.TrangThaiGiaoHang == 2)
                        {
                            donMuaViewModels = donMuaViewModels.Where(p => p.TrangThaiGiaoHang == 2 || p.TrangThaiGiaoHang == 8).ToList();
                        }
                        else
                        {
                            donMuaViewModels = donMuaViewModels.Where(p => p.TrangThaiGiaoHang == danhGiaCTHDView.TrangThaiGiaoHang).ToList();
                        }
                        var model = donMuaViewModels.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                        return Json(new
                        {
                            data = model,
                            total = donMuaViewModels.Count,
                            status = true
                        });
                    }
                    else
                    {
                        var model = donMuaViewModels.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                        return Json(new
                        {
                            data = model,
                            total = donMuaViewModels.Count,
                            status = true
                        });
                    }
                }
                else return Json(new { status = false });
            }
            catch
            {
                return Json(new { status = false });
            }
        }
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        [HttpGet]

        #endregion

        #region ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            HttpContext.Session.Remove("LoginInfor");
            return View();
        }
        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            try
            {
                var khachHang = JsonConvert.DeserializeObject<KhachHangViewModel>(_httpClient.GetAsync(_httpClient.BaseAddress + "KhachHang/GetKhachHangByEmail?email=" + email).Result.Content.ReadAsStringAsync().Result);
                if (khachHang.Id != null)
                {
                    string ma = Guid.NewGuid().ToString().Substring(0, 5);
                    MailData mailData = new MailData() { EmailToId = email, EmailToName = email, EmailSubject = "Mã xác nhận", EmailBody = "Mã xác nhận: " + ma };
                    HttpResponseMessage response = _httpClient.PostAsJsonAsync(_httpClient.BaseAddress + "Mail/SendMail", mailData).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        HttpContext.Session.SetString("ForgotPassword", khachHang.Id + ":" + ma);
                        return RedirectToAction("SubmitForgotPassword");
                    }
                    else return BadRequest();
                }
                else return BadRequest("Không tìm thấy email");
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpGet]
        public IActionResult SubmitForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SubmitForgotPassword(string code)
        {
            try
            {
                string[] submit = HttpContext.Session.GetString("ForgotPassword").Split(":");
                if (code == submit[1])
                {
                    return RedirectToAction("ChangeForgotPassword");
                }
                else return BadRequest("Mã không hợp lệ");
            }
            catch
            {
                return BadRequest("Mã không hợp lệ");
            }
        }
        [HttpGet]
        public IActionResult ChangeForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ChangeForgotPassword(string password, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    ViewData["PasswordError"] = "Mật khẩu không được bỏ trống";
                    return View();
                }
                else if (password.Length < 8)
                {
                    ViewData["PasswordError"] = "Mật khẩu phải lớn hơn 8 ký tự";
                    return View();
                }



                if (password == confirmPassword)
                {
                    string[] submit = HttpContext.Session.GetString("ForgotPassword").Split(":");
                    KhachHangViewModel khachHang = new KhachHangViewModel() { Id = new Guid(submit[0]), Password = password };
                    HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "KhachHang/ChangeForgotPassword?id=" + khachHang.Id + "&password=" + khachHang.Password).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Login", new { actionName = "Index" });
                    }
                    else
                    {
                        ViewData["ErrorMessage"] = "Không thể đặt lại mật khẩu. Vui lòng kiểm tra lại thông tin.";
                        return View();
                    }
                }
                else
                {
                    ViewData["ErrorMessage"] = "Không thể đặt lại mật khẩu. Vui lòng kiểm tra lại thông tin.";
                    return View();
                }
            }
            catch
            {
                ViewData["ConfirmPasswordError"] = "Error";
                return View();
            }
        }


        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            try
            {
                // Kiểm tra email và chuyển đến trang ResetPassword nếu email hợp lệ
                if (!string.IsNullOrEmpty(email))
                {
                    ResetPasswordRequest request = new ResetPasswordRequest
                    {
                        Email = email
                    };
                    return View(request);
                }
                else
                {
                    // Xử lý khi email không hợp lệ
                    TempData["Message"] = "Invalid email.";
                    return RedirectToAction("Login", new { actionNam = "Index" });
                }
            }
            catch
            {
                TempData["Message"] = "Invalid email.";
                return RedirectToAction("Login", new { actionNam = "Index" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validate password and confirm password match
                    if (request.Password != request.ConfirmPassword)
                    {
                        ModelState.AddModelError("", "Passwords do not match");
                        return View("ResetPassword");
                    }

                    // Implement the code to save the new password in your QuanLyNguoiDungService
                    string apiUrl = "https://localhost:7095/api/QuanLyNguoiDung/ResetPassword";
                    var response = await _httpClient.PostAsync(apiUrl, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Password reset successfully";
                        return RedirectToAction("Login", new { actionNam = "Index" });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể đặt lại mật khẩu");
                    }
                }

                return View("ResetPassword");
            }
            catch
            {
                return View("ResetPassword");
            }
        }

        #endregion

        #region CheckOut
        [HttpGet]
        public JsonResult UseDiemTich(int diem, string id, int tongTien)
        {
            try
            {
                var response = _httpClient.GetAsync(_httpClient.BaseAddress + $"QuanLyNguoiDung/UseDiemTich?diem={diem}&id={id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    var soTienGiam = Convert.ToInt32(response.Content.ReadAsStringAsync().Result);
                    if (soTienGiam <= (tongTien / 5))
                    {
                        return Json(new { SoTienGiam = soTienGiam, TrangThai = true });
                    }
                    else
                    {
                        return Json(new { Loi = "Số tiền giảm khi sử dụng điểm phải nhỏ hơn 20% giá trị đơn hàng", TrangThai = false });
                    }
                }
                else return Json(new { Loi = "Không kết nối được với server", TrangThai = false });
            }
            catch
            {
                return Json(new { Loi = "Không kết nối được với server", TrangThai = false });
            }
        }

        [HttpGet]
        public IActionResult CheckOut()
        {
            return View();
        }
        [HttpPost]
        public async Task<string> Order([FromBody] HoaDonViewModel hoaDon)
        {
            string check = string.Empty;
            try
            {
                List<ChiTietHoaDonViewModel> lstChiTietHoaDon = new List<ChiTietHoaDonViewModel>();
                string temp = TempData.Peek("ListBienThe") as string;
                string trangThai = TempData.Peek("TrangThai") as string;
                foreach (var item in JsonConvert.DeserializeObject<List<GioHangRequest>>(temp))
                {
                    ChiTietHoaDonViewModel chiTietHoaDon = new ChiTietHoaDonViewModel();
                    chiTietHoaDon.IDChiTietSanPham = item.IDChiTietSanPham;
                    chiTietHoaDon.SoLuong = item.SoLuong;
                    chiTietHoaDon.DonGia = item.DonGia.Value;
                    lstChiTietHoaDon.Add(chiTietHoaDon);
                }
                hoaDon.ChiTietHoaDons = lstChiTietHoaDon;
                hoaDon.TrangThai = Convert.ToBoolean(trangThai);
                var session = HttpContext.Session.GetString("LoginInfor");
                if (session != null)
                {
                    hoaDon.IDKhachHang = JsonConvert.DeserializeObject<LoginViewModel>(session).Id;
                }
                TempData.Remove("TongTien");
                TempData.Remove("Quantity");
                // 3. Gán ID phương thức thanh toán
                hoaDon.IDPhuongThucTT = hoaDon.PhuongThucThanhToan switch
                {
                    "COD" => Guid.Parse("DD56B0D5-721D-4CD3-A20A-CEB190755E26"),
                    "VNPay" => Guid.Parse("761881CC-2324-4760-9628-6ED287A59AC7"),
                    _ => Guid.Empty
                };

                // 4. Xử lý COD
                if (hoaDon.PhuongThucThanhToan == "COD")
                {
                    var response = await _httpClient.PostAsJsonAsync("HoaDon", hoaDon);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["CheckOutSuccess"] = await response.Content.ReadAsStringAsync();

                        if (!hoaDon.TrangThai) Response.Cookies.Delete("Cart");
                        TempData["SoLuong"] = "0";

                        TempData.Remove("TongTien");
                        TempData.Remove("Quantity");

                        return "https://localhost:5001/Home/CheckOutSuccess";
                    }

                    return "";
                }

                // 5. Xử lý VNPay
                if (hoaDon.PhuongThucThanhToan == "VNPay")
                {
                    TempData["HoaDon"] = JsonConvert.SerializeObject(hoaDon);

                    string returnUrl = "https://localhost:5001/Home/PaymentCallBack";
                    string url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
                    string tmnCode = "NJJ0R8FS";
                    string hashSecret = "BYKJBHPPZKQMKBIBGGXIYKWYFAYSJXCW";
                    string ip = HttpContext.Connection.RemoteIpAddress?.ToString();

                    var order = new OrderInfo
                    {
                        OrderId = DateTime.Now.Ticks,
                        Amount = hoaDon.TongTien,
                        CreatedDate = DateTime.Now,
                        OrderDesc = $"Thanh toán đơn hàng #{DateTime.Now.Ticks}",
                        BankCode = "VNPAYQR"
                    };

                    var vnpay = new VnPayLibrary();
                    vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                    vnpay.AddRequestData("vnp_Command", "pay");
                    vnpay.AddRequestData("vnp_TmnCode", tmnCode);
                    vnpay.AddRequestData("vnp_Amount", (order.Amount * 100).ToString());
                    vnpay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
                    vnpay.AddRequestData("vnp_CurrCode", "VND");
                    vnpay.AddRequestData("vnp_IpAddr", ip);
                    vnpay.AddRequestData("vnp_Locale", "vn");
                    vnpay.AddRequestData("vnp_OrderInfo", order.OrderDesc);
                    vnpay.AddRequestData("vnp_OrderType", "other");
                    vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
                    vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString());
                    vnpay.AddRequestData("vnp_ExpireDate", order.CreatedDate.AddMinutes(15).ToString("yyyyMMddHHmmss"));
                    vnpay.AddRequestData("vnp_BankCode", "");

                    return check = vnpay.CreateRequestUrl(url, hashSecret);
                }

                return check;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Order Error: " + ex.Message);
                return "";
            }
        }


        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            try
            {
                if (Request.Query.Count > 0)
                {
                    string vnp_HashSecret = "BYKJBHPPZKQMKBIBGGXIYKWYFAYSJXCW"; //Chuoi bi mat
                    var vnpayData = Request.Query;
                    VnPayLibrary vnpay = new VnPayLibrary();

                    foreach (var s in vnpayData)
                    {
                        //get all querystring data
                        if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                        {
                            vnpay.AddResponseData(s.Key, vnpayData[s.Key]);
                        }
                    }
                    //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                    //vnp_TransactionNo: Ma GD tai he thong VNPAY
                    //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                    //vnp_SecureHash: HmacSHA512 cua du lieu tra ve

                    long orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
                    long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                    string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                    string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                    String vnp_SecureHash = Request.Query["vnp_SecureHash"];
                    String TerminalID = Request.Query["vnp_TmnCode"];
                    long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                    String bankCode = Request.Query["vnp_BankCode"];

                    bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                    if (checkSignature)
                    {
                        if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                        {
                            //Thanh toan thanh cong
                            TempData.Remove("TongTien");
                            TempData.Remove("Quantity");
                            var hoaDon = JsonConvert.DeserializeObject<HoaDonViewModel>(TempData["HoaDon"].ToString());
                            hoaDon.NgayThanhToan = DateTime.Now;
                            hoaDon.TrangThaiGiaoHang = 3;
                            HttpResponseMessage response = _httpClient.PostAsJsonAsync("HoaDon", hoaDon).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                TempData["CheckOutSuccess"] = response.Content.ReadAsStringAsync().Result;
                                if (!hoaDon.TrangThai) Response.Cookies.Delete("Cart");
                                // lam them
                                TempData["SoLuong"] = "0";
                                // lam end
                                return RedirectToAction("CheckOutSuccess");
                            }
                            else return BadRequest();
                        }
                        else
                        {
                            //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                            return BadRequest($"Lỗi thanh toán: Mã VNPay = {vnp_ResponseCode}, Trạng thái = {vnp_TransactionStatus}");

                        }
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else return BadRequest();
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpGet]
        public IActionResult CheckOutSuccess()
        {
            var tempDataJson = TempData.Peek("CheckOutSuccess") as string;
            if (string.IsNullOrEmpty(tempDataJson))
            {
                return View(new DonMuaSuccessViewModel()); // Không có dữ liệu => hiển thị View rỗng
            }

            DonMuaSuccessViewModel donMua;
            try
            {
                donMua = JsonConvert.DeserializeObject<DonMuaSuccessViewModel>(tempDataJson);
            }
            catch
            {
                return View(new DonMuaSuccessViewModel()); // Lỗi khi parse dữ liệu
            }

            var temp = HttpContext.Session.GetString("LoginInfor");
            if (!string.IsNullOrEmpty(temp))
            {
                var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(temp);
                if (loginInfor != null)
                {
                    loginInfor.DiemTich -= donMua.DiemSuDung;
                    if (loginInfor.DiemTich < 0) loginInfor.DiemTich = 0;
                    HttpContext.Session.SetString("LoginInfor", JsonConvert.SerializeObject(loginInfor));
                }
            }

            return View(donMua);
        }


        [HttpGet]
        public JsonResult GetAllVoucherByTien(int tongTien)
        {
            try
            {
                var response = _httpClient.GetAsync("Voucher/GetAllVoucherByTien?tongTien=" + tongTien).Result;
                if (response.IsSuccessStatusCode)
                {
                    var lst = JsonConvert.DeserializeObject<List<Voucher>>(response.Content.ReadAsStringAsync().Result);
                    return Json(new { TrangThai = true, KetQua = lst });
                }
                else return Json(new { TrangThai = false });
            }
            catch
            {
                return Json(new { TrangThai = false });
            }
        }
        #endregion

        #region Other
        public IActionResult BlogDetails()
        {
            return View();
        }
        public IActionResult Contacts()
        {
            return View();
        }
        public IActionResult Blog()
        {
            return View();
        }
        #endregion

        public IActionResult HoSo()
        {
            var session = HttpContext.Session.GetString("LoginInfor");
            if (string.IsNullOrEmpty(session)) return RedirectToAction("Login", "Home");

            var user = JsonConvert.DeserializeObject<LoginViewModel>(session);
            return View(user); // View: Views/Home/HoSo.cshtml
        }


        // GET: Danh sách địa chỉ của khách hàng
        [HttpGet]
        public async Task<IActionResult> Indexx()
        {
            var session = HttpContext.Session.GetString("LoginInfor");
            if (string.IsNullOrEmpty(session)) return RedirectToAction("Login", "Home");

            var khachHang = JsonConvert.DeserializeObject<LoginViewModel>(session);
            var response = await _httpClient.GetAsync($"khachhang/{khachHang.Id}/diachi");

            if (!response.IsSuccessStatusCode) return View(new List<DiaChiDTO>());

            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<DiaChiDTO>>(json);
            return View(list);
        }

        // POST: Thêm địa chỉ
        [HttpGet]
        public IActionResult Add()
        {
            var session = HttpContext.Session.GetString("LoginInfor");
            if (string.IsNullOrEmpty(session))
                return RedirectToAction("Login", "Home");

            var khachHang = JsonConvert.DeserializeObject<LoginViewModel>(session);

            return View(khachHang);
        }

        [HttpPost]
        public async Task<IActionResult> Add(DiaChiDTO diaChi)
        {
            var session = HttpContext.Session.GetString("LoginInfor");
            if (string.IsNullOrEmpty(session)) return RedirectToAction("Login", "Home");

            var khachHang = JsonConvert.DeserializeObject<LoginViewModel>(session);

            var response = await _httpClient.PostAsJsonAsync($"khachhang/{khachHang.Id}/diachi", diaChi);
            return RedirectToAction("Index");
        }

        // GET: Sửa địa chỉ
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var res = await _httpClient.GetAsync($"https://localhost:7095/api/DiaChi/get-one/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var diaChi = JsonConvert.DeserializeObject<DiaChiDTO>(json);
            return View(diaChi);
        }

        [HttpPut]
        public async Task<IActionResult> Edit(Guid id, DiaChiDTO diaChi)
        {
            var res = await _httpClient.PutAsJsonAsync($"https://localhost:7095/api/DiaChi/diachi/{id}", diaChi);
            return RedirectToAction("Index");
        }


        // GET: Xoá địa chỉ
        public async Task<IActionResult> Delete(Guid id)
        {
            await _httpClient.DeleteAsync($"diachi/{id}");
            return RedirectToAction("Index");
        }




    }
}
