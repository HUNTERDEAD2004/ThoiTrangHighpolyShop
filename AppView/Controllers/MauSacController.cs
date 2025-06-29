using AppData.Models;
using AppData.ViewModels;
using AppData.ViewModels.SanPham;
using AppView.PhanTrang;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace AppView.Controllers
{
    public class MauSacController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly AssignmentDBContext dBContext;
        public MauSacController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7095/api/");
            dBContext = new AssignmentDBContext();
        }
        public int PageSize = 8;

        public async Task<IActionResult> Show(int ProductPage = 1)
        {
            try
            {
                string apiUrl = $"https://localhost:7095/api/MauSac/GetAllMauSac";
                var response = await _httpClient.GetAsync(apiUrl);
                string apiData = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<MauSac>>(apiData);
                return View(new PhanTrangMauSac
                {
                    listNv = users
                            .Skip((ProductPage - 1) * PageSize).Take(PageSize),
                    PagingInfo = new PagingInfo
                    {
                        ItemsPerPage = PageSize,
                        CurrentPage = ProductPage,
                        TotalItems = users.Count()
                    }
                });
            }
            catch { return Redirect("https://localhost:5001/"); }
        }
        [HttpGet]
        public async Task<IActionResult> SearchTheoTen(string? Ten, int ProductPage = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Ten))
                {
                    ViewData["SearchError"] = "Vui lòng nhập tên để tìm kiếm";
                    return RedirectToAction("Show");
                }
                string apiUrl = $"https://localhost:7095/api/MauSac/TimKiemMauSac?name={Ten}";
                var response = await _httpClient.GetAsync(apiUrl);
                string apiData = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<MauSac>>(apiData);
                if (users.Count == 0)
                {
                    ViewData["SearchError"] = "Không tìm thấy kết quả phù hợp";
                }
                return View("Show", new PhanTrangMauSac
                {
                    listNv = users
                             .Skip((ProductPage - 1) * PageSize).Take(PageSize),
                    PagingInfo = new PagingInfo
                    {
                        ItemsPerPage = PageSize,
                        CurrentPage = ProductPage,
                        TotalItems = users.Count()
                    }
                });
            }
            catch { return Redirect("https://localhost:5001/"); }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MauSac ms)
        {
            try
            {
                ms.TrangThai = 1;
                
                if (string.IsNullOrEmpty(ms.Ten))
                {
                    ViewBag.ErrorMessage = "Vui lòng nhập tên màu sắc!";
                    return View();
                }
                else
                {
                    string encodedMauSac = Uri.EscapeDataString(ms.Ma);
                    string apiUrl = $"https://localhost:7095/api/MauSac/ThemMauSac?ten={ms.Ten}&ma={encodedMauSac}&trangthai={ms.TrangThai}";
                    ///*var content = new StringContent(JsonConvert.SerializeObject(ms), */Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(apiUrl, null);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Show");
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        ViewBag.ErrorMessage = "Màu sắc này đã có trong danh sách";
                        return View();
                    }
                    return View();
                }
            }
            catch { return Redirect("https://localhost:5001/"); }
        }


        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            string apiUrl = $"https://localhost:7095/api/MauSac/GetMauSacById?id={id}";
            var response = await _httpClient.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();

            var user = JsonConvert.DeserializeObject<MauSac>(apiData);
            return View(user);
        }
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            try
            {
                string apiUrl = $"https://localhost:7095/api/MauSac/GetMauSacById?id={id}";
                var response = _httpClient.GetAsync(apiUrl).Result;
                var apiData = response.Content.ReadAsStringAsync().Result;
                var user = JsonConvert.DeserializeObject<MauSac>(apiData);
                return View(user);
            }
            catch
            {
                return Redirect("https://localhost:5001/");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, MauSac ms)
        {

            try
            {
                ms.TrangThai = 1;

                if (string.IsNullOrEmpty(ms.Ten))
                {
                    ViewBag.ErrorMessage = "Vui lòng nhập tên màu sắc!";
                    return View();
                }
                else
                {
                    string encodedMauSac = Uri.EscapeDataString(ms.Ma);
                    string apiUrl = $"https://localhost:7095/api/MauSac/{id}?ten={ms.Ten}&ma={encodedMauSac}&trangthai={ms.TrangThai}";
                    var reponsen = await _httpClient.PutAsync(apiUrl, null);
                    if (reponsen.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Show");
                    }
                    ViewBag.ErrorMessage = "Màu sắc này đã có trong danh sách";
                    return View();
                }
            }
            catch { return Redirect("https://localhost:5001/"); }
        }
        public async Task<IActionResult> Delete(Guid id)
        {
            string apiUrl = $"https://localhost:7095/api/MauSac/{id}";
            var reposen = await _httpClient.DeleteAsync(apiUrl);
            if (reposen.IsSuccessStatusCode)
            {
                return RedirectToAction("Show");
            }
            return RedirectToAction("Show");
        }
        public async Task<IActionResult> Sua(Guid id)
        {
            try
            {
                var timkiem = dBContext.MauSacs.FirstOrDefault(x => x.ID == id);
                if (timkiem != null)
                {
                    timkiem.TrangThai = timkiem.TrangThai == 0 ? 1 : 0;
                    dBContext.MauSacs.Update(timkiem);
                    dBContext.SaveChanges();
                    return RedirectToAction("Show");
                }
                else
                {
                    return View();
                }
            }
            catch { return Redirect("https://localhost:5001/"); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Next(int ProductPage = 1)
        {
            ProductPage++;
            return await Show(ProductPage);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Previous(int ProductPage = 1)
        {
            ProductPage--;
            return await Show(ProductPage);
        }
    }
}
