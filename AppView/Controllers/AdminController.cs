using AppData.Models;
using AppData.ViewModels;
using AppData.ViewModels.SanPham;
using AppView.IServices;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Text;

namespace AppView.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IFileService _iFileService;
        private readonly AssignmentDBContext _context;
        private readonly ILogger<AdminController> _logger;
        public AdminController(AssignmentDBContext context, IWebHostEnvironment hostEnvironment, IFileService iFileService, ILogger<AdminController> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _hostEnvironment = hostEnvironment;
            _iFileService = iFileService;
            _context = context;
            _logger = logger;
        }
        public IActionResult HomePageAdmin(Guid id)
        {
            return RedirectToAction("BanHang", "BanHangTaiQuay");
        }
        public IActionResult ProductManager()
        {
            return View();
        }
        [HttpGet]
        public JsonResult ShowProduct(FilterData filter)
        {
            try
            {
                var response = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllSanPhamAdmin").Result;
                List<SanPhamViewModelAdmin> lstSanpham = new List<SanPhamViewModelAdmin>();

                if (response.IsSuccessStatusCode)
                {
                    lstSanpham = JsonConvert.DeserializeObject<List<SanPhamViewModelAdmin>>(response.Content.ReadAsStringAsync().Result);

                    // Tìm kiếm theo tên sản phẩm
                    if (!string.IsNullOrEmpty(filter.search))
                    {
                        lstSanpham = lstSanpham
                            .Where(x => x.Ten != null && x.Ten.ToLower().Contains(filter.search.ToLower()))
                            .ToList();
                    }

					// Lọc theo loại sản phẩm (multi-select)
					if (!string.IsNullOrEmpty(filter.loaiSP))
					{
						lstSanpham = lstSanpham
							.Where(x => x.LoaiSP != null && x.LoaiSP == filter.loaiSP)
							.ToList();
					}

					// Lọc theo chất liệu
					if (filter.chatLieu != null && filter.chatLieu.Any())
                    {
                        lstSanpham = lstSanpham
                            .Where(x => x.ChatLieu != null && filter.chatLieu.Contains(x.ChatLieu))
                            .ToList();
                    }

                    // Lọc theo giá
                    if (filter.minPrice != null)
                    {
                        lstSanpham = lstSanpham.Where(x => x.GiaBan >= filter.minPrice).ToList();
                    }
                    if (filter.maxPrice != null)
                    {
                        lstSanpham = lstSanpham.Where(x => x.GiaBan <= filter.maxPrice).ToList();
                    }

                    // Sắp xếp
                    switch (filter.sortSP)
                    {
                        case "1":
                            lstSanpham = lstSanpham.OrderBy(x => Convert.ToInt32(x.Ma.Substring(2))).ToList();
                            break;
                        case "2":
                            lstSanpham = lstSanpham.OrderBy(x => x.GiaBan).ToList();
                            break;
                        case "3":
                            lstSanpham = lstSanpham.OrderByDescending(x => x.GiaBan).ToList();
                            break;
                        case "4":
                            lstSanpham = lstSanpham.OrderBy(x => x.SoLuong).ToList();
                            break;
                        case "5":
                            lstSanpham = lstSanpham.OrderByDescending(x => x.SoLuong).ToList();
                            break;
                        case "6":
                            lstSanpham = lstSanpham.OrderBy(x => x.Ten).ToList();
                            break;
                    }

                    // Phân trang
                    var model = lstSanpham
                        .Skip((filter.page - 1) * filter.pageSize)
                        .Take(filter.pageSize)
                        .ToList();

                    return Json(new
                    {
                        data = model,
                        total = lstSanpham.Count,
                        status = true
                    });
                }

                return Json(new { status = false });
            }
            catch
            {
                return Json(new { status = false });
            }
        }
        [HttpPost]
        public JsonResult UpdateTrangThaiSanPham(string idSanPham, int trangThai)
        {
            try
            {
                var response = _httpClient.DeleteAsync(_httpClient.BaseAddress + "SanPham/UpdateTrangThaiSanPham?id=" + idSanPham + "&trangThai=" + trangThai).Result;
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { KetQua = trangThai, Status = true });
                }
                else return Json(new { Status = false });
            }
            catch
            {
                return Json(new { Status = false });
            }
        }
        public JsonResult GetLoaiSP()
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllLoaiSP").Result;

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new List<LoaiSP>());
                }

                var loaiSP = JsonConvert.DeserializeObject<List<LoaiSP>>(response.Content.ReadAsStringAsync().Result);
                return Json(loaiSP);
            }
            catch
            {
                return Json(new List<LoaiSP>());
            }
        }
        public JsonResult GetAllMauSac()
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllMauSac").Result;
                var mauSac = JsonConvert.DeserializeObject<List<MauSac>>(response.Content.ReadAsStringAsync().Result);
                return Json(mauSac);
            }
            catch
            {
                return Json(new List<MauSac>());
            }
        }
		[HttpPost]
		public async Task<JsonResult> TaoMauMoiTuSanPham(string ten, string ma, int trangThai = 1)
		{
			try
			{
				// Gọi API đúng kiểu query string vì AppAPI không nhận JSON
				var url = $"MauSac/ThemMauSac?ten={Uri.EscapeDataString(ten)}&ma={Uri.EscapeDataString(ma)}&trangthai={trangThai}";

				// Gửi POST với body rỗng
				var response = await _httpClient.PostAsync(url, null);

				if (response.IsSuccessStatusCode)
				{
					var mau = await response.Content.ReadFromJsonAsync<MauSac>();
					if (mau != null)
					{
						return Json(new { success = true, data = mau });
					}
					return Json(new { success = false, message = "API trả về null" });
				}

				var error = await response.Content.ReadAsStringAsync();
				return Json(new { success = false, message = "API lỗi: " + error });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Lỗi: " + ex.Message });
			}
		}


		[HttpPost]
		public async Task<JsonResult> TaoChatLieuMoiTuSanPham(string ten, int trangThai)
		{
			try
			{
				string apiUrl = $"ChatLieu/ThemChatLieu?ten={ten}&trangthai={trangThai}";
				var response = await _httpClient.PostAsync(apiUrl, null);

				if (response.IsSuccessStatusCode)
				{
					var chatLieu = await response.Content.ReadFromJsonAsync<ChatLieu>();
					return Json(new { success = true, data = chatLieu });
				}

				return Json(new { success = false, message = "API thêm chất liệu thất bại." });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Lỗi: " + ex.Message });
			}
		}

		[HttpPost]
		public async Task<IActionResult> TaoLoaiSanPhamMoi(string ten, int trangThai = 1)
		{
			if (string.IsNullOrWhiteSpace(ten))
				return Json(new { success = false, message = "Tên loại sản phẩm không được để trống" });

			try
			{
				var lsp = new LoaiSPRequest
				{
					ID = Guid.NewGuid(),
					Ten = ten.Trim(),
					TrangThai = trangThai
				};

				string apiURL = $"https://localhost:7095/api/LoaiSP/save";
				var content = new StringContent(JsonConvert.SerializeObject(lsp), Encoding.UTF8, "application/json");
				var response = await _httpClient.PostAsync(apiURL, content);

				if (response.IsSuccessStatusCode)
				{
					return Json(new { success = true, data = new { id = lsp.ID, ten = lsp.Ten } });
				}

				return Json(new { success = false, message = "Loại sản phẩm đã tồn tại hoặc lỗi API" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}


		public async Task<KichCo?> TaoKichCoTuSanPhamAsync(string tenKichCo)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(tenKichCo))
					return null;

				// Gọi API đúng kiểu query string
				string url = $"{_httpClient.BaseAddress}KichCo/ThemKichCo?ten={Uri.EscapeDataString(tenKichCo)}&trangthai=1";

				var response = await _httpClient.PostAsync(url, null);

				if (!response.IsSuccessStatusCode)
					return null;

				// Nếu API trả về đối tượng KichCo, deserialize
				var kichCo = await response.Content.ReadFromJsonAsync<KichCo>();
				return kichCo;
			}
			catch
			{
				return null;
			}
		}

		[HttpPost]
		public async Task<JsonResult> TaoKichCoMoiTuSanPham(string ten)
		{
			if (string.IsNullOrWhiteSpace(ten))
				return Json(new { success = false, message = "Tên kích cỡ không được để trống" });

			var kichCoMoi = await TaoKichCoTuSanPhamAsync(ten);

			if (kichCoMoi != null)
			{
				// Trả về dữ liệu đầy đủ để JS xử lý
				return Json(new { success = true, data = new { id = kichCoMoi.ID, ten = kichCoMoi.Ten } });
			}

			return Json(new { success = false, message = "Thêm kích cỡ thất bại hoặc đã tồn tại" });
		}

		public JsonResult GetAllKichCo()
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllKichCo").Result;
                var kichCo = JsonConvert.DeserializeObject<List<KichCo>>(response.Content.ReadAsStringAsync().Result);
                return Json(kichCo);
            }
            catch
            {
                return Json(new List<KichCo>());
            }

        }
        public JsonResult GetAllChatLieu()
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllChatLieu").Result;
                var chatLieu = JsonConvert.DeserializeObject<List<ChatLieu>>(response.Content.ReadAsStringAsync().Result);
                return Json(chatLieu);
            }
            catch
            {
                return Json(new List<ChatLieu>());
            }
        }
        // Danh sách các ID màu bị đánh dấu xoá
        private List<Guid> _mauBiXoa = new List<Guid>();
        private List<Guid> _sizeBiXoa = new List<Guid>();
        // Hàm kiểm tra xem ID màu có nằm trong danh sách bị xoá hay không
        private bool XoaMau(Guid id)
        {
            return _mauBiXoa.Contains(id);
        }
        // Hàm kiểm tra xem ID size có nằm trong danh sách bị xoá hay không
        private bool XoaSize(Guid id)
        {
            return _sizeBiXoa.Contains(id);
        }
        [HttpGet]
        public IActionResult AddSanPham()
        {
            return View();
        }
        [HttpPost]
		[HttpPost]
		public async Task<IActionResult> AddSanPham(IFormFile file, SanPhamRequest sanPhamRequest)
		{
			try
			{
				string wwwrootPath = _hostEnvironment.WebRootPath;
				var filePath = await _iFileService.AddFile(file, wwwrootPath);

				var request = new SanPhamRequest
				{
					Ten = sanPhamRequest.Ten,
					AnhDaiDien = filePath,
					MoTa = sanPhamRequest.MoTa,
					IDChatLieu = sanPhamRequest.IDChatLieu,
					IDKichCos = sanPhamRequest.IDKichCos,
					IDMauSacs = sanPhamRequest.IDMauSacs,
					IDLoaiSP = sanPhamRequest.IDLoaiSP,
				};

				request.IDMauSacs?.RemoveAll(id => XoaMau(id));
				request.IDKichCos?.RemoveAll(id => XoaSize(id));

				var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}SanPham/AddSanPham", request);

				if (!response.IsSuccessStatusCode)
				{
					_iFileService.DeleteFile(filePath, wwwrootPath);
					return Json(new { success = false, message = "Tạo sản phẩm thất bại." });
				}

				return Json(new { success = true, message = "Tạo sản phẩm thành công!" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo sản phẩm." });
			}
		}

		[HttpGet]
        public IActionResult ProductDetail(string idSanPham)
        {
            TempData["IDSanPham"] = idSanPham;
            return View();

        }
        [HttpGet]
        public JsonResult ShowProductDetail(string id, int page, int pageSize, string? ma, int? minPrice, int? maxPrice, int? minQuantity, int? maxQuantity, int? sort)
        {
            try
            {
                var response = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllChiTietSanPhamAdmin?idSanPham=" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    var lstSanpham = JsonConvert.DeserializeObject<List<ChiTietSanPhamViewModelAdmin>>(response.Content.ReadAsStringAsync().Result);
                    //Sắp xếp
                    if (sort == 1)
                    {
                        lstSanpham = lstSanpham.OrderBy(x => x.Ma).ToList();
                    }
                    else if (sort == 2)
                    {
                        lstSanpham = lstSanpham.OrderBy(x => x.GiaBan).ToList();
                    }
                    else if (sort == 3)
                    {
                        lstSanpham = lstSanpham.OrderByDescending(x => x.GiaBan).ToList();
                    }
                    else if (sort == 4)
                    {
                        lstSanpham = lstSanpham.OrderBy(x => x.SoLuong).ToList();
                    }
                    else if (sort == 5)
                    {
                        lstSanpham = lstSanpham.OrderByDescending(x => x.SoLuong).ToList();
                    }
                    //Tìm kiếm theo tên sản phẩm
                    if (ma != null)
                    {
                        lstSanpham = lstSanpham.Where(x => x.Ma.Contains(ma.ToUpper())).ToList();
                    }
                    //Tìm kiếm theo giá
                    if (minPrice != null)
                    {
                        lstSanpham = lstSanpham.Where(x => x.GiaBan >= minPrice).ToList();
                    }
                    if (maxPrice != null)
                    {
                        lstSanpham = lstSanpham.Where(x => x.GiaBan <= maxPrice).ToList();
                    }
                    //Tìm kiếm theo số lượng
                    if (minQuantity != null)
                    {
                        lstSanpham = lstSanpham.Where(x => x.SoLuong >= minQuantity).ToList();
                    }
                    if (maxQuantity != null)
                    {
                        lstSanpham = lstSanpham.Where(x => x.SoLuong <= maxQuantity).ToList();
                    }
                    var model = lstSanpham.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                    return Json(new
                    {
                        data = model,
                        total = lstSanpham.Count,
                        status = true
                    }); ;
                }
                else return Json(new { status = false });
            }
            catch
            {
                return Json(new { status = false });
            }
        }
        [HttpGet]
        public async Task<IActionResult> QuanLyAnhChiTiet(Guid idSanPham)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}SanPham/GetAllAnhSanPhamChiTiet?idSanPham={idSanPham}");

                List<UploadAnhViewModel> lstAnh = new();
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    lstAnh = JsonConvert.DeserializeObject<List<UploadAnhViewModel>>(jsonData) ?? new();
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API trả lỗi: {StatusCode} - {Error}", response.StatusCode, err);
                }

                ViewData["IDSanPham"] = idSanPham.ToString();
                return View("QuanLyAnhChiTiet", lstAnh.OrderBy(x => x.TenMau).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API GetAllAnhSanPhamChiTiet");
                TempData["ErrorMessage"] = "Không thể tải dữ liệu ảnh.";
                return RedirectToAction("ProductDetail", new { idSanPham });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddImage([FromBody] List<UploadAnhViewModel> model)
        {
            try
            {
                if (model == null || !model.Any())
                    return BadRequest("Không có dữ liệu.");

                var lstAnhRequest = new List<AnhRequest>();

                foreach (var item in model)
                {
                    if (item.DuongDanAnh == null || !item.DuongDanAnh.Any())
                        continue;

                    foreach (var duongDan in item.DuongDanAnh)
                    {
                        if (string.IsNullOrWhiteSpace(duongDan))
                            continue;

                        foreach (var idChiTiet in item.DanhSachIDChiTietSP)
                        {
                            lstAnhRequest.Add(new AnhRequest
                            {
                                IDSanPhamChiTiet = idChiTiet,
                                DuongDan = duongDan,
                                MaMau = item.MaMau
                            });
                        }
                    }
                }

                if (!lstAnhRequest.Any())
                    return BadRequest("Không có ảnh hợp lệ để lưu.");

                var response = await _httpClient.PostAsJsonAsync("SanPham/AddImage", lstAnhRequest);
                if (response.IsSuccessStatusCode)
                {
                    return Ok(); // hoặc Redirect nếu gọi từ View truyền thống
                }

                return BadRequest("Lỗi khi gọi API nội bộ.");
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi hệ thống: " + ex.Message);
            }
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteImage(string duongDan, string id, string idSanPham)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(duongDan) || string.IsNullOrWhiteSpace(id))
                    return BadRequest("Thiếu dữ liệu.");

                // Gọi API nội bộ để xóa trong DB
                var response = await _httpClient.DeleteAsync($"SanPham/DeleteImage?id={id}");

                if (!response.IsSuccessStatusCode)
                    return BadRequest("Xóa DB thất bại");

                // Nếu DB xóa thành công, tiếp tục xóa file vật lý
                string wwwrootPath = _hostEnvironment.WebRootPath;
                bool xoaFile = _iFileService.DeleteFile(duongDan, wwwrootPath);

                if (!xoaFile)
                    Console.WriteLine($"Không thể xóa file vật lý: {duongDan}");

                return Ok(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi server khi xóa ảnh: " + ex.Message);
                return BadRequest("Lỗi server");
            }
        }
        [HttpGet]
        public IActionResult AddChiTietSanPham(string idSanPham)
        {
            TempData["IDSanPham"] = idSanPham;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddChiTietSanPham(ChiTietSanPhamAddRequest request)
        {
            try
            {
                // Xoá các màu và size bị đánh dấu xoá
                request.IDMauSacs?.RemoveAll(XoaMau);
                request.IDKichCos?.RemoveAll(XoaSize);

                // Lấy ID sản phẩm từ TempData
                string idSanPham = TempData.Peek("IDSanPham")?.ToString();
                if (string.IsNullOrEmpty(idSanPham)) return BadRequest("Không tìm thấy ID sản phẩm.");

                request.IDSanPham = new Guid(idSanPham);

                // Gửi request tạo chi tiết sản phẩm
                var response = await _httpClient.PostAsJsonAsync(
                    _httpClient.BaseAddress + "SanPham/AddChiTietSanPham",
                    request
                );

                if (!response.IsSuccessStatusCode)
                    return BadRequest();

                var responseContent = await response.Content.ReadAsStringAsync();
                var chiTietSanPham = JsonConvert.DeserializeObject<ChiTietSanPhamUpdateRequest>(responseContent);

                if (chiTietSanPham?.ChiTietSanPhams == null || !chiTietSanPham.ChiTietSanPhams.Any())
                {
                    return RedirectToAction("ProductDetail", new { idSanPham });
                }

                TempData["UpdateChiTietSanPham"] = responseContent;
                return RedirectToAction("ProductManager");
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public JsonResult UpdateSoluongChiTietSanPham(string id, int soLuong)
        {
            try
            {
                ChiTietSanPhamRequest request = new ChiTietSanPhamRequest() { IDChiTietSanPham = new Guid(id), SoLuong = soLuong };
                var response = _httpClient.PutAsJsonAsync(_httpClient.BaseAddress + "SanPham/UpdateSoluongChiTietSanPham", request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { Message = soLuong.ToString(), TrangThai = true });
                }
                else
                {
                    return Json(new { Message = "Error", TrangThai = false });
                }
            }
            catch
            {
                return Json(new { Message = "Error", TrangThai = false });
            }
        }
        [HttpPost]
        public async Task<JsonResult> UpdateGiaGocChiTietSanPham(string id, decimal GiaGoc)
        {
            try
            {
                var request = new ChiTietSanPhamRequest()
                {
                    IDChiTietSanPham = Guid.Parse(id),
                    GiaGoc = GiaGoc
                };
                    
                var response = await _httpClient.PutAsJsonAsync(_httpClient.BaseAddress + "SanPham/UpdateGiaGocChiTietSanPham", request);

                if (response.IsSuccessStatusCode)
                {
                    // Server trả kiểu gì thì dùng đúng kiểu đó
                    var result = await response.Content.ReadFromJsonAsync<decimal>(); // hoặc decimal nếu API return decimal
                    return Json(new { GiaGoc = result, success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false });
            }
        }
        [HttpPost]
        public async Task<JsonResult> UpdateMultipleProductDetails([FromBody] UpdateSoLuongGiaRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request),Encoding.UTF8,"application/json");

                var response = await _httpClient.PutAsync(
                    "SanPham/UpdateSoLuongGia", content);

                return Json(new { TrangThai = response.IsSuccessStatusCode });
            }
            catch
            {
                return Json(new { TrangThai = false });
            }
        }
        [HttpPost]
        public async Task<JsonResult> UpdateMacDinhChiTietSanPham(string id)
        {
            try
            {
                // Sửa từ POST thành PUT và sửa URL
                var response = await _httpClient.PutAsync($"SanPham/UpdateMacDinhChiTietSanPham/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { TrangThai = true });
                }

                return Json(new { TrangThai = false, Message = "API xử lý thất bại." });
            }
            catch (Exception ex)
            {
                return Json(new { TrangThai = false, Message = "Lỗi: " + ex.Message });
            }
        }
        public FileResult GenerateQRCode(string id, string ma)
        {
            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(id, QRCodeGenerator.ECCLevel.Q);
            QRCode qRCode = new QRCode(qRCodeData);
            using (MemoryStream stream = new MemoryStream())
            {
                using (Bitmap bitmap = qRCode.GetGraphic(20))
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return File(stream.ToArray(), "image/png", ma + ".png");
                }
            }
        }
        [HttpDelete]
        public async Task<JsonResult> DeleteProductDetail(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync("SanPham/DeleteChiTietSanPham?id=" + id);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var ketQua = JsonConvert.DeserializeObject<bool>(jsonString);

                    if (ketQua)
                    {
                        return Json(new { TrangThai = true });
                    }
                    else
                    {
                        return Json(new { TrangThai = false, Loi = "Không phù hợp để xóa sản phẩm mặc định" });
                    }
                }
                else
                {
                    return Json(new { TrangThai = false, Loi = "Lỗi kết nối API" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { TrangThai = false, Loi = "Exception: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<JsonResult> UndoProductDetail(string id)
        {
            try
            {
                // Validate ID là Guid
                if (!Guid.TryParse(id, out var guid))
                {
                    return Json(new { TrangThai = false, Loi = "ID không hợp lệ." });
                }

                var response = await _httpClient.GetAsync($"SanPham/UndoChiTietSanPham?id={id}");
                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { TrangThai = false, Loi = "Lỗi gọi API." });
                }

                var content = await response.Content.ReadAsStringAsync();

                if (bool.TryParse(content, out var ketQua))
                {
                    return ketQua
                        ? Json(new { TrangThai = true })
                        : Json(new { TrangThai = false, Loi = "Không thể bật lại. Kiểm tra giá, số lượng hoặc trạng thái sản phẩm cha." });
                }

                return Json(new { TrangThai = false, Loi = "Phản hồi API không hợp lệ." });
            }
            catch
            {
                return Json(new { TrangThai = false, Loi = "Lỗi hệ thống." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> UpdateSanPham(Guid id)
        {
            try
            {
                // Lấy thông tin sản phẩm
                var response = await _httpClient.GetFromJsonAsync<SanPhamUpdateRequest>($"SanPham/GetSanPhamById?id={id}");

                // Load danh sách chất liệu
                var chatLieus = await _httpClient.GetFromJsonAsync<List<ChatLieu>>("SanPham/GetAllChatLieu");
                ViewBag.ChatLieus = new SelectList(chatLieus, "ID", "Ten", response.IDChatLieu);

                // Load toàn bộ danh sách loại sản phẩm (chỉ 1 cấp)
                var loaiSPs = await _httpClient.GetFromJsonAsync<List<LoaiSP>>("SanPham/GetAllLoaiSP");
                ViewBag.LoaiSPs = new SelectList(loaiSPs, "ID", "Ten", response.IDLoaiSP);

                return View(response);
            }
            catch
            {
                return View(new SanPhamUpdateRequest());
            }
        }
        [HttpPost]
        public IActionResult UpdateSanPham([FromForm] SanPhamUpdateRequest request)
        {
            try
            {
                var response = _httpClient.PutAsJsonAsync("SanPham/UpdateSanPham", request).Result;
				if (response.IsSuccessStatusCode)
				{
					return Json(new { success = true, message = "Cập nhật sản phẩm thành công!" });
				}
				else
				{
					return Json(new { success = false, message = "Cập nhật sản phẩm thất bại!" });
				}
			}
			catch
			{
				return Json(new { success = false, message = "Có lỗi hệ thống!" });
			}
		}
        [HttpPost]
        public async Task<IActionResult> UploadImages(List<IFormFile> images)
        {
            if (images == null || !images.Any())
                return BadRequest("Không có ảnh nào được chọn.");

            string wwwrootPath = _hostEnvironment.WebRootPath;
            var duongDanList = new List<string>();

            foreach (var image in images)
            {
                var filePath = await _iFileService.AddFile(image, wwwrootPath);
                if (!string.IsNullOrEmpty(filePath))
                {
                    duongDanList.Add(filePath); // filePath đã là đường dẫn tương đối /img/product/xxx
                }
            }

            return Json(duongDanList); // Trả thẳng list đường dẫn
        }
    }
}
