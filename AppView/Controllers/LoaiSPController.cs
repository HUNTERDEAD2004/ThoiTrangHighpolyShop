using AppData.Models;
using AppData.ViewModels.SanPham;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using AppView.PhanTrang;
using AppData.ViewModels;
using DocumentFormat.OpenXml.InkML;
using System.Net;

namespace AppView.Controllers
{
    public class LoaiSPController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly AssignmentDBContext dBContext;
        public LoaiSPController()
        {
            _httpClient = new HttpClient();
            dBContext = new AssignmentDBContext();
        }
        public int PageSize = 8;
        // laam them 
        public async Task<IActionResult> Show(int ProductPage = 1)
        {
            try
            {
                string apiUrl = "https://localhost:7095/api/LoaiSP/getAll";
                var response = await _httpClient.GetAsync(apiUrl);
                string apiData = await response.Content.ReadAsStringAsync();
                var loaiSPs = JsonConvert.DeserializeObject<List<LoaiSP>>(apiData);

                // Không lọc loại cha nữa, hiển thị tất cả
                var pagedLoaiSPs = loaiSPs.Skip((ProductPage - 1) * PageSize).Take(PageSize).ToList();

                return View(new PhanTrangLoaiSP
                {
                    listlsp = pagedLoaiSPs,
                    PagingInfo = new PagingInfo
                    {
                        ItemsPerPage = PageSize,
                        CurrentPage = ProductPage,
                        TotalItems = loaiSPs.Count
                    }
                });
            }
            catch (Exception ex)
            {
                // Gợi ý: Log lỗi ở đây để dễ debug
                Console.WriteLine(ex);
                return View(new PhanTrangLoaiSP
                {
                    listlsp = new List<LoaiSP>(),
                    PagingInfo = new PagingInfo
                    {
                        ItemsPerPage = PageSize,
                        CurrentPage = ProductPage,
                        TotalItems = 0
                    }
                });
            }
        }
        // Tim kiem Loai SP theo ten
        [HttpGet]
        public async Task<IActionResult> TimKiemLoaiSPTheoTen(string? ten, int ProductPage = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ten))
                {
                    ViewData["SearchError"] = "Vui lòng nhập tên để tìm kiếm";
                    return RedirectToAction("Show");
                }
                string apiUrl = $"https://localhost:7095/api/LoaiSP/TimKiemLoaiSP?name={ten}";
                var response = await _httpClient.GetAsync(apiUrl);
                string apiData = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<LoaiSP>>(apiData);
                if (users.Count == 0)
                {
                    ViewData["SearchError"] = "Không tìm thấy kết quả phù hợp";
                }
                return View("Show", new PhanTrangLoaiSP
                {
                    listlsp = users
                             .Skip((ProductPage - 1) * PageSize).Take(PageSize),
                    PagingInfo = new PagingInfo
                    {
                        ItemsPerPage = PageSize,
                        CurrentPage = ProductPage,
                        TotalItems = users.Count()
                    }
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IActionResult> Create()
        {
            try
            {
                var responseLoaiSP = _httpClient.GetAsync(_httpClient.BaseAddress + $"https://localhost:7095/api/LoaiSP/getAll").Result;
                if (responseLoaiSP.IsSuccessStatusCode)
                {
                    ViewData["listLoaiSP"] = JsonConvert.DeserializeObject<List<LoaiSP>>(responseLoaiSP.Content.ReadAsStringAsync().Result);
                }
                return View();
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create(LoaiSPRequest lsp)
        {
            try
            {
                lsp.ID = Guid.NewGuid();
                lsp.TrangThai = 1;
                string apiURL = $"https://localhost:7095/api/LoaiSP/save";
                var content = new StringContent(JsonConvert.SerializeObject(lsp), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiURL, content);
				if (response.IsSuccessStatusCode)
				{
					return Json(new { success = true, message = "Thêm loại sản phẩm thành công!" });
				}
				else
				{
					return Json(new { success = false, message = "Loại sản phẩm này đã có trong danh sách" });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
			}
		}
		public async Task<IActionResult> Details(Guid id)
        {

            string apiUrl = $"https://localhost:7095/api/LoaiSP/getById/{id}";
            var response = await _httpClient.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            var LoaiSPs = JsonConvert.DeserializeObject<LoaiSP>(apiData);
            return View(LoaiSPs);

        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                string apiUrl = $"https://localhost:7095/api/LoaiSP/getById/{id}";
                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest();
                }

                var lspJson = await response.Content.ReadAsStringAsync();
                var lsp = JsonConvert.DeserializeObject<LoaiSP>(lspJson); // sử dụng LoaiSP thay vì LoaiSPRequest

                return View(lsp);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LoaiSP lsp)
        {
            try
            {
                string apiUrl = $"https://localhost:7095/api/LoaiSP/save";
                var content = new StringContent(JsonConvert.SerializeObject(lsp), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);

				if (response.IsSuccessStatusCode)
				{
					return Json(new { success = true, message = "Cập nhật loại sản phẩm thành công!" });
				}
				else if (response.StatusCode == HttpStatusCode.BadRequest)
				{
					return Json(new { success = false, message = "Loại sản phẩm này đã tồn tại trong danh sách" });
				}
				else
				{
					return Json(new { success = false, message = "Cập nhật thất bại!" });
				}
			}
			catch (Exception)
			{
				return Json(new { success = false, message = "Có lỗi hệ thống!" });
			}
		}

        [HttpGet]
        public async Task<IActionResult> GetLoaiSpById(Guid id, int ProductPage = 1)
        {
            // list loai san pham con
            try
            {
                string apiUrl = $"https://localhost:7095/api/LoaiSP?id={id}";
                var response = await _httpClient.GetAsync(apiUrl);
                string apiData = await response.Content.ReadAsStringAsync();
                var LoaiSPs = JsonConvert.DeserializeObject<List<LoaiSP>>(apiData);
                return View(new PhanTrangLoaiSP
                {
                    listlsp = LoaiSPs.Where(x => x.TrangThai == 1)
                            .Skip((ProductPage - 1) * PageSize).Take(PageSize),
                    PagingInfo = new PagingInfo
                    {
                        ItemsPerPage = PageSize,
                        CurrentPage = ProductPage,
                        TotalItems = LoaiSPs.Count()
                    }
                });
            }
            catch (Exception) { throw; }
        }
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetLoaiSPCon(Guid idLoaiSPCha)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7095/api/LoaiSP/GetLoaiSPCon?idLoaiSPCha={idLoaiSPCha}");
            var data = await response.Content.ReadAsStringAsync();
            return Content(data, "application/json");
        }

        [HttpGet]

        public async Task<IActionResult> EditLoaiSPCon(Guid id)
        {
            try
            {
                string apiUrl = $"https://localhost:7095/api/LoaiSP/getById/{id}";
                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest();
                }
                var lspJson = await response.Content.ReadAsStringAsync();
                var lsp = JsonConvert.DeserializeObject<LoaiSP>(lspJson);
                return View(lsp);
            }
            catch (Exception) { throw; }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLoaiSP(LoaiSP lsp)
        {
            try
            {
                lsp.TrangThai = 1;

                string apiUrl = "https://localhost:7095/api/LoaiSP/save";
                var content = new StringContent(JsonConvert.SerializeObject(lsp), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Show", "LoaiSP"); // Trở lại danh sách loại sản phẩm
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    ViewBag.ErrorMessage = "Loại sản phẩm này đã tồn tại.";
                    return View(lsp); // Hiển thị lại form để sửa
                }

                TempData["Error"] = "Cập nhật thất bại.";
                return RedirectToAction("Show", "LoaiSP");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Lỗi hệ thống: " + ex.Message;
                return View(lsp);
            }
        }

        public async Task<IActionResult> CreateLoaiSPCon()
        {
            var responseLoaiSP = _httpClient.GetAsync(_httpClient.BaseAddress + $"https://localhost:7095/api/LoaiSP/getAll").Result;
            if (responseLoaiSP.IsSuccessStatusCode)
            {
                ViewData["listLoaiSP"] = JsonConvert.DeserializeObject<List<LoaiSP>>(responseLoaiSP.Content.ReadAsStringAsync().Result);
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateLoaiSPCon(LoaiSP lsp)
        {
            lsp.TrangThai = 1;
            string apiURL = $"https://localhost:7095/api/LoaiSP/save";
            var content = new StringContent(JsonConvert.SerializeObject(lsp), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiURL, content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetLoaiSpById");
            }
            return View();
        }
        public async Task<IActionResult> Sua(Guid id)
        {
            try
            {
                var timkiem = dBContext.LoaiSPs.FirstOrDefault(x => x.ID == id);
                if (timkiem != null)
                {
                    timkiem.TrangThai = timkiem.TrangThai == 0 ? 1 : 0;
                    dBContext.LoaiSPs.Update(timkiem);
                    dBContext.SaveChanges();
                    return RedirectToAction("Show");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception) { throw; }
        }
    }
}
