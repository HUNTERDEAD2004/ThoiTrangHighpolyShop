using AppData.Models;
using AppData.ViewModels;
using AppData.ViewModels.ThongKe;
using AppView.PhanTrang;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using AppView.Models;

namespace AppView.Controllers
{
    public class VouchersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly AssignmentDBContext dBContext;
        public VouchersController()
        {
            _httpClient = new HttpClient();
            dBContext=new AssignmentDBContext();
        }
        public int PageSize = 8;
        
        // Two-column: Create voucher + customer list
        [HttpGet]
        public async Task<IActionResult> CreateWithCustomers()
        {
            // load customers
            var customersRes = await _httpClient.GetAsync("https://localhost:7095/api/KhachHang/get-view-all");
            var customersJson = await customersRes.Content.ReadAsStringAsync();
            var customers = JsonConvert.DeserializeObject<List<KhachHangViewModel>>(customersJson) ?? new List<KhachHangViewModel>();

            var vm = new VoucherCustomerVM
            {
                VoucherForm = new VoucherView
                {
                    NgayApDung = DateTime.Now,
                    NgayKetThuc = DateTime.Now.AddDays(7)
                },
                Customers = customers
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWithCustomers(VoucherCustomerVM vm, string DiscountUnit, string Visibility)
        {
            // Validation tùy chỉnh
            if (string.IsNullOrEmpty(vm.VoucherForm.MaVoucher))
            {
                ModelState.AddModelError("VoucherForm.MaVoucher", "Mã voucher không được để trống");
            }
            
            if (string.IsNullOrEmpty(vm.VoucherForm.Ten))
            {
                ModelState.AddModelError("VoucherForm.Ten", "Tên voucher không được để trống");
            }

            if (vm.VoucherForm.GiaTri <= 0)
            {
                ModelState.AddModelError("VoucherForm.GiaTri", "Giá trị giảm phải lớn hơn 0");
            }

            // Validation cho đơn vị giảm giá
            if (DiscountUnit == "percent" && vm.VoucherForm.GiaTri > 100)
            {
                ModelState.AddModelError("VoucherForm.GiaTri", "Giá trị giảm theo % không được vượt quá 100%");
            }

            if (vm.VoucherForm.SoLuong <= 0)
            {
                ModelState.AddModelError("VoucherForm.SoLuong", "Số lượng phải lớn hơn 0");
            }

            if (vm.VoucherForm.GiaTriToiThieu < 0)
            {
                ModelState.AddModelError("VoucherForm.GiaTriToiThieu", "Đơn giá tối thiểu không được âm");
            }

            if (vm.VoucherForm.GiaTriToiDa < vm.VoucherForm.GiaTri)
            {
                ModelState.AddModelError("VoucherForm.GiaTriToiDa", "Giảm tối đa phải lớn hơn hoặc bằng giá trị giảm");
            }

            if (vm.VoucherForm.NgayKetThuc <= vm.VoucherForm.NgayApDung)
            {
                ModelState.AddModelError("VoucherForm.NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu");
            }

            // Cập nhật HinhThucGiamGia dựa trên đơn vị
            vm.VoucherForm.HinhThucGiamGia = DiscountUnit == "percent" ? 1 : 0;
            // Gán chế độ hiển thị
            vm.VoucherForm.IsPublic = Visibility == "public";

            // Kiểm tra mã voucher đã tồn tại (server-side) trước khi Post
            if (!string.IsNullOrWhiteSpace(vm.VoucherForm.MaVoucher))
            {
                var existsRes = await _httpClient.GetAsync($"https://localhost:7095/api/Voucher/check-exists?maVoucher={Uri.EscapeDataString(vm.VoucherForm.MaVoucher)}");
                if (existsRes.IsSuccessStatusCode)
                {
                    var existsJson = await existsRes.Content.ReadAsStringAsync();
                    var existsObj = JsonConvert.DeserializeObject<dynamic>(existsJson);
                    bool exists = existsObj?.exists == true;
                    if (exists)
                    {
                        ModelState.AddModelError("VoucherForm.MaVoucher", "Mã voucher đã tồn tại");
                        ViewBag.VoucherExists = true;
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                // reload customers when validation fails
                var customersRes = await _httpClient.GetAsync("https://localhost:7095/api/KhachHang/get-view-all");
                var customersJson = await customersRes.Content.ReadAsStringAsync();
                vm.Customers = JsonConvert.DeserializeObject<List<KhachHangViewModel>>(customersJson) ?? new List<KhachHangViewModel>();
                return View(vm);
            }

            var response = await _httpClient.PostAsJsonAsync("https://localhost:7095/api/Voucher", vm.VoucherForm);
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError("VoucherForm.MaVoucher", "Mã voucher đã tồn tại");
                var customersResDup = await _httpClient.GetAsync("https://localhost:7095/api/KhachHang/get-view-all");
                var customersJsonDup = await customersResDup.Content.ReadAsStringAsync();
                vm.Customers = JsonConvert.DeserializeObject<List<KhachHangViewModel>>(customersJsonDup) ?? new List<KhachHangViewModel>();
                return View(vm);
            }
            if (response.IsSuccessStatusCode)
            {
                var voucherId = vm.VoucherForm.Id;
                
                // Xử lý logic phân phối voucher dựa trên loại hiển thị
                if (Visibility == "public")
                {
                    // Voucher công khai - gán cho tất cả khách hàng
                    var allCustomersRes = await _httpClient.GetAsync("https://localhost:7095/api/KhachHang/get-view-all");
                    var allCustomersJson = await allCustomersRes.Content.ReadAsStringAsync();
                    var allCustomers = JsonConvert.DeserializeObject<List<KhachHangViewModel>>(allCustomersJson) ?? new List<KhachHangViewModel>();
                    
                    if (allCustomers.Any())
                    {
                        var allCustomerIds = allCustomers.Select(c => c.Id).ToList();
                        await _httpClient.PostAsJsonAsync("https://localhost:7095/api/UserVoucher/assign", new
                        {
                            VoucherId = voucherId,
                            CustomerIds = allCustomerIds
                        });
                    }
                }
                else
                {
                    // Voucher riêng tư - chỉ gán cho khách hàng được chọn
                    if (voucherId != Guid.Empty && vm.SelectedCustomerIds != null && vm.SelectedCustomerIds.Count > 0)
                    {
                        await _httpClient.PostAsJsonAsync("https://localhost:7095/api/UserVoucher/assign", new
                        {
                            VoucherId = voucherId,
                            CustomerIds = vm.SelectedCustomerIds
                        });
                    }
                }
                
                return RedirectToAction("GetAllVoucher");
            }

            // Post failed, reload customers and show view
            var customersRes2 = await _httpClient.GetAsync("https://localhost:7095/api/KhachHang/get-view-all");
            var customersJson2 = await customersRes2.Content.ReadAsStringAsync();
            vm.Customers = JsonConvert.DeserializeObject<List<KhachHangViewModel>>(customersJson2) ?? new List<KhachHangViewModel>();
            return View(vm);
        }
        // get all vocher
        [HttpGet]
        public async Task<IActionResult> GetAllVoucher(int ProductPage = 1)
        {
            string apiURL = $"https://localhost:7095/api/Voucher";
            var response = await _httpClient.GetAsync(apiURL);
            var apiData = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<VoucherView>>(apiData);
            return View(new PhanTrangVouchers
            {
                listvouchers = roles
                        .Skip((ProductPage - 1) * PageSize).Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    ItemsPerPage = PageSize,
                    CurrentPage = ProductPage,
                    TotalItems = roles.Count()
                }

            }
                );

        }
        // tim kiem ten
        [HttpGet]
        public async Task<IActionResult> TimKiemTenVC(string Ten, int ProductPage = 1)
        {
            string apiURL = $"https://localhost:7095/api/Voucher";
            var response = await _httpClient.GetAsync(apiURL);
            var apiData = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<VoucherView>>(apiData);
            return View("GetAllVoucher", new PhanTrangVouchers
            {
                listvouchers = roles.Where(x => x.Ten.Contains(Ten.Trim()))
                        .Skip((ProductPage - 1) * PageSize).Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    ItemsPerPage = PageSize,
                    CurrentPage = ProductPage,
                    TotalItems = roles.Count()
                }

            }
                );

        }
        // create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(VoucherView voucher)
        {
            try
            {
                string apiURL = $"https://localhost:7095/api/Voucher";
                var response1 = await _httpClient.GetAsync(apiURL);
                var apiData = await response1.Content.ReadAsStringAsync();
                var roles = JsonConvert.DeserializeObject<List<VoucherView>>(apiData);

                if (voucher.SoTienCan != null || voucher.Ten != null || voucher.GiaTri != null || voucher.HinhThucGiamGia != null || voucher.TrangThai != null || voucher.NgayApDung != null || voucher.NgayKetThuc != null)
                {

                    if (voucher.SoTienCan < 0)
                    {
                        ViewData["SoTienCan"] = "Số tiền cần không được âm ";
                    }
                    if (voucher.GiaTri <= 100000)
                    {
                        ViewData["GiaTri"] = "Mời bạn nhập giá trị lớn hơn 100000";
                    }
                    if (voucher.SoLuong <= 0)
                    {
                        ViewData["SoLuong"] = "Mời bạn nhập số lượng lớn hơn 0";
                    }
                    if (voucher.NgayKetThuc < voucher.NgayApDung)
                    {
                        ViewData["Ngay"] = "Ngày kết thúc phải lớn hơn ngày áp dụng";
                    }
                    var timkiem = roles.FirstOrDefault(x => x.Ten == voucher.Ten.Trim());
                   
                    if (timkiem != null)
                    {
                        ViewData["Ma"] = "Mã này đã tồn tại";

                    }

                    if (voucher.HinhThucGiamGia == 1)
                    {
                        if (voucher.SoTienCan == 0)
                        {
                            if (voucher.GiaTri > 50 || voucher.GiaTri <= 10)
                            {
                                ViewData["GiaTri"] = "Giá trị từ 10 đến 50";
                                return View();
                            }
                            if (voucher.GiaTri <= 50 && voucher.GiaTri > 0)
                            {
                                if (voucher.SoTienCan >= 0 && voucher.GiaTri > 0 && voucher.SoLuong > 0 && voucher.NgayKetThuc >= voucher.NgayApDung && timkiem == null)
                                {
                                    var response = await _httpClient.PostAsJsonAsync($"https://localhost:7095/api/Voucher", voucher);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        return RedirectToAction("GetAllVoucher");
                                    }
                                    return View();
                                }
                            }
                        }
                        if (voucher.SoTienCan > 0)
                        {
                            if (voucher.GiaTri <= voucher.SoTienCan)
                            {
                                if (voucher.GiaTri <= 100 && voucher.GiaTri > 0)
                                {
                                    if (voucher.SoTienCan >= 0 && voucher.GiaTri > 0 && voucher.SoLuong > 0 && voucher.NgayKetThuc >= voucher.NgayApDung && timkiem == null)
                                    {
                                        var response = await _httpClient.PostAsJsonAsync($"https://localhost:7095/api/Voucher", voucher);
                                        if (response.IsSuccessStatusCode)
                                        {
                                            return RedirectToAction("GetAllVoucher");
                                        }
                                        return View();
                                    }
                                }
                                if (voucher.GiaTri > 100 || voucher.GiaTri <= 0)
                                {
                                    ViewData["GiaTri"] = "Giá trị từ 1 đến 100";
                                    return View();
                                }

                            }
                            if (voucher.GiaTri > voucher.SoTienCan)
                            {
                                ViewData["GiaTri"] = "Giá trị phải nhỏ hơn hoặc bằng số tiền cần";
                                return View();

                            }
                        }


                    }
                    if (voucher.HinhThucGiamGia == 0)
                    {
                        if (voucher.SoTienCan == 0)
                        {
                            if (voucher.GiaTri > voucher.GiaTriToiDa || voucher.GiaTri > voucher.GiaTriToiThieu)
                            {
                                ViewData["GiaTri"] = "Nhập lại giá trị Giảm";
                                return View();
                            }
                            if (voucher.GiaTri > 0)
                            {
                                if (voucher.SoTienCan >= 0 && voucher.GiaTri > 0 && voucher.SoLuong > 0 && voucher.NgayKetThuc >= voucher.NgayApDung && timkiem == null)
                                {
                                    var response = await _httpClient.PostAsJsonAsync($"https://localhost:7095/api/Voucher", voucher);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        return RedirectToAction("GetAllVoucher");
                                    }
                                    return View();
                                }
                            }
                        }
                        if (voucher.SoTienCan > 0)
                        {
                            if (voucher.GiaTri <= voucher.SoTienCan)
                            {
                                if (voucher.GiaTri <= 0)
                                {
                                    ViewData["GiaTri"] = "Giá trị phải lớn hơn 0";
                                    return View();
                                }
                                if (voucher.GiaTri > 0)
                                {
                                    if (voucher.SoTienCan >= 0 && voucher.GiaTri > 0 && voucher.SoLuong > 0 && voucher.NgayKetThuc >= voucher.NgayApDung && timkiem == null)
                                    {
                                        var response = await _httpClient.PostAsJsonAsync($"https://localhost:7095/api/Voucher", voucher);
                                        if (response.IsSuccessStatusCode)
                                        {
                                            return RedirectToAction("GetAllVoucher");
                                        }
                                        return View();
                                    }
                                }
                            }
                            if (voucher.GiaTri > voucher.SoTienCan)
                            {
                                ViewData["GiaTri"] = "Giá trị phải nhỏ hơn hoặc bằng số tiền cần";
                                return View();
                            }

                        }
                    }


                }

                return View();
            }
            catch
            {
                return View();
            }

        }



        [HttpGet]
        public IActionResult Updates(Guid id)
        {
            try
            {
                var url = $"https://localhost:7095/api/Voucher/{id}";
                var response = _httpClient.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không tìm thấy voucher.";
                    return RedirectToAction("GetAllVoucher");
                }

                var result = response.Content.ReadAsStringAsync().Result;
                var voucher = JsonConvert.DeserializeObject<VoucherView>(result);

                return View(voucher ?? new VoucherView());
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải dữ liệu voucher.";
                return RedirectToAction("GetAllVoucher");
            }
        }


        // POST: Vouchers/Updates
        [HttpPost]
        public async Task<IActionResult> Updates(VoucherView voucher)
        {
            if (!ModelState.IsValid) return View(voucher);

            // Validate ngày & số lượng
            if (voucher.NgayKetThuc < voucher.NgayApDung)
            {
                ViewData["Ngay"] = "Ngày kết thúc phải lớn hơn ngày áp dụng";
                return View(voucher);
            }
            if (voucher.SoLuong <= 0)
            {
                ViewData["SoLuong"] = "Số lượng phải > 0";
                return View(voucher);
            }

            // Gửi PUT kèm cả Id
            var response = await _httpClient.PutAsJsonAsync(
                $"https://localhost:7095/api/Voucher/{voucher.Id}", voucher);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cập nhật voucher thành công!";
                return RedirectToAction("GetAllVoucher");
            }

            // Debug lỗi trả về từ API
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Không thể cập nhật voucher. Chi tiết: {error}");
            return View(voucher);
        }
        // delete
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                string apiURL = $"https://localhost:7095/api/Voucher/{id}";

                var response = await _httpClient.DeleteAsync(apiURL);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Xóa voucher thành công.";
                    return RedirectToAction("GetAllVoucher");
                }

                TempData["Error"] = "Không thể xóa voucher. Vui lòng thử lại.";
                return RedirectToAction("GetAllVoucher");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi trong quá trình xóa: " + ex.Message;
                return RedirectToAction("GetAllVoucher");
            }
        }
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                string apiURL = $"https://localhost:7095/api/Voucher/{id}";
                var response = await _httpClient.GetAsync(apiURL);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không tìm thấy voucher.";
                    return RedirectToAction("GetAllVoucher");
                }

                var jsonData = await response.Content.ReadAsStringAsync();
                var voucher = JsonConvert.DeserializeObject<VoucherView>(jsonData);

                return View(voucher);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi lấy thông tin voucher: " + ex.Message;
                return RedirectToAction("GetAllVoucher");
            }
        }
        public async Task<IActionResult> SuDung(Guid id)
        {
            try
            {
                var timkiem = dBContext.Vouchers.FirstOrDefault(x => x.ID == id);
                if (timkiem != null)
                {
                    timkiem.TrangThai = 1;
                    dBContext.Vouchers.Update(timkiem);
                    dBContext.SaveChanges();
                    return RedirectToAction("GetAllVoucher");
                }
                else
                {
                    return View();
                }
            }
            catch
            {
                return View();
            }
           
        }
        public async Task<IActionResult> KoSuDung(Guid id)
        {
            try
            {
                var timkiem = dBContext.Vouchers.FirstOrDefault(x => x.ID == id);
                if (timkiem != null)
                {
                    timkiem.TrangThai = 0;
                    dBContext.Vouchers.Update(timkiem);
                    dBContext.SaveChanges();
                    return RedirectToAction("GetAllVoucher");
                }
                else
                {
                    return View();
                }
            }
            catch
            {
                return View();
            }
            
        }
    }
}
