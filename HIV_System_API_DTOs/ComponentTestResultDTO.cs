using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs
{
    public class ComponentTestResultDTO
    {
        public int CtId { get; set; }
        public int TrsId { get; set; }
        public int StfId { get; set; }
        public string CtrName { get; set; } = null!;
        public string? CtrDescription { get; set; }
        public string? ResultValue { get; set; }
        public string? Notes { get; set; }
    }
}
