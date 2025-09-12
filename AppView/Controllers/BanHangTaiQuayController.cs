using AppData.Models;
using AppData.ViewModels;
using AppData.ViewModels.BanOffline;
using AppData.ViewModels.SanPham;
using AppData.ViewModels.VNPay;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace AppView.Controllers
{
    public class BanHangTaiQuayController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BanHangTaiQuayController> _logger;

        public BanHangTaiQuayController(IHttpClientFactory httpClientFactory, ILogger<BanHangTaiQuayController> logger)
        {

            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
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
                // ID cố định hoặc từ config
                var ID_TIEN_MAT = Guid.Parse("9b6289bf-5e83-419a-94d5-926abb264961");
                var ID_BANKING = Guid.Parse("f29cd85d-0251-4b50-8867-6a88891417f6");

                // Lấy thông tin đăng nhập từ Session nếu request không có
                if (request == null)
                {
                    return Json(new { success = false, message = "Request null" });
                }

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

                if (request.IDPhuongThucThanhToan == ID_TIEN_MAT)
                {
                    var hdrequest = new HoaDonThanhToanRequest()
                    {
                        Id = request.Id,
                        IdNhanVien = request.IdNhanVien,   // ✅ Lấy từ Session
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

                    // ✅ Thanh toán tiền mặt: gọi API nội bộ như hiện tại
                    var response = await _httpClient.PutAsJsonAsync("HoaDon/UpdateHoaDon/", hdrequest);
                    if (response.IsSuccessStatusCode)
                    {
                        // Gọi xoá hóa đơn chờ nếu cần
                        await _httpClient.DeleteAsync($"HoaDon/DeleteHoaDonCho?idHoaDon={hdrequest.Id}");
                        return Json(new { success = true, message = "Thanh toán thành công" });
                    }

                    return Json(new { success = false, message = "Thanh toán thất bại" });
                }
                else if (request.IDPhuongThucThanhToan == ID_BANKING)
                {
                    TempData["HoaDonVNPay"] = JsonConvert.SerializeObject(request);

                    var order = new OrderInfo
                    {
                        OrderId = DateTime.Now.Ticks,
                        Amount = (long)request.TongTien,
                        Status = "0",
                        CreatedDate = DateTime.Now
                    };

                    string returnUrl = "https://localhost:5001/BanHang/PaymentCallBack";
                    string url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
                    string tmnCode = "RZZISK72";
                    string hashSecret = "1MGOZCCX72BAUIO5JUD5XV0O1KWEULNC";
                    string ip = HttpContext.Connection.RemoteIpAddress?.ToString();

                    var vnpay = new VnPayLibrary();
                    vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                    vnpay.AddRequestData("vnp_Command", "pay");
                    vnpay.AddRequestData("vnp_TmnCode", tmnCode);
                    vnpay.AddRequestData("vnp_Amount", ((long)(order.Amount * 100)).ToString()); // ép kiểu 
                    vnpay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
                    vnpay.AddRequestData("vnp_CurrCode", "VND");
                    vnpay.AddRequestData("vnp_IpAddr", ip);
                    vnpay.AddRequestData("vnp_Locale", "vn");
                    vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang:{order.OrderId}");
                    vnpay.AddRequestData("vnp_OrderType", "other");
                    vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
                    vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString());

                    string paymentUrl = vnpay.CreateRequestUrl(url, hashSecret);
                    Console.WriteLine("VNPay URL: " + paymentUrl); // hoặc log ra file/log
                    return Json(new { Success = true, PaymentUrl = paymentUrl });
                }
                return Json(new { success = false, message = "Phương thức không hỗ trợ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            try
            {
                if (Request.Query.Count == 0) return BadRequest("Thiếu dữ liệu từ VNPay");

                string vnp_HashSecret = "1MGOZCCX72BAUIO5JUD5XV0O1KWEULNC";
                var vnpayData = Request.Query;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (var s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s.Key, vnpayData[s.Key]);
                    }
                }

                // Lấy thông tin đăng nhập từ Session
                var loginInfor = new LoginViewModel();
                string? session = HttpContext.Session.GetString("LoginInfor");
                if (session != null)
                {
                    loginInfor = JsonConvert.DeserializeObject<LoginViewModel>(session);
                }

                long orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string vnp_SecureHash = Request.Query["vnp_SecureHash"];

                bool isValidSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (!isValidSignature)
                    return Content("Sai chữ ký hash của VNPay.");

                if (vnp_ResponseCode != "00" || vnp_TransactionStatus != "00")
                    return Content("Thanh toán thất bại. VNPay trả về lỗi.");

                if (!TempData.ContainsKey("HoaDonVNPay"))
                    return Content("Không tìm thấy thông tin hóa đơn trong TempData.");

                var request = JsonConvert.DeserializeObject<HoaDonThanhToanRequest>(TempData["HoaDonVNPay"]!.ToString());

                var hdrequest = new HoaDonThanhToanRequest
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
                    TrangThai = 6
                };

                var response = await _httpClient.PutAsJsonAsync("HoaDon/UpdateHoaDon", hdrequest);
                if (response.IsSuccessStatusCode)
                {
                    // Gọi xoá hóa đơn chờ nếu cần
                    await _httpClient.DeleteAsync($"HoaDon/DeleteHoaDonCho?idHoaDon={hdrequest.Id}");

                    return Content("Thanh toán thành công. Hóa đơn đã được cập nhật.");
                }
                else
                {
                    return Content("❌ Giao dịch VNPay thành công nhưng cập nhật hóa đơn thất bại. Vui lòng xử lý thủ công!.");
                }
            }
            catch (Exception ex)
            {
                return Content("Lỗi callback: " + ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetVouchers(decimal tongTien)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Voucher/GetAllHoaDonTaiQuay?tongTien={tongTien}");

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không lấy được danh sách voucher."
                    });
                }

                var json = await response.Content.ReadAsStringAsync();
                var vouchers = JsonConvert.DeserializeObject<List<Voucher>>(json);

                // Lọc voucher chỉ lấy những cái khả dụng
                var now = DateTime.Now;
                var vouchersKhachDung = vouchers.Where(v =>
                    v.TrangThai == 1 &&
                    v.SoLuong > 0 &&
                    now >= v.NgayApDung &&
                    now <= v.NgayKetThuc &&
                    tongTien >= v.GiaTriToiThieu
                ).ToList();

                return Json(vouchersKhachDung); // Trả về danh sách voucher trực tiếp
            }
            catch (Exception ex)
            {
                return Json(new List<Voucher>()); // Trả về danh sách rỗng nếu có lỗi
            }
        }
        //Thêm nhanh khách hàng
        [HttpPost]
        public async Task<IActionResult> AddKhachHang(KhachHang request)
        {
            try
            {
                KhachHang khview = new KhachHang();
                khview.IDKhachHang = Guid.NewGuid();
                khview.SDT = request.SDT;
                khview.Email = request.Email;
                khview.Ten = request.Ten;
                khview.NgaySinh = request.NgaySinh;
                khview.GioiTinh = request.GioiTinh;
                khview.Password = khview.MaKhachHang.ToString().Substring(0, 8);
                khview.TrangThai = 1;
                khview.DiemTich = 0;
                var lstkh = await _httpClient.GetFromJsonAsync<List<KhachHang>>("KhachHang");
                if (request.SDT != null && lstkh.Any(c => c.SDT != null && c.SDT.Trim().Equals(request.SDT.Trim())))
                {
                    return Json(new { success = false, message = "Số điện thoại đã được sử dụng" });
                }
                if (request.Email != null && lstkh.Any(c => c.Email != null && c.Email.Trim().Equals(request.Email.Trim())))
                {
                    return Json(new { success = false, message = "Email đã được sử dụng" });
                }
                else
                {
                    var url = $"https://localhost:7095/api/QuanLyNguoiDung/AddNhanhKH";
                    var response = await _httpClient.PostAsJsonAsync(url, khview);
                    if (response.IsSuccessStatusCode) // Thêm khách hàng thành công -> tạo lịch sử tích điểm
                    {
                        var qdd = await _httpClient.GetFromJsonAsync<List<QuyDoiDiem>>("QuyDoiDiem");
                        var idqdd = qdd.FirstOrDefault(c => c.TrangThai != 0).ID;
                        var kh = new KhachHang();
                        if (request.SDT != null)
                        {
                            kh = await _httpClient.GetFromJsonAsync<KhachHang>($"KhachHang/getBySDT?sdt={request.SDT}");
                        }
                        else if (request.Email != null)
                        {
                            kh = await _httpClient.GetFromJsonAsync<KhachHang>($"KhachHang/getBySDT?sdt={request.Email}");
                        }

                        var IDHD = request.MaKhachHang; // Luu tam idhd qua idkh
                                                        // ktra hd đã có lstd 
                        var checkexist = await _httpClient.GetFromJsonAsync<bool>($"HoaDon/CheckLSGDHD/{IDHD}");
                        if (checkexist == true) // Tồn tại-> xóa
                        {
                            var lstdexist = await _httpClient.GetFromJsonAsync<LichSuTichDiem>($"HoaDon/LichSuGiaoDich/{IDHD}");
                            var deletelstd = await _httpClient.DeleteAsync($"LichSuTichDiem/{lstdexist.ID}");
                        }
                        string apiUrl = $"https://localhost:7095/api/LichSuTichDiem?diem=0&trangthai=1&IdKhachHang={kh.MaKhachHang}&IdQuyDoiDiem={idqdd}&IdHoaDon={IDHD}";
                        var lstdresponse = await _httpClient.PostAsync(apiUrl, null);
                        return Json(new { success = true, message = "Thêm khách hàng thành công" });

                    }
                }
                return Json(new { success = false, message = "Thêm khách hàng thất bại" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Thêm khách hàng thất bại" });
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
