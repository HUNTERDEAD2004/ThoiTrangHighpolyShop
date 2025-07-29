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
                    //Sắp xếp
                    if (filter.sortSP == "1")
                    {
                        lstSanpham = lstSanpham.OrderBy(x => Convert.ToInt32(x.Ma.Substring(2))).ToList();
                    }
                    else if (filter.sortSP == "6")
                    {
                        lstSanpham = lstSanpham.OrderBy(x => x.Ten).ToList();
                    }
                    else if (filter.sortSP == "2")
                    {
                        lstSanpham = lstSanpham.OrderBy(x => x.GiaBan).ToList();
                    }
                    else if (filter.sortSP == "3")
                    {
                        lstSanpham = lstSanpham.OrderByDescending(x => x.GiaBan).ToList();
                    }
                    else if (filter.sortSP == "4")
                    {
                        lstSanpham = lstSanpham.OrderBy(x => x.SoLuong).ToList();
                    }
                    else if (filter.sortSP == "5")
                    {
                        lstSanpham = lstSanpham.OrderByDescending(x => x.SoLuong).ToList();
                    }
                    //Tìm kiếm theo tên sản phẩm
                    if (filter.search != null)
                    {
                        lstSanpham = lstSanpham.Where(x => x.Ten.ToLower().Contains(filter.search.ToLower())).ToList();
                    }
                    //Tìm kiếm theo giá
                    if (filter.minPrice != null)
                    {
                        lstSanpham = lstSanpham.Where(x => x.GiaBan >= filter.minPrice).ToList();
                    }
                    if (filter.maxPrice != null)
                    {
                        lstSanpham = lstSanpham.Where(x => x.GiaBan <= filter.maxPrice).ToList();
                    }
                    //Tìm kiếm theo loại sản phẩm
                    if (filter.loaiSPCha != "all")
                    {
                        lstSanpham = lstSanpham.Where(x => x.LoaiSPCha == filter.loaiSPCha).ToList();
                        if (filter.loaiSPCon != "all")
                        {
                            lstSanpham = lstSanpham.Where(x => x.LoaiSPCon == filter.loaiSPCon).ToList();
                        }
                    }
                    var model = lstSanpham.Skip((filter.page - 1) * filter.pageSize).Take(filter.pageSize).ToList();
                    return Json(new
                    {
                        data = model,
                        total = lstSanpham.Count,
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
        public JsonResult GetLoaiSPCha()
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllLoaiSPCha").Result;
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
        public async Task<MauSac?> TaoMauMoiTuSanPham(string ten, string ma, int trangThai)
        {
            try
            {
                var requestData = new
                {
                    ten = ten,
                    ma = ma,
                    trangthai = 1
                };

                var response = await _httpClient.PostAsJsonAsync("MauSac/AddMauSac", requestData);

                if (response.IsSuccessStatusCode)
                {
                    var mau = await response.Content.ReadFromJsonAsync<MauSac>();
                    return mau;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
        public async Task<ChatLieu?> TaoChatLieuMoiTuSanPham(string ten, int trangThai)
        {
            try
            {
                var requestData = new
                {
                    ten = ten,
                    trangthai = 1
                };

                var response = await _httpClient.PostAsJsonAsync("ChatLieu/AddChatLieu", requestData);

                if (response.IsSuccessStatusCode)
                {
                    var chatLieu = await response.Content.ReadFromJsonAsync<ChatLieu>();
                    return chatLieu;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
        public async Task<KichCo?> TaoKichCoTuSanPhamAsync(string tenKichCo)
        {
            try
            {
                var data = new { ten = tenKichCo }; // API của bạn chỉ cần tên

                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}KichCo/AddKichCo", data);

                if (!response.IsSuccessStatusCode)
                    return null;

                var kichCo = await response.Content.ReadFromJsonAsync<KichCo>();
                return kichCo;
            }
            catch
            {
                return null;
            }
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
        public async Task<JsonResult> GetLoaiSPCon(Guid idLoaiSPCha)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}SanPham/GetAllLoaiSPCon?idLoaiSPCha={idLoaiSPCha}");
                if (!response.IsSuccessStatusCode)
                    return Json(new { TrangThai = false });

                var body = await response.Content.ReadAsStringAsync();
                var loaiSP = JsonConvert.DeserializeObject<List<LoaiSP>>(body);

                return Json(new { KetQua = loaiSP, TrangThai = true });
            }
            catch (Exception ex)
            {
                // Có thể log nếu cần: _logger.LogError(ex, "...");
                return Json(new { TrangThai = false });
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
                    IDLoaiSPCha = sanPhamRequest.IDLoaiSPCha,
                    IDLoaiSPCon = sanPhamRequest.IDLoaiSPCon
                };

                request.IDMauSacs?.RemoveAll(id => XoaMau(id));
                request.IDKichCos?.RemoveAll(id => XoaSize(id));

                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}SanPham/AddSanPham", request);

                if (!response.IsSuccessStatusCode)
                {
                    _iFileService.DeleteFile(filePath, wwwrootPath);
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "Tạo sản phẩm thất bại.";
                    return RedirectToAction("ProductManager");
                }

                TempData["AlertType"] = "success";
                TempData["AlertMessage"] = "Tạo sản phẩm thành công!";
                return RedirectToAction("ProductManager");
            }
            catch
            {
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "Đã xảy ra lỗi khi tạo sản phẩm.";
                return RedirectToAction("ProductManager");
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
        public IActionResult QuanLyAnhChiTiet(Guid idSanPham)
        {
            try
            {
                var response = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/GetAllAnhSanPhamChiTiet?idSanPham=" + idSanPham).Result;
                if (response.IsSuccessStatusCode)
                {
                    var lstAnh = JsonConvert.DeserializeObject<List<UploadAnhViewModel>>(response.Content.ReadAsStringAsync().Result);
                    ViewData["IDSanPham"] = idSanPham.ToString();
                    return View("QuanLyAnhChiTiet", lstAnh.OrderBy(x => x.TenMau).ToList()); // ✅ Trả về View
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API GetAllAnhSanPhamChiTiet");
                return RedirectToAction("ProductDetail");
            }

            return View("QuanLyAnhChiTiet", new List<UploadAnhViewModel>()); // Trả về view rỗng nếu lỗi
        }

        [HttpPost]
        public async Task<IActionResult> AddImage(List<UploadAnhViewModel> model)
        {
            try
            {
                string wwwrootPath = _hostEnvironment.WebRootPath;
                var lstAnhRequest = new List<AnhRequest>();

                foreach (var item in model)
                {
                    if (item.Image == null || item.Image.Length == 0) continue;

                    string imagePath = await _iFileService.AddFile(item.Image, wwwrootPath);

                    lstAnhRequest.Add(new AnhRequest
                    {
                        IDSanPhamChiTiet = item.IDChiTietSanPham,
                        DuongDan = imagePath,
                        MaMau = item.MaMau
                    });
                }

                var response = await _httpClient.PostAsJsonAsync("SanPham/AddImage", lstAnhRequest);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ProductDetail", new { idSanPham = TempData.Peek("IDSanPham") });
                }

                return BadRequest("Lỗi khi gọi API.");
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi hệ thống: " + ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateImage(IFormFile file, string id, string duongDan, Guid idSanPham)
        {
            try
            {
                string wwwrootPath = _hostEnvironment.WebRootPath;

                // Lưu ảnh mới
                var duongDanMoi = await _iFileService.AddFile(file, wwwrootPath);
                if (string.IsNullOrEmpty(duongDanMoi)) return BadRequest("Upload ảnh thất bại");

                // Gửi yêu cầu cập nhật ảnh
                var anh = new Anh
                {
                    ID = new Guid(id), // Đây phải là ID của ảnh cần sửa
                    DuongDan = duongDanMoi,
                    TrangThai = 1
                };

                var response = await _httpClient.PutAsJsonAsync("SanPham/UpdateImage", anh);
                if (response.IsSuccessStatusCode)
                {
                    // Xoá ảnh cũ
                    _iFileService.DeleteFile(duongDan, wwwrootPath);
                    return RedirectToAction("QuanLyAnhChiTiet", new { idSanPham });
                }

                return BadRequest("Cập nhật ảnh thất bại");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                return BadRequest("Lỗi hệ thống");
            }
        }
        [HttpGet]
        public IActionResult DeleteImage(string duongDan, string id, string idSanPham)
        {
            try
            {
                string wwwrootPath = _hostEnvironment.WebRootPath;
                var response = _httpClient.DeleteAsync($"SanPham/DeleteImage?id={id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    _iFileService.DeleteFile(duongDan, wwwrootPath);
                    return RedirectToAction("QuanLyAnhChiTiet", new { idSanPham });
                }
                return BadRequest();
            }
            catch
            {
                return BadRequest();
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
        public JsonResult UpdateGiaBanChiTietSanPham(string id, int giaBan)
        {
            try
            {
                ChiTietSanPhamRequest request = new ChiTietSanPhamRequest() { IDChiTietSanPham = new Guid(id), GiaBan = giaBan };
                var response = _httpClient.PutAsJsonAsync(_httpClient.BaseAddress + "SanPham/UpdateGiaBanChiTietSanPham", request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { Message = response.Content.ReadAsStringAsync().Result, TrangThai = true });
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
        public JsonResult UpdateTrangThaiChiTietSanPham(string id)
        {
            try
            {
                var response = _httpClient.GetAsync(_httpClient.BaseAddress + "SanPham/UpdateTrangThaiChiTietSanPham?id=" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { TrangThai = true });
                }
                else
                {
                    return Json(new { TrangThai = false });
                }
            }
            catch
            {
                return Json(new { TrangThai = false });
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
        [HttpGet]
        public async Task<JsonResult> DeleteProductDetail(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync("SanPham/DeleteChiTietSanPham?id=" + id);
                if (response.IsSuccessStatusCode)
                {
                    var ketQua = Convert.ToBoolean(await response.Content.ReadAsStringAsync());
                    if (ketQua)
                    {
                        return Json(new { TrangThai = true });
                    }
                    else return Json(new { TrangThai = false, Loi = "Không thể xóa sản phẩm mặc định" });
                }
                else
                {
                    return Json(new { TrangThai = false, Loi = "Error" });
                }
            }
            catch
            {
                return Json(new { TrangThai = false, Loi = "Error" });
            }
        }
        [HttpGet]
        public async Task<JsonResult> UndoProductDetail(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync("SanPham/UndoChiTietSanPham?id=" + id);
                if (response.IsSuccessStatusCode)
                {
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            catch
            {
                return Json(false);
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

        // Load danh sách loại sản phẩm cha
        var loaiSPChas = await _httpClient.GetFromJsonAsync<List<LoaiSP>>("SanPham/GetAllLoaiSPCha");
        ViewBag.LoaiSPChas = new SelectList(loaiSPChas, "ID", "Ten", response.IDLoaiSPCha);

        // Load danh sách loại sản phẩm con theo cha
        if (response.IDLoaiSPCha != Guid.Empty)
        {
            var loaiSPCons = await _httpClient.GetFromJsonAsync<List<LoaiSP>>(
                $"SanPham/GetAllLoaiSPCon?idLoaiSPCha={response.IDLoaiSPCha}");

            ViewBag.LoaiSPCons = new SelectList(loaiSPCons, "ID", "Ten", response.IDLoaiSPCon);
        }
        else
        {
            ViewBag.LoaiSPCons = new SelectList(new List<LoaiSP>(), "ID", "Ten");
        }

        return View(response);
    }
    catch
    {
        return View(new SanPhamUpdateRequest());
    }
}

        [HttpPost]
        public IActionResult UpdateSanPham(SanPhamUpdateRequest request)
        {
            try
            {
                var response = _httpClient.PutAsJsonAsync("SanPham/UpdateSanPham", request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ProductManager");
                }
                else return BadRequest();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
