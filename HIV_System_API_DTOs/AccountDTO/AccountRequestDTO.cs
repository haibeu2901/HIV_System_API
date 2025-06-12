using System;
using System.Collections.Generic;

namespace HIV_System_API_DTOs.AccountDTO;

public class AccountRequestDTO
{
    public string AccUsername { get; set; } = null!;

    public string AccPassword { get; set; } = null!;

    public string? Email { get; set; }

    public string? Fullname { get; set; }

    public DateOnly? Dob { get; set; }

    public bool? Gender { get; set; }

    public byte Roles { get; set; }

    public bool? IsActive { get; set; }

}
