using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.TestResultDTO
{
    public class TestResultUpdateRequestDTO
    {
        public DateOnly? TestDate { get; set; }
        public bool? Result { get; set; } 
        public string? Notes { get; set; } 
    }
}
