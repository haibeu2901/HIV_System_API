using HIV_System_API_DTOs.ComponentTestResultDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.TestResultDTO
{
    public class TestResultResponseDTO
    {
        public int TestResultId { get; set; }
        public int PatientMedicalRecordId { get; set; }
        public DateOnly TestDate { get; set; }
        public bool? Result { get; set; } 
        public string? Notes { get; set; } 
        public List<ComponentTestResultResponseDTO> ComponentTestResults { get; set; } = new List<ComponentTestResultResponseDTO>();
    }
}
