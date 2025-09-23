using AppData.Models;
using AppData.ViewModels;
using AppView.PhanTrang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Claims;

namespace AppView.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly HttpClient httpClients;

        public KhachHangController()
        {
            httpClients = new HttpClient();
        }

        public int PageSize = 10;

        // Get All KH - Sử dụng API endpoint mới
        public async Task<IActionResult> GetAllKhachHang(int ProductPage = 1)
        {
            string apiUrl = "https://localhost:7095/api/KhachHang/get-view-all";
            var response = await httpClients.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            var kh = JsonConvert.DeserializeObject<List<KhachHangViewModel>>(apiData);

            return View(new PhanTrangKhachHang
            {
                listkh = kh.Skip((ProductPage - 1) * PageSize).Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    ItemsPerPage = PageSize,
                    CurrentPage = ProductPage,
                    TotalItems = kh.Count()
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLSTDByIDKH(Guid id)
        {
            string apiURL = $"https://localhost:7095/api/LichSuTichDiem/TongDonThanhCong?id={id}";
            var response = await httpClients.GetAsync(apiURL);
            var apiData = await response.Content.ReadAsStringAsync();
            var DonThanhCong = JsonConvert.DeserializeObject<TongDon?>(apiData);
            ViewBag.DonThanhCong = DonThanhCong;

            string apiURL1 = $"https://localhost:7095/api/LichSuTichDiem/TongDonHuy?id={id}";
            var response1 = await httpClients.GetAsync(apiURL1);
            var apiData1 = await response1.Content.ReadAsStringAsync();
            var DonHuy = JsonConvert.DeserializeObject<TongDon?>(apiData1);
            ViewBag.DonHuy = DonHuy;

            string apiURL2 = $"https://localhost:7095/api/LichSuTichDiem/TongDonHoanHang?id={id}";
            var response2 = await httpClients.GetAsync(apiURL2);
            var apiData2 = await response2.Content.ReadAsStringAsync();
            var DonHoanHang = JsonConvert.DeserializeObject<TongDon?>(apiData2);
            ViewBag.DonHoanHang = DonHoanHang;

            HttpContext.Session.SetString("DonKH", id.ToString());
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DonThanhCong(int ProductPage = 1)
        {
            var id = Guid.Parse(HttpContext.Session.GetString("DonKH"));
            string apiURL2 = $"https://localhost:7095/api/LichSuTichDiem/ListDonThanhCong?id={id}";
            var response2 = await httpClients.GetAsync(apiURL2);
            var apiData2 = await response2.Content.ReadAsStringAsync();
            var Don = JsonConvert.DeserializeObject<List<ListDon>>(apiData2);

            return View(new PhanTrangDon
            {
                listdon = Don.Skip((ProductPage - 1) * PageSize).Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    ItemsPerPage = PageSize,
                    CurrentPage = ProductPage,
                    TotalItems = Don.Count()
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> DonHuy(int ProductPage = 1)
        {
            var id = Guid.Parse(HttpContext.Session.GetString("DonKH"));
            string apiURL2 = $"https://localhost:7095/api/LichSuTichDiem/ListDonHuy?id={id}";
            var response2 = await httpClients.GetAsync(apiURL2);
            var apiData2 = await response2.Content.ReadAsStringAsync();
            var Don = JsonConvert.DeserializeObject<List<ListDon>>(apiData2);

            return View(new PhanTrangDon
            {
                listdon = Don.Skip((ProductPage - 1) * PageSize).Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    ItemsPerPage = PageSize,
                    CurrentPage = ProductPage,
                    TotalItems = Don.Count()
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> DonHoanHang(int ProductPage = 1)
        {
            var id = Guid.Parse(HttpContext.Session.GetString("DonKH"));
            string apiURL2 = $"https://localhost:7095/api/LichSuTichDiem/ListDonHoanHang?id={id}";
            var response2 = await httpClients.GetAsync(apiURL2);
            var apiData2 = await response2.Content.ReadAsStringAsync();
            var Don = JsonConvert.DeserializeObject<List<ListDon>>(apiData2);

            return View(new PhanTrangDon
            {
                listdon = Don.Skip((ProductPage - 1) * PageSize).Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    ItemsPerPage = PageSize,
                    CurrentPage = ProductPage,
                    TotalItems = Don.Count()
                }
            });
        }
        public IActionResult AddDiaChi()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // hoặc cách bạn truyền ID
            var model = new LoginViewModel { Id = Guid.Parse(userId) };
            return View(model);
        }


        // Tìm kiếm KH theo Ten hoặc SDT
        [HttpGet]
        public async Task<IActionResult> GetAllKHTheoTimKiem(string? Ten, string? SDT, string? SortOrder, int ProductPage = 1)
        {
            string apiUrl = $"https://localhost:7095/api/KhachHang/TimKiemKH?Ten={Ten?.Trim()}&SDT={SDT?.Trim()}";
            var response = await httpClients.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            var kh = JsonConvert.DeserializeObject<List<KhachHang>>(apiData);

            // Convert sang KhachHangViewModel để hiển thị
            var khViewModel = kh.Select(x => new KhachHangViewModel
            {
                Id = x.IDKhachHang,
                MaKhachHang = x.MaKhachHang,
                Ten = x.Ten,
                Email = x.Email,
                SDT = x.SDT,
                NgaySinh = x.NgaySinh?.ToString("yyyy-MM-dd"),
                GioiTinh = x.GioiTinh,
                TrangThai = x.TrangThai,
                DiemTich = x.DiemTich
            }).ToList();
			switch (SortOrder)
			{
				case "ten_asc":
					khViewModel = khViewModel.OrderBy(x => x.Ten).ToList();
					break;
				case "ten_desc":
					khViewModel = khViewModel.OrderByDescending(x => x.Ten).ToList();
					break;
				default:
					break;
			}
			return View("GetAllKhachHang", new PhanTrangKhachHang
            {
                listkh = khViewModel.Skip((ProductPage - 1) * PageSize).Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    ItemsPerPage = PageSize,
                    CurrentPage = ProductPage,
                    TotalItems = khViewModel.Count()
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(KhachHangViewModel kh)
        {
            try
            {
                // Validation cơ bản
                if (string.IsNullOrWhiteSpace(kh.Ten))
                {
                    ViewBag.Error = "Tên khách hàng không được để trống";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(kh.Email) || !kh.Email.Contains("@"))
                {
                    ViewBag.Error = "Email không hợp lệ";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(kh.SDT) || kh.SDT.Length < 10)
                {
                    ViewBag.Error = "Số điện thoại không hợp lệ";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(kh.DiaChiChiTiet))
                {
                    ViewBag.Error = "Địa chỉ chi tiết không được để trống";
                    return View();
                }

                // Gửi sang API - Không cần password và mã KH vì API tự tạo
                var response = await httpClients.PostAsJsonAsync("https://localhost:7095/api/KhachHang?isAdmin=true", kh);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    ViewBag.Success = "Tạo khách hàng thành công. Thông tin đăng nhập đã được gửi qua email.";
                    return RedirectToAction("GetAllKhachHang");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Lỗi: {errorContent}";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Có lỗi xảy ra: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            string apiUrl = "https://localhost:7095/api/KhachHang/GetById?id=" + id;
            var response = await httpClients.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            var khach = JsonConvert.DeserializeObject<KhachHang>(apiData);

            // Convert sang ViewModel để hiển thị
            var khViewModel = new KhachHangViewModel
            {
                Id = khach.IDKhachHang,
                MaKhachHang = khach.MaKhachHang,
                Ten = khach.Ten,
                Email = khach.Email,
                SDT = khach.SDT,
                NgaySinh = khach.NgaySinh?.ToString("yyyy-MM-dd"),
                GioiTinh = khach.GioiTinh,
                TrangThai = khach.TrangThai,
                DiemTich = khach.DiemTich
            };

            return View(khViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Updates(Guid id)
        {
            // Lấy thông tin từ API endpoint get-view-all để có đầy đủ thông tin địa chỉ
            string apiUrl = "https://localhost:7095/api/KhachHang/get-view-all";
            var response = await httpClients.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            var allKH = JsonConvert.DeserializeObject<List<KhachHangViewModel>>(apiData);

            var kh = allKH.FirstOrDefault(x => x.Id == id);
            if (kh == null)
            {
                return NotFound();
            }

            return View(kh);
        }

        [HttpPost]
        public async Task<IActionResult> Updates(KhachHangViewModel kh)
        {
            try
            {
                // Validation cơ bản
                if (string.IsNullOrWhiteSpace(kh.Ten))
                {
                    ViewBag.Error = "Tên khách hàng không được để trống";
                    return View(kh);
                }

                if (string.IsNullOrWhiteSpace(kh.Email) || !kh.Email.Contains("@"))
                {
                    ViewBag.Error = "Email không hợp lệ";
                    return View(kh);
                }

                if (string.IsNullOrWhiteSpace(kh.SDT) || kh.SDT.Length < 10)
                {
                    ViewBag.Error = "Số điện thoại không hợp lệ";
                    return View(kh);
                }

                var url = "https://localhost:7095/api/KhachHang/PutKhView";
                var response = await httpClients.PutAsJsonAsync(url, kh);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("GetAllKhachHang");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Cập nhật thất bại: {errorContent}";
                    return View(kh);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Có lỗi xảy ra: {ex.Message}";
                return View(kh);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var url = $"https://localhost:7095/api/KhachHang/{id}";
                var response = await httpClients.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("GetAllKhachHang");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Xóa thất bại: {errorContent}";
                    return RedirectToAction("GetAllKhachHang");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("GetAllKhachHang");
            }
        }
    }
}