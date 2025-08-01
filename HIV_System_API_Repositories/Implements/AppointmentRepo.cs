﻿using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DTOs.Appointment;
using HIV_System_API_DTOs.AppointmentDTO;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class AppointmentRepo : IAppointmentRepo
    {
        public async Task<Appointment> ChangeAppointmentStatusAsync(int id, byte status)
        {
            return await AppointmentDAO.Instance.ChangeAppointmentStatusAsync(id, status);
        }

        public async Task<Appointment> CreateAppointmentAsync(CreateAppointmentRequestDTO appointment, int accId)
        {
            return await AppointmentDAO.Instance.CreateAppointmentAsync(appointment, accId);
        }

        public async Task<bool> DeleteAppointmentByIdAsync(int id)
        {
            return await AppointmentDAO.Instance.DeleteAppointmentByIdAsync(id);
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await AppointmentDAO.Instance.GetAllAppointmentsAsync();
        }

        public async Task<List<Appointment>> GetAllPersonalAppointmentsAsync(int accId)
        {
            return await AppointmentDAO.Instance.GetAllPersonalAppointmentsAsync(accId);
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
        {
            return await AppointmentDAO.Instance.GetAppointmentByIdAsync(id);
        }
            
        public async Task<List<Appointment>> GetAppointmentsByAccountIdAsync(int accountId, byte role)
        {
            return await AppointmentDAO.Instance.GetAppointmentsByAccountIdAsync(accountId, role);
        }

        public async Task<Appointment> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentRequestDTO appointment, int accId)
        {
            return await AppointmentDAO.Instance.UpdateAppointmentAsync(appointmentId, appointment, accId);
        }

        public async Task<Appointment> UpdateAppointmentByIdAsync(int id, Appointment appointment)
        {
            return await AppointmentDAO.Instance.UpdateAppointmentByIdAsync(id, appointment);
        }
    }
}
