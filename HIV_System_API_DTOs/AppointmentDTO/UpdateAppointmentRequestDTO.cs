﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.AppointmentDTO
{
    public class UpdateAppointmentRequestDTO
    {
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly AppointmentTime { get; set; }
        public string? Notes { get; set; }
    }
}
