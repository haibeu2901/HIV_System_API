﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientDTO
{
    public class PatientProfileUpdateDTO
    {
        public string? Fullname { get; set; }
        public DateOnly? Dob { get; set; }
        public bool? Gender { get; set; }
    }
}
