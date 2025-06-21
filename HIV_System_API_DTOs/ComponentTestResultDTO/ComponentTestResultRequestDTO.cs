using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.ComponentTestResultDTO
{
    public class ComponentTestResultRequestDTO
    {
        public int ComponentTestResultId { get; set; }

        public int TestResultId { get; set; }

        public string ComponentTestResultName  { get; set; } = null!;

        public string? CtrDescription { get; set; }

        public string? ResultValue { get; set; }

        public string? Notes { get; set; }
    }
}
