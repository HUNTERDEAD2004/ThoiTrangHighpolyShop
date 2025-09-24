using AppData.Models;
using AppData.ViewModels;
using AppData.ViewModels.BanOffline;
using AppData.ViewModels.SanPham;
using AppData.ViewModels.VNPay;
using AppView.IServices;
using AppView.Models;
using AppView.Models.Momo;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace AppView.Controllers
{
    public class BanHangTaiQuayController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BanHangTaiQuayController> _logger;
        private readonly IMomoService _momoService;

        public BanHangTaiQuayController(IHttpClientFactory httpClientFactory, ILogger<BanHangTaiQuayController> logger, IMomoService momoService)
        {

            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
            _momoService = momoService;
        }
        //Giao diện bán hàng
        [HttpGet]
        public async Task<IActionResult> BanHang()
        {
            try
            {
                var listhdcho = await _httpClient.GetFromJsonAsync<List<HoaDon>>("HoaDon/GetAllHDCho");

                if (listhdcho != null)
                {
                    // Xóa hóa đơn cũ không thuộc ngày hôm nay
                    var deletehdcho = listhdcho.Where(c => !c.NgayTao.Date.Equals(DateTime.Today.Date)).ToList();
                    foreach (var item in deletehdcho)
                    {
                        var response = await _httpClient.DeleteAsync($"HoaDon/deleteHoaDon/{item.ID}");
                    }

                    // Lấy lại danh sách sau khi xóa
                    listhdcho = await _httpClient.GetFromJsonAsync<List<HoaDon>>("HoaDon/GetAllHDCho");
                }
                else
                {
                    listhdcho = new List<HoaDon>();
                }

                ViewData["lsthdcho"] = listhdcho;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi BanHang: {ex.Message}");
                ViewData["lsthdcho"] = new List<HoaDon>();
                return View();
            }
        }
        // Sản phẩm
        [HttpGet]
        public async Task<IActionResult> LoadSp(int page, int pagesize)
        {
                var listsanPham = await _httpClient.GetFromJsonAsync<List<SanPhamBanHang>>("SanPham/getAllSPBanHang");
            listsanPham = listsanPham.Where(c => c.GiaGoc > 0).ToList();
                var model = listsanPham.Skip((page - 1) * pagesize).Take(pagesize).ToList();
                int totalRow = listsanPham.Count;
            return Json(new
            {
                data = model,
                total = totalRow,
                status = true,
            });
        }
        //Hiển thị sản phẩm
        [HttpGet("/BanHangTaiQuay/ShowSPDetail/{idsp}")]
        public async Task<IActionResult> ShowSPDetail(string idsp)
        {
            var sP = await _httpClient.GetFromJsonAsync<ChiTietSanPhamBanHang>($"SanPham/getChiTietSPBHById/{idsp}");
            return PartialView("_SanPhamDetail", sP);
        }
        //Hiển thị lọc
        public async Task<IActionResult> ShowFilterSP()
        {
            var lsp = await _httpClient.GetFromJsonAsync<List<LoaiSP>>($"LoaiSP/getAll");
            ViewData["lstLSP"] = lsp;
            return PartialView("_LocSP");
        }
        // Lọc sản phẩm
        public async Task<IActionResult> LocSP(FilterSP filter)
        {
            try
            {
                var listSP = await _httpClient.GetFromJsonAsync<List<SanPhamBanHang>>("SanPham/getAllSPBanHang");
                //Lọc danh mục 
                if (filter.lstDM != null)
                {
                    listSP = listSP.Where(c => filter.lstDM.Contains(c.IdLsp)).ToList();
                }
                //Lọc giá
                if (filter.khoangGia != 0)
                {
                    switch (filter.khoangGia)
                    {
                        case 1:
                            listSP = listSP.Where(c => c.GiaBan < 100000).ToList();
                            break;
                        case 2:
                            listSP = listSP.Where(c => c.GiaBan >= 100000 && c.GiaBan < 200000).ToList();
                            break;
                        case 3:
                            listSP = listSP.Where(c => c.GiaBan >= 200000 && c.GiaBan < 300000).ToList();
                            break;
                        case 4:
                            listSP = listSP.Where(c => c.GiaBan >= 300000 && c.GiaBan < 400000).ToList();
                            break;
                        case 5:
                            listSP = listSP.Where(c => c.GiaBan >= 400000 && c.GiaBan < 500000).ToList();
                            break;
                        case 6:
                            listSP = listSP.Where(c => c.GiaBan >= 500000).ToList();
                            break;
                        default:
                            break;
                    }
                }
                //Phân trang
                if (listSP.Count == 0)
                {
                    listSP = await _httpClient.GetFromJsonAsync<List<SanPhamBanHang>>("SanPham/getAllSPBanHang");
                }
                var model = listSP.Skip((filter.page - 1) * filter.pageSize).Take(filter.pageSize).ToList();
                int totalRow = listSP.Count;
                return Json(new
                {
                    data = model,
                    total = totalRow,
                    status = true,
                });
            }
            catch (Exception ex)
            {
                return RedirectToAction("BanHang", "BanHangTaiQuay");
            }
        }
        //Tìm kiếm sản phẩm
        [HttpGet("/BanHangTaiQuay/Search/{keyword}")]
        public async Task<IActionResult> Search(string keyword)
        {
            try
            {
                var listsanPham = await _httpClient.GetFromJsonAsync<List<SanPhamBanHang>>("SanPham/getAllSPBanHang");
                listsanPham = listsanPham.Where(c => c.GiaGoc != 0).ToList();
                var distinctResult = listsanPham
                    .Where(c => c.Ten.ToLower().Contains(keyword.Trim().ToLower()))
                    .Distinct()
                    .ToList();
                var result = new List<SanPhamBanHang>();
                if (distinctResult.Count < 3)
                {
                    var additionalItems = distinctResult.Take(result.Count).ToList();
                    result.AddRange(additionalItems);
                }
                result = distinctResult.Take(3).ToList();
                return Json(new { data = result });
            }
            catch (Exception ex)
            {
                return RedirectToAction("BanHang", "BanHangTaiQuay");
            }
        }
        // Lấy Load CTSP trong SP
        [HttpGet("/BanHangTaiQuay/ShowListCTSP/{idsp}")]
        public async Task<IActionResult> ShowListCTSP(string idsp)
        {
            var lstctsP = await _httpClient.GetFromJsonAsync<List<ChiTietCTSPBanHang>>($"SanPham/getChiTietCTSPBHById/{idsp}");
            return Json(new { data = lstctsP });
        }
        public async Task<IActionResult> FilterCTSP(FilterCTSP filter)
        {
            try
            {
                var lstctsP = await _httpClient.GetFromJsonAsync<List<ChiTietCTSPBanHang>>($"SanPham/getChiTietCTSPBHById/{filter.IdSanPham}");
                //Lọc màu
                if (filter.lstIdMS != null)
                {
                    lstctsP = lstctsP.Where(c => filter.lstIdMS.Contains(c.idMauSac)).ToList();
                }
                //Lọc kích thước
                if (filter.lstIdKC != null)
                {
                    lstctsP = lstctsP.Where(c => filter.lstIdKC.Contains(c.idKichCo)).ToList();
                }
                return Json(new { data = lstctsP });
            }
            catch (Exception ex)
            {
                return RedirectToAction("BanHang", "BanHangTaiQuay");
            }
        }
        //Update ghi chú
        public async Task<IActionResult> UpdateGhichu(Guid idhd, string ghichu)
        {
            try
            {
                var loginInfor = new LoginViewModel();
                string? session = HttpContext.Session.GetString("LoginInfor");
                if (session != null)
                {
                    loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                }

                if (ghichu != null)
                {
                    var stringURL = $"https://localhost:7095/api/HoaDon/UpdateGhichu?idhd={idhd}&idnv={loginInfor.Id}&ghichu={ghichu}";
                    var response = await _httpClient.PutAsync(stringURL, null);
                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Lưu ghi chú thành công" });
                    }
                    else
                        return Json(new { success = false, message = "Lưu ghi chú thất bại" });
                }
                else
                {
                    return Json(new { success = false, message = "Ghi chú không được để trống" });
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("BanHang", "BanHangTaiQuay");
            }
        }
        //Lấy Hóa đơn chi tiết
        [HttpGet("/BanHangTaiQuay/getCTHD/{id}")]
        public async Task<IActionResult> getCTHD(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid guidId))
                {
                    return Json(new { success = false, message = "ID không hợp lệ" });
                }

                // Gọi API từ project khác để lấy thông tin hóa đơn
                var hdon = await _httpClient.GetFromJsonAsync<HoaDonViewModelBanHang>($"HoaDon/GetHDBanHang/{guidId}");

                if (hdon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Tính tổng tiền từ danh sách chi tiết
                var tongTien = hdon.lstHDCT?.Sum(ct => ct.SoLuong * ct.GiaKM) ?? 0;

                return Json(new
                {
                    success = true,
                    data = hdon.lstHDCT ?? new List<HoaDonChiTietViewModel>(),
                    tongTien = tongTien,
                    hoaDon = hdon
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi getCTHD: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi tải chi tiết hóa đơn" });
            }
        }
        // Thêm hóa đơn chi tiết
        public async Task<ActionResult> addHdct(HoaDonChiTietRequest request)
        {
            try
            {
                // Kiểm tra request
                if (request == null || request.IdChiTietSanPham == Guid.Empty || request.IdHoaDon == Guid.Empty)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                var ctsp = await _httpClient.GetFromJsonAsync<ChiTietSanPhamViewModel>($"SanPham/GetChiTietSanPhamByID?id={request.IdChiTietSanPham}");
                if (ctsp == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm này" });
                }

                if (ctsp.TrangThai == 0)
                {
                    return Json(new { success = false, message = "Sản phẩm không hoạt động" });
                }

                if (request.SoLuong <= 0)
                {
                    return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
                }

                if (request.SoLuong > ctsp.SoLuong)
                {
                    return Json(new { success = false, message = $"Số lượng vượt quá tồn kho. Chỉ còn {ctsp.SoLuong} sản phẩm" });
                }

                HoaDonChiTietRequest hdct = new HoaDonChiTietRequest()
                {
                    Id = Guid.NewGuid(),
                    IdChiTietSanPham = request.IdChiTietSanPham,
                    IdHoaDon = request.IdHoaDon,
                    SoLuong = request.SoLuong,
                    DonGia = ctsp.GiaBan
                };

                var response = await _httpClient.PostAsJsonAsync("ChiTietHoaDon/saveHDCT/", hdct);

                if (response.IsSuccessStatusCode)
                {
                    // Cập nhật số lượng tồn kho
                    await _httpClient.PostAsync($"SanPham/UpdateSoluongChiTietSanPham?IdChiTietSanPham={request.IdChiTietSanPham}&soLuong={-request.SoLuong}", null);

                    return Json(new { success = true, message = "Thêm sản phẩm thành công" });
                }

                return Json(new { success = false, message = "Thêm sản phẩm thất bại" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi addHdct: {ex.Message}");
                return Json(new { success = false, message = "Lỗi hệ thống khi thêm sản phẩm" });
            }
        }
        //Xóa chi tiết hóa đơn
        [HttpDelete("/BanHangTaiQuay/deleteHdct/{id}")]
        public async Task<ActionResult> deleteHdct(String id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"ChiTietHoaDon/delete/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Xóa thành công" });
                }
                else
                {
                    // Thêm thông tin lỗi chi tiết
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Xóa thất bại: {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi deleteHdct: {ex.Message}");
                return Json(new { success = false, message = "Lỗi hệ thống khi xóa sản phẩm" });
            }
        }
        // Lấy chi tiết hóa đơn cho giỏ hàng
        [HttpGet("/BanHangTaiQuay/GetChiTietHoaDon/{idHoaDon}")]
        public async Task<IActionResult> GetChiTietHoaDon(string idHoaDon)
        {
            try
            {
                var lstcthd = await _httpClient.GetFromJsonAsync<List<HoaDonChiTietViewModel>>($"ChiTietHoaDon/getByIdHD/{idHoaDon}");

                if (lstcthd == null)
                {
                    return Json(new { success = true, data = new List<HoaDonChiTietViewModel>(), tongTien = 0 });
                }

                lstcthd = lstcthd.Where(c => c.SoLuong > 0).ToList();
                var tongTien = lstcthd.Sum(c => c.SoLuong * c.GiaKM);

                return Json(new { success = true, data = lstcthd, tongTien = tongTien });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi GetChiTietHoaDon: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi tải giỏ hàng" });
            }
        }
        // Lấy thông tin chi tiết sản phẩm cho modal
        [HttpGet("/BanHangTaiQuay/ChiTietSanPham/{id}")]
        public async Task<IActionResult> ChiTietSanPham(string id)
        {
            try
            {
                var sP = await _httpClient.GetFromJsonAsync<ChiTietSanPhamBanHang>($"SanPham/getChiTietSPBHById/{id}");

                if (sP == null)
                {
                    return PartialView("_Error", "Không tìm thấy thông tin sản phẩm");
                }

                return PartialView("_SanPhamDetail", sP);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ChiTietSanPham: {ex.Message}");
                return PartialView("_Error", "Lỗi khi tải thông tin sản phẩm");
            }
        }
        //Cập nhật số lượng 
        [HttpPost("/BanHangTaiQuay/UpdateSL")]
        public async Task<IActionResult> UpdateSL(string idhdct, int sl)
        {
            try
            {
                var response = await _httpClient.PostAsync($"ChiTietHoaDon/UpdateSL?id={idhdct}&sl={sl}", null);
                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, message = "Cập nhật số lượng thành công" });
                else
                    return Json(new { success = false, message = "Số lượng sản phẩm không đủ" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi UpdateSL: {ex.Message}");
                return Json(new { success = false, message = "Lỗi hệ thống khi cập nhật số lượng" });
            }
        }
        //Check voucher
        [HttpGet]
        public async Task<IActionResult> CheckVoucher(Guid idvoucher, int ttien)
        {
            try
            {
                var vc = await _httpClient.GetFromJsonAsync<Voucher>($"Voucher/{idvoucher}");
                if (vc.HinhThucGiamGia == 0)
                {
                    return Json(new { success = true, idvoucher = vc.ID, giatri = vc.GiaTri, message = "Bạn được giảm " + vc.GiaTri.ToString("n0") + " VND" });

                }
                else if (vc.HinhThucGiamGia == 1)
                {
                    return Json(new { success = true, idvoucher = vc.ID, giatri = (ttien * vc.GiaTri / 100), message = "Bạn được giảm " + vc.GiaTri + "%" });
                }
                return Json(new { message = "Đã xảy ra lỗi" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("BanHang", "BanHangTaiQuay");
            }
        }

        //ThanhToan
        [HttpPost]
        public async Task<IActionResult> ThanhToan([FromBody] HoaDonThanhToanRequest request)
        {
            try
            {
                // ID cố định
                var ID_TIEN_MAT = Guid.Parse("9b6289bf-5e83-419a-94d5-926abb264961");
                var ID_MOMO = Guid.Parse("f29cd85d-0251-4b50-8867-6a88891417f6");

                if (request == null)
                {
                    return Json(new { success = false, message = "Request null" });
                }

                // Lấy nhân viên từ session nếu thiếu
                if (request.IdNhanVien == Guid.Empty)
                {
                    string? session = HttpContext.Session.GetString("LoginInfor");
                    if (!string.IsNullOrEmpty(session))
                    {
                        var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                        if (loginInfor != null)
                            request.IdNhanVien = loginInfor.Id;
                        else
                            return Json(new { success = false, message = "Không lấy được thông tin nhân viên từ session" });
                    }
                }

                // So sánh Guid an toàn bằng Equals (không ==)
                if (request.IDPhuongThucThanhToan.Equals(ID_TIEN_MAT))
                {
                    // Thanh toán tiền mặt
                    var hdrequest = new HoaDonThanhToanRequest()
                    {
                        Id = request.Id,
                        IdNhanVien = request.IdNhanVien,
                        NgayThanhToan = DateTime.Now,
                        IdVoucher = request.IdVoucher == Guid.Empty ? Guid.Empty : request.IdVoucher,
                        IDPhuongThucThanhToan = request.IDPhuongThucThanhToan,
                        TienShip = request.TienShip,
                        tenNguoiNhan = request.tenNguoiNhan,
                        sdtNguoiNhan = request.sdtNguoiNhan,
                        GhiChu = request.GhiChu,
                        diaChi = request.diaChi,
                        TongTien = request.TongTien,
                        TrangThai = 6,
                    };

                    var response = await _httpClient.PutAsJsonAsync("HoaDon/UpdateHoaDon/", hdrequest);
                    if (response.IsSuccessStatusCode)
                    {
                        await _httpClient.DeleteAsync($"HoaDon/DeleteHoaDonCho?idHoaDon={hdrequest.Id}");
                        return Json(new { success = true, message = "Thanh toán thành công" });
                    }

                    return Json(new { success = false, message = "Thanh toán thất bại" });
                }
                else if (request.IDPhuongThucThanhToan.Equals(ID_MOMO))
                {
                    // Update hóa đơn trước khi thanh toán momo
                    var hdChoUpdate = new HoaDonThanhToanRequest
                    {
                        Id = request.Id,
                        IdNhanVien = request.IdNhanVien,
                        IdVoucher = request.IdVoucher == Guid.Empty ? Guid.Empty : request.IdVoucher,
                        IDPhuongThucThanhToan = request.IDPhuongThucThanhToan,
                        TienShip = request.TienShip,
                        tenNguoiNhan = request.tenNguoiNhan,
                        sdtNguoiNhan = request.sdtNguoiNhan,
                        GhiChu = request.GhiChu,
                        diaChi = request.diaChi,
                        TongTien = request.TongTien,
                    };

                    var updateResponse = await _httpClient.PutAsJsonAsync("HoaDon/UpdateHoaDon/", hdChoUpdate);
                    if (!updateResponse.IsSuccessStatusCode)
                    {
                        return Json(new { success = false, message = "Không thể cập nhật hóa đơn trước khi thanh toán" });
                    }

                    var momoModel = new OrderInfoModel
                    {
                        FullName = request.tenNguoiNhan ?? "Khách hàng",
                        Amount = (double)request.TongTien,
                        OrderInfo = $"Thanh toán đơn hàng {request.Id} - {request.tenNguoiNhan}",
                        OrderId = request.Id.ToString()
                    };

                    var momoResult = await _momoService.CreatePaymentAsync(momoModel);
                    _logger.LogInformation($"Momo Service Result - ErrorCode: {momoResult?.ErrorCode}, PayUrl: {momoResult?.PayUrl}");

                    if (momoResult != null && momoResult.ErrorCode == 0)
                    {
                        var paymentSession = new MomoPaymentSession
                        {
                            HoaDonId = request.Id,
                            OrderId = request.Id.ToString(),
                            RequestData = request,
                            CreatedTime = DateTime.Now
                        };

                        HttpContext.Session.SetString($"MomoPayment_{request.Id}", JsonConvert.SerializeObject(paymentSession));

                        return Json(new
                        {
                            success = true,
                            paymentUrl = momoResult.PayUrl
                        });
                    }
                    else
                    {
                        var errorMessage = momoResult?.LocalMessage ?? momoResult?.Message ?? "Lỗi khi tạo link thanh toán Momo";
                        _logger.LogError($"Momo Error: {errorMessage}");
                        return Json(new { success = false, message = errorMessage });
                    }
                }

                return Json(new { success = false, message = "Phương thức không hỗ trợ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        private async Task<HoaDonThanhToanRequest> LayThongTinHoaDonTuDatabase(Guid hoadonId)
        {
            try
            {
                // Gọi API để lấy thông tin hóa đơn từ database 
                var hoadonResponse = await _httpClient.GetAsync($"HoaDon/GetById/{hoadonId}");
                if (hoadonResponse.IsSuccessStatusCode)
                {
                    var hoadonData = await hoadonResponse.Content.ReadFromJsonAsync<HoaDon>();
                    if (hoadonData != null)
                    {
                        // Tạo request từ dữ liệu database
                        return new HoaDonThanhToanRequest
                        {
                            Id = hoadonData.ID,
                            IdNhanVien = hoadonData.IDNhanVien ?? Guid.Empty,
                            IdVoucher = hoadonData.IDVoucher ?? Guid.Empty,
                            TongTien = hoadonData.TongTien ?? 0,
                            TienShip = hoadonData.TienShip,
                            tenNguoiNhan = hoadonData.TenNguoiNhan,
                            sdtNguoiNhan = hoadonData.SDT,
                            diaChi = hoadonData.DiaChi,
                            GhiChu = hoadonData.GhiChu
                        };
                    }
                }

                _logger.LogWarning($"Không thể lấy thông tin hóa đơn từ database: {hoadonId}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy thông tin hóa đơn từ database: {hoadonId}");
                return null;
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            try
            {
                _logger.LogInformation($"Momo callback received: {Request.QueryString}");

                var momoResult = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

                if (momoResult != null)
                {
                    _logger.LogInformation($"Momo result: OrderId={momoResult.OrderId}, Amount={momoResult.Amount}");

                    // parse về Guid
                    if (!Guid.TryParse(momoResult.OrderId, out Guid hoaDonId))
                    {
                        _logger.LogError($"OrderId Momo trả về không phải Guid hợp lệ: {momoResult.OrderId}");
                        return View("PaymentResult", new PaymentResultViewModel
                        {
                            Success = false,
                            Message = "Mã đơn hàng không hợp lệ"
                        });
                    }

                    // Session key dựa theo Guid gốc
                    var sessionKey = $"MomoPayment_{hoaDonId}";
                    var sessionData = HttpContext.Session.GetString(sessionKey);

                    HoaDonThanhToanRequest request = null;

                    if (!string.IsNullOrEmpty(sessionData))
                    {
                        var paymentSession = JsonConvert.DeserializeObject<MomoPaymentSession>(sessionData);
                        request = paymentSession.RequestData;

                        HttpContext.Session.Remove(sessionKey);
                        _logger.LogInformation($"Lấy thông tin từ Session thành công cho hóa đơn: {hoaDonId}");
                    }
                    else
                    {
                        _logger.LogWarning($"Không tìm thấy Session, thử lấy từ Database cho OrderId: {hoaDonId}");

                        request = await LayThongTinHoaDonTuDatabase(hoaDonId);
                        if (request != null)
                        {
                            _logger.LogInformation($"Lấy thông tin từ Database thành công cho hóa đơn: {hoaDonId}");
                        }
                    }

                    if (request != null)
                    {
                        return await XuLyHoaDonSauThanhToan(request, momoResult);
                    }
                    else
                    {
                        _logger.LogError($"Không tìm thấy thông tin thanh toán cho OrderId: {momoResult.OrderId}");
                        return View("PaymentResult", new PaymentResultViewModel
                        {
                            Success = false,
                            Message = "Không tìm thấy thông tin đơn hàng. Vui lòng liên hệ nhân viên để được hỗ trợ."
                        });
                    }
                }

                return View("PaymentResult", new PaymentResultViewModel
                {
                    Success = false,
                    Message = "Không thể xác thực kết quả thanh toán từ Momo"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong PaymentCallBack");
                return View("PaymentResult", new PaymentResultViewModel
                {
                    Success = false,
                    Message = $"Lỗi hệ thống: {ex.Message}"
                });
            }
        }

        private async Task<IActionResult> XuLyHoaDonSauThanhToan(HoaDonThanhToanRequest request, MomoExecuteResponseModel momoResult)
        {
            try
            {
                // Kiểm tra và bổ sung thông tin nếu cần
                if (request.IdNhanVien == Guid.Empty)
                {
                    // Lấy thông tin nhân viên từ session nếu có
                    string? session = HttpContext.Session.GetString("LoginInfor");
                    if (!string.IsNullOrEmpty(session))
                    {
                        var loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                        if (loginInfor != null)
                            request.IdNhanVien = loginInfor.Id;
                    }

                    // Nếu vẫn không có, sử dụng giá trị mặc định
                    if (request.IdNhanVien == Guid.Empty)
                    {
                        request.IdNhanVien = Guid.Parse("00000000-0000-0000-0000-000000000000"); // ID mặc định
                    }
                }

                // Cập nhật hóa đơn với thông tin đầy đủ
                var hdrequest = new HoaDonThanhToanRequest
                {
                    Id = request.Id,
                    IdNhanVien = request.IdNhanVien,
                    NgayThanhToan = DateTime.Now,
                    IdVoucher = request.IdVoucher,
                    IDPhuongThucThanhToan = Guid.Parse("f29cd85d-0251-4b50-8867-6a88891417f6"), // ID Momo
                    TienShip = request.TienShip,
                    tenNguoiNhan = !string.IsNullOrEmpty(request.tenNguoiNhan) ? request.tenNguoiNhan : "Khách hàng",
                    sdtNguoiNhan = !string.IsNullOrEmpty(request.sdtNguoiNhan) ? request.sdtNguoiNhan : "",
                    GhiChu = $"Đã thanh toán Momo - {momoResult.OrderInfo}",
                    diaChi = !string.IsNullOrEmpty(request.diaChi) ? request.diaChi : "",
                    TongTien = request.TongTien > 0 ? request.TongTien : decimal.Parse(momoResult.Amount),
                    TrangThai = 6 // Đã thanh toán
                };

                var response = await _httpClient.PutAsJsonAsync("HoaDon/UpdateHoaDon", hdrequest);
                if (response.IsSuccessStatusCode)
                {
                    // Xóa hóa đơn chờ nếu tồn tại
                    try
                    {
                        await _httpClient.DeleteAsync($"HoaDon/DeleteHoaDonCho?idHoaDon={request.Id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Không thể xóa hóa đơn chờ: {request.Id}");
                    }

                    _logger.LogInformation($"Cập nhật hóa đơn {request.Id} thành công");

                    return View("PaymentResult", new PaymentResultViewModel
                    {
                        Success = true,
                        OrderId = momoResult.OrderId,
                        Amount = momoResult.Amount,
                        Message = "Thanh toán Momo thành công!"
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Lỗi cập nhật hóa đơn: {errorContent}");

                    return View("PaymentResult", new PaymentResultViewModel
                    {
                        Success = false,
                        Message = "Thanh toán thành công nhưng có lỗi khi cập nhật hóa đơn. Vui lòng liên hệ nhân viên."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xử lý hóa đơn {request.Id}");
                return View("PaymentResult", new PaymentResultViewModel
                {
                    Success = false,
                    Message = $"Lỗi khi xử lý hóa đơn: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVouchers(decimal tongTien, Guid? userId)
        {
            try
            {
                // Gọi API service và truyền cả userId
                var response = await _httpClient.GetAsync(
                    $"Voucher/GetAllVoucherByTien?tongTien={tongTien}&userId={userId}"
                );

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new List<object>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var vouchers = JsonConvert.DeserializeObject<List<Voucher>>(json) ?? new List<Voucher>();

                var now = DateTime.Now;

                // Lọc voucher khả dụng thêm 1 lớp an toàn (mặc dù API service đã lọc)
                var vouchersKhachDung = vouchers
                    .Where(v =>
                        v.TrangThai == 1 &&
                        v.SoLuong > 0 &&
                        now >= v.NgayApDung &&
                        now <= v.NgayKetThuc &&
                        tongTien >= v.GiaTriToiThieu
                    )
                    .Select(v => new
                    {
                        v.ID,
                        v.Ten,
                        v.MaVoucher,
                        v.HinhThucGiamGia,
                        GiaTri = v.GiaTri,
                        GiaTriToiThieu = v.GiaTriToiThieu,
                        GiaTriToiDa = v.GiaTriToiDa,
                        v.NgayApDung,
                        v.NgayKetThuc,
                        v.SoLuong,
                        v.MoTa,
                        v.TrangThai
                    })
                    .ToList();

                return Json(vouchersKhachDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách voucher từ API");
                return Json(new List<object>());
            }
        }

        //Thêm nhanh khách hàng
        [HttpPost]
        public async Task<IActionResult> AddKhachHang([FromBody] KhachHangViewModel request)
        {
            try
            {
                if (request == null) return BadRequest();
                // Validate cơ bản
                if (string.IsNullOrWhiteSpace(request.Ten))
                    return Json(new { success = false, message = "Tên khách hàng không được để trống" });

                if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                    return Json(new { success = false, message = "Email không hợp lệ" });

                if (string.IsNullOrWhiteSpace(request.SDT) || request.SDT.Length < 10)
                    return Json(new { success = false, message = "Số điện thoại không hợp lệ" });

                if (string.IsNullOrWhiteSpace(request.DiaChiChiTiet))
                    return Json(new { success = false, message = "Địa chỉ chi tiết không được để trống" });

                // Gửi sang API - API sẽ lo generate MaKH, Password, tạo giỏ hàng, gửi mail
                var url = $"https://localhost:7095/api/KhachHang?isAdmin=true";
                var response = await _httpClient.PostAsJsonAsync(url, request);

                if (response.IsSuccessStatusCode)
                {
                    var khach = await response.Content.ReadFromJsonAsync<KhachHang>();
                    return Json(new
                    {
                        success = true,
                        message = "Thêm khách hàng thành công. Thông tin đăng nhập đã được gửi qua email.",
                        data = new { id = khach.IDKhachHang, ten = khach.Ten, sdt = khach.SDT, email = khach.Email }
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Lỗi từ API: {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        //Sửa khách hàng
        public async Task<IActionResult> UpdateKHinHD(string idkh, string idhd)
        {
            try
            {
                var qdd = await _httpClient.GetFromJsonAsync<List<QuyDoiDiem>>("QuyDoiDiem");
                var idqdd = qdd.FirstOrDefault(c => c.TrangThai != 0).ID;
                var checkexist = await _httpClient.GetFromJsonAsync<bool>($"HoaDon/CheckLSGDHD/{idhd}");
                if (checkexist == true) // Tồn tại-> sửa
                {
                    var lstd = await _httpClient.GetFromJsonAsync<LichSuTichDiem>($"HoaDon/LichSuGiaoDich/{idhd}");
                    string apiUrl = $"https://localhost:7095/api/LichSuTichDiem/{lstd.ID}?diem={lstd.Diem}&trangthai={lstd.TrangThai}&IdKhachHang={idkh}&IdQuyDoiDiem={lstd.IDQuyDoiDiem}&IdHoaDon={idhd}";
                    var response = await _httpClient.PutAsync(apiUrl, null);
                }
                else // Chưa có lstd-> tạo mới
                {
                    string apiUrl = $"https://localhost:7095/api/LichSuTichDiem?diem=0&trangthai=1&IdKhachHang={idkh}&IdQuyDoiDiem={idqdd}&IdHoaDon={idhd}";
                    var lstdresponse = await _httpClient.PostAsync(apiUrl, null);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }
        //Xóa khách hàng
        [HttpGet("/BanHangTaiQuay/DeleteKHinHD/{idhd}")]
        public async Task<IActionResult> DelefteKHinHD(string idhd)
        {
            try
            {
                var checkexist = await _httpClient.GetFromJsonAsync<bool>($"HoaDon/CheckLSGDHD/{idhd}");
                if (checkexist == true) // Tồn tại-> xóa
                {
                    var lstd = await _httpClient.GetFromJsonAsync<LichSuTichDiem>($"HoaDon/LichSuGiaoDich/{idhd}");
                    var response = await _httpClient.DeleteAsync($"LichSuTichDiem/{lstd.ID}");
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }
        //Tìm kiếm khách hàng
        [HttpGet("/BanHangTaiQuay/SearchKH/{keyword}")]
        public async Task<IActionResult> SearchKH(string keyword)
        {
            try
            {
                var lstkh = await _httpClient.GetFromJsonAsync<List<KhachHang>>("KhachHang");
                var distinctResult = lstkh
                                    .Where(c => c.Ten.ToLower().Contains(keyword.Trim().ToLower()) || (c.SDT != null && c.SDT.Contains(keyword.Trim())))
                                    .Distinct()
                                    .ToList();
                var result = new List<KhachHang>();
                if (distinctResult.Count < 3)
                {
                    var additionalItems = distinctResult.Take(result.Count).ToList();
                    result.AddRange(additionalItems);
                }
                result = distinctResult.Take(3).ToList();
                return Json(new { data = result });
            }
            catch (Exception ex)
            {
                return RedirectToAction("BanHang", "BanHangTaiQuay");
            }
        }
        [HttpGet("/BanHangTaiQuay/GetAllKhachHang")]
        public async Task<IActionResult> GetAllKhachHang()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<KhachHangViewModel>>("KhachHang/get-view-all");
                return Json(new { data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Không gọi được API Khách hàng", error = ex.Message });
            }
        }
        // AddHDCho
        [HttpPost]
        public async Task<IActionResult> AddHDCho(Guid idKhachHang, Guid idPhuongThucTT)
        {
            try
            {
                var listhdcho = await _httpClient.GetFromJsonAsync<List<HoaDon>>("HoaDon/GetAllHDCho");

                if (listhdcho.Count < 15)
                {
                    var loginInfor = new LoginViewModel();
                    string? session = HttpContext.Session.GetString("LoginInfor");
                    if (session != null)
                    {
                        loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                    }

                    var request = new HoaDonOfflineRequest
                    {
                        IdKhachHang = idKhachHang,
                        IdPhuongThucTT = idPhuongThucTT
                    };

                    var response = await _httpClient.PostAsJsonAsync($"HoaDon/Offline/{loginInfor.Id}", request);

                    listhdcho = await _httpClient.GetFromJsonAsync<List<HoaDon>>("HoaDon/GetAllHDCho");

                    return Json(new { success = true, data = listhdcho });
                }
                else
                {
                    return Json(new { success = false, message = "Hệ thống giới hạn 15 tab cho màn hình bán hàng" });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("BanHang", "BanHangTaiQuay");
            }
        }
        // Xóa hóa đơn
        [HttpDelete("/BanHangTaiQuay/deleteHd/{id}")]
        public async Task<ActionResult> deleteHd(String id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid guidId))
                {
                    return Json(new { success = false, message = "ID không hợp lệ" });
                }

                var response = await _httpClient.DeleteAsync($"HoaDon/deleteHoaDon/{guidId}");

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Xóa hóa đơn thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Xóa hóa đơn thất bại" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi deleteHd: {ex.Message}");
                return Json(new { success = false, message = "Lỗi hệ thống khi xóa hóa đơn" });
            }
        }
        //HÓA ĐƠN
        //Chuyển view hóa đơn
        [HttpGet("/BanHangTaiQuay/QuanLyHD")]
        public IActionResult QuanLyHD()
        {
            return PartialView("_QuanLyHoaDon");
        }
        //Tam
        public IActionResult ScanQRCode()
        {
            return PartialView("ScanQRCode");
        }
        //End
    }
}
