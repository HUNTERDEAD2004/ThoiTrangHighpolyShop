using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.DTO
{
    public class DiaChiDTO
    {
        public Guid Id { get; set; }

        // alias cho Id
        public Guid IDDiaChi
        {
            get => Id;
            set => Id = value;
        }
        public string DiaChiChiTiet { get; set; }
        public string Tinh { get; set; }
        public string Huyen { get; set; }
        public string Xa { get; set; }
        public bool IsDefault { get; set; }
    }
}
