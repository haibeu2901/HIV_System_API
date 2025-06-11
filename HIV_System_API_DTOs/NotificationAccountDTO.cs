using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs
{
    public class NotificationAccountDTO
    {
        public int NtaId { get; set; }

        public int AccId { get; set; }

        public int NtfId { get; set; } = 0;
    }
}
