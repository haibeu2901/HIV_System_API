using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.NotificationDTO
{
    public class NotificationResponseDTO
    {
        public int NtfId { get; set; }
        public string? NotiType { get; set; }
        public string? NotiMessage { get; set; }
        public DateTime SendAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}