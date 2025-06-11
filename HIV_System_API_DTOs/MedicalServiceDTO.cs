using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs
{
    public class MedicalServiceDTO
    {
        public int SrvId { get; set; }

        public int AccId { get; set; }

        public string ServiceName { get; set; } = null!;

        public string? ServiceDescription { get; set; }

        public double Price { get; set; }

        public bool? IsAvailable { get; set; }
    }
}
