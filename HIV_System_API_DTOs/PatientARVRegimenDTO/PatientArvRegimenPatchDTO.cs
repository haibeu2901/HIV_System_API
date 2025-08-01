﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientARVRegimenDTO
{
    public class PatientArvRegimenPatchDTO
    {
        public string? Notes { get; set; }
        public byte? RegimenLevel { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public byte? RegimenStatus { get; set; }
        public double? TotalCost { get; set; }
    }
}
