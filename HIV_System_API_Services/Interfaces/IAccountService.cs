﻿using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.PatientDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IAccountService
    {
        Task<List<AccountResponseDTO>> GetAllAccountsAsync();
        Task<AccountResponseDTO?> GetAccountByLoginAsync(string accUsername, string accPassword);
        Task<AccountResponseDTO?> GetAccountByIdAsync(int accId);
        Task<AccountResponseDTO> UpdateAccountByIdAsync(int id, UpdateAccountRequestDTO updatedAccount);
        Task<bool> DeleteAccountAsync(int accId);
        Task<AccountResponseDTO> CreateAccountAsync(AccountRequestDTO account);
        Task<PatientResponseDTO> CreatePatientAccountAsync(PatientAccountRequestDTO patient);
        Task<AccountResponseDTO> UpdatePatientProfileAsync(int accountId, PatientProfileUpdateDTO profileDTO);
        Task<AccountResponseDTO> UpdateDoctorProfileAsync(int accountId, DoctorProfileUpdateDTO profileDTO);
    }
}
