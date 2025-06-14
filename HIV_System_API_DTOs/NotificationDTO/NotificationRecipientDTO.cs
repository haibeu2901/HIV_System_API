using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.NotificationDTO
{
    public class NotificationRecipientDTO
    {
        public int AccId { get; set; }
        public string? Fullname { get; set; }
        public byte Role { get; set; }
    }
}