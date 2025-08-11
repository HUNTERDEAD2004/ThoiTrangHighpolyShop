using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
    public class FilterData
    {
        public List<string>? priceRange { get; set; }
        public string? search { get; set; }
        public string? loaiSP { get; set; } // Trước là List<string>
        public List<Guid> mauSac { get; set; }
        public List<Guid>? kichCo { get; set; }
        public List<string>? chatLieu { get; set; } // GUID -> string
        public string? sortSP { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public int? minPrice { get; set; }
        public int? maxPrice { get; set; }
    }
}
