using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.NotificationDTO
{
    public class NotificationDetailResponseDTO : NotificationResponseDTO
    {
        public List<NotificationRecipientDTO> Recipients { get; set; } = new();
    }
}