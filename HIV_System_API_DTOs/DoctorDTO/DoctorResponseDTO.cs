﻿using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.DoctorWorkScheduleDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.DoctorDTO
{
    public class DoctorResponseDTO
    {
        public int DoctorId { get; set; }
        public string? Degree { get; set; }
        public string? Bio {  get; set; }
        public int AccId { get; set; }
        public AccountResponseDTO? Account {  get; set; }
        public List<DoctorWorkScheduleResponseDTO>? WorkSchedule { get; set; }
    }
}
