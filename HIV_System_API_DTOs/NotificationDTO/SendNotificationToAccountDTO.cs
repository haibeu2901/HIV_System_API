using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.NotificationDTO
{
    public class SendNotificationToAccountDTO
    {
        public int AccountId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public DateTime SendAt { get; set; }
    }
}
