//using AppAPI.IServices;
//using AppData.IRepositories;
//using AppData.Models;
//using AppData.Repositories;

//namespace AppAPI.Services
//{
//    public class ChiTietKhuyenMaiServices:IChiTietKhuyenMaiServices
//    {
//        private readonly IAllRepository<ChiTietKhuyenMai> _respon;
//        AssignmentDBContext context= new AssignmentDBContext();
//        public ChiTietKhuyenMaiServices()
//        {
//            _respon = new AllRepository<ChiTietKhuyenMai>(context,context.ChiTietKhuyenMais);
//        }

//        public bool Add(int TrangThai, Guid IdBienThe, Guid IdKhuyenMai)
//        {
//            var Chitietkhuyenmai = new ChiTietKhuyenMai();
//            Chitietkhuyenmai.ID= Guid.NewGuid();
//            Chitietkhuyenmai.TrangThai= TrangThai;
//            Chitietkhuyenmai.IDKhuyenMai = IdKhuyenMai;
//            Chitietkhuyenmai.IDBienThe = IdBienThe;
//            return _respon.Add(Chitietkhuyenmai);
//        }

//        public bool Delete(Guid Id)
//        {
//            var Chitietkhuyenmai = _respon.GetAll().FirstOrDefault(x => x.ID == Id);
//            if (Chitietkhuyenmai != null)
//            {
               
//                return _respon.Delete(Chitietkhuyenmai);
//            }
//            else
//            {
//                return false;
//            }
//        }

//        public List<ChiTietKhuyenMai> GetAll()
//        {
//            return _respon.GetAll();
//        }

//        public ChiTietKhuyenMai GetById(Guid Id)
//        {
//            return _respon.GetAll().FirstOrDefault(x => x.ID == Id);
//        }

//        public bool Update(Guid Id, int TrangThai, Guid IdBienThe, Guid IdKhuyenMai)
//        {
//            var Chitietkhuyenmai= _respon.GetAll().FirstOrDefault(x => x.ID == Id);
//            if (Chitietkhuyenmai != null)
//            {
//                Chitietkhuyenmai.TrangThai = TrangThai;
//                Chitietkhuyenmai.IDKhuyenMai = IdKhuyenMai;
//                Chitietkhuyenmai.IDBienThe = IdBienThe;
//                return _respon.Update(Chitietkhuyenmai);
//            }
//            else
//            {
//                return false;
//            }
//        }
//    }
//}
