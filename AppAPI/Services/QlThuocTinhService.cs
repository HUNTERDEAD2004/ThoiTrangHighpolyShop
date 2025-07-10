using System.Globalization;
using System.Text;
using AppAPI.IServices;
using AppData.Models;
using Microsoft.EntityFrameworkCore;

namespace AppAPI.Services
{
    public class QlThuocTinhService : IQlThuocTinhService
    {
        private readonly
        AssignmentDBContext _dbContext;
        public QlThuocTinhService()
        {
            _dbContext = new AssignmentDBContext();
        }
        #region KichCo
        public async Task<KichCo> AddKichCo(string ten, int trangthai)
        {
            try
            {
                var existingColor = await _dbContext.KichCos.FirstOrDefaultAsync(x => x.Ten.Trim().ToUpper() == ten.Trim().ToUpper());
                if (existingColor != null)
                {
                    return null;
                }
                KichCo kc = new KichCo()
                {
                    ID = Guid.NewGuid(),
                    Ten = ten,
                    TrangThai = 1
                };
                _dbContext.KichCos.Add(kc);
                _dbContext.SaveChanges();
                return kc;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> DeleteKichCo(Guid id)
        {
            try
            {
                var nv = await _dbContext.KichCos.FirstOrDefaultAsync(nv => nv.ID == id);
                if (nv != null)
                {
                    _dbContext.KichCos.Remove(nv);
                    _dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception) { throw; }
        }
        public async Task<KichCo> UpdateKichCo(Guid id, string ten, int trangthai)
        {
            try
            {
                var nv = await _dbContext.KichCos.FirstOrDefaultAsync(x => x.ID == id);
                if (nv != null)
                {
                    var existingColor = await _dbContext.KichCos.FirstOrDefaultAsync(x => x.Ten.Trim().ToUpper() == ten.Trim().ToUpper());
                    if (existingColor != null)
                    {
                        return null; // Trả về null để báo hiệu tên trùng
                    }
                    nv.Ten = ten;
                    nv.TrangThai = 1;
                    _dbContext.KichCos.Update(nv);
                    _dbContext.SaveChanges();
                    return nv;
                }

                return null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<KichCo>> GetAllKichCo()
        {
            try
            {
                return await _dbContext.KichCos.OrderByDescending(x => x.TrangThai).ToListAsync();
            }
            catch (Exception) { throw; }
        }
        public async Task<KichCo> GetKickCoById(Guid id)
        {
            var nv = await _dbContext.KichCos.FirstOrDefaultAsync(nv => nv.ID == id);
            return nv;
        }

        #endregion

        #region MauSac

        public async Task<MauSac> AddMauSac(string ten, string ma, int trangthai)
        {
            try
            {
                string tenChuanHoa = ChuanHoaTen(ten);
                string maChuanHoa = ma?.Trim().ToUpper();

                // Kiểm tra trùng tên màu (không dấu, không phân biệt hoa thường)
                var mauTrungTen = _dbContext.MauSacs
                    .AsEnumerable()
                    .FirstOrDefault(x => ChuanHoaTen(x.Ten) == tenChuanHoa);

                if (mauTrungTen != null)
                {
                    // Tên đã tồn tại (nghĩa là trùng về ý nghĩa)
                    return null;
                }

                // Kiểm tra trùng mã màu (ví dụ: #FF0000, #ff0000, FF0000)
                string maSoSanh = maChuanHoa.StartsWith("#") ? maChuanHoa : $"#{maChuanHoa}";
                var mauTrungMa = await _dbContext.MauSacs
                    .FirstOrDefaultAsync(x => x.Ma.ToUpper().Trim() == maSoSanh);

                if (mauTrungMa != null)
                {
                    // Mã màu đã tồn tại
                    return null;
                }

                // Xử lý mã màu: nếu thiếu dấu # thì thêm vào
                string maLuu = maChuanHoa.StartsWith("#") ? maChuanHoa : $"#{maChuanHoa}";

                MauSac kc = new MauSac()
                {
                    ID = Guid.NewGuid(),
                    Ten = ten.Trim(),
                    Ma = maLuu,
                    TrangThai = trangthai
                };

                _dbContext.MauSacs.Add(kc);
                await _dbContext.SaveChangesAsync();
                return kc;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteMauSac(Guid id)
        {
            try
            {
                var nv = await _dbContext.MauSacs.FirstOrDefaultAsync(nv => nv.ID == id);
                if (nv != null)
                {
                    _dbContext.MauSacs.Remove(nv);
                    _dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;

            }
        }

        public async Task<List<MauSac>> GetAllMauSac()
        {
            try
            {
                return await _dbContext.MauSacs.OrderByDescending(x => x.TrangThai).ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<MauSac> GetMauSacById(Guid id)
        {
            try
            {
                var nv = await _dbContext.MauSacs.FirstOrDefaultAsync(nv => nv.ID == id);
                return nv;
            }
            catch (Exception) { throw; }
        }

        public async Task<MauSac> UpdateMauSac(Guid id, string ten, string ma, int trangthai)
        {
            try
            {
                var nv = await _dbContext.MauSacs.FirstOrDefaultAsync(x => x.ID == id);
                if (nv == null)
                    return null;

                // Chuẩn hóa tên mới để kiểm tra trùng
                string tenChuanHoaMoi = ChuanHoaTen(ten);

                // Kiểm tra có màu khác (khác ID) trùng tên đã chuẩn hóa không
                var existingColor = _dbContext.MauSacs
                    .AsEnumerable()
                    .FirstOrDefault(x => x.ID != id && ChuanHoaTen(x.Ten) == tenChuanHoaMoi);

                if (existingColor != null)
                {
                    // Trùng tên với màu khác
                    return null;
                }

                // Xử lý mã màu
                bool isHasHash = ma.StartsWith("#");
                string maChuan = isHasHash ? ma.Trim().ToUpper() : $"#{Uri.EscapeDataString(ma.Trim().ToUpper())}";

                // Gán dữ liệu mới
                nv.Ten = ten.Trim();
                nv.Ma = maChuan;
                nv.TrangThai = 1;

                _dbContext.MauSacs.Update(nv);
                await _dbContext.SaveChangesAsync();

                return nv;

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region chat lieu
        public async Task<ChatLieu> AddChatLieu(string ten, int trangthai)
        {
            try
            {
                string tenChuanHoa = ChuanHoaTen(ten);

                // So sánh sau khi chuẩn hóa, dùng AsEnumerable để xử lý trong RAM
                var existing = _dbContext.ChatLieus
                    .AsEnumerable()
                    .FirstOrDefault(x => ChuanHoaTen(x.Ten) == tenChuanHoa);

                if (existing != null)
                {
                    // Đã tồn tại chất liệu trùng logic
                    return null;
                }

                var chatLieuMoi = new ChatLieu
                {
                    ID = Guid.NewGuid(),
                    Ten = ten.Trim(),
                    TrangThai = 1
                };

                _dbContext.ChatLieus.Add(chatLieuMoi);
                await _dbContext.SaveChangesAsync();

                return chatLieuMoi;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ChatLieu> GetChatLieuById(Guid id)
        {
            try
            {
                var nv = await _dbContext.ChatLieus.FirstOrDefaultAsync(nv => nv.ID == id);
                return nv;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> DeleteChatLieu(Guid id)
        {

            try
            {
                var nv = await _dbContext.ChatLieus.FirstOrDefaultAsync(nv => nv.ID == id);
                if (nv != null)
                {
                    _dbContext.ChatLieus.Remove(nv);
                    _dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ChatLieu> UpdateChatLieu(Guid id, string ten, int trangthai)
        {

            try
            {
                var nv = await _dbContext.ChatLieus.FirstOrDefaultAsync(x => x.ID == id);
                if (nv == null)
                    return null;

                string tenChuanHoaMoi = ChuanHoaTen(ten);

                // Kiểm tra tên đã tồn tại ở bản ghi KHÁC
                var existing = _dbContext.ChatLieus
                    .AsEnumerable()
                    .FirstOrDefault(x => x.ID != id && ChuanHoaTen(x.Ten) == tenChuanHoaMoi);

                if (existing != null)
                {
                    // Có bản ghi khác trùng tên (ý nghĩa)
                    return null;
                }

                nv.Ten = ten.Trim();
                nv.TrangThai = 1;

                _dbContext.ChatLieus.Update(nv);
                await _dbContext.SaveChangesAsync();

                return nv;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ChatLieu>> GetAllChatLieu()
        {
            try
            {
                return await _dbContext.ChatLieus.OrderByDescending(x => x.TrangThai).ToListAsync();
            }
            catch (Exception)
            {

                throw;

            }
        }

        #endregion

        #region other
        //Chuẩn hóa tên
        public static string ChuanHoaTen(string tennguoidungnhap)
        {
            if (string.IsNullOrWhiteSpace(tennguoidungnhap))
                return string.Empty;

            var normalized = tennguoidungnhap.Normalize(NormalizationForm.FormD); // Bước chuẩn hóa để chuyển chuổi có dấu unicode về ko dấu
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(c); // Kiểm tra loại ký tự
                if (cat != UnicodeCategory.NonSpacingMark) // Bỏ qua các ký tự dấu, chỉ giữ lại chữ cái gốc
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToUpper().Trim();
        }

        #endregion
    }
}
