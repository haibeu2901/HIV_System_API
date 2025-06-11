using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs
{
    public class TestResultDTO
    {
        public int TrsId { get; set; }

        public int PmrId { get; set; }

        public DateOnly TestDate { get; set; }

        public bool? ResultValue { get; set; }

        public string? Notes { get; set; } = null;
    }
}
