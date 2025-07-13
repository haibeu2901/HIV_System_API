using HIV_System_API_DAOs.Implements.DashboardDAO;
using HIV_System_API_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements.DashboardRepo
{
    public class PatientDashboardRepo : BaseDashboardRepo
    {
        private readonly PatientDashboardDAO _patientDao;

        public PatientDashboardRepo(PatientDashboardDAO patientDao, BaseDashboardDAO baseDao)
            : base(baseDao)
        {
            _patientDao = patientDao;
        }

        public async Task<PatientDashboardStats> GetPatientDashboardStatsAsync(int patientId, DateTime today)
        {
            return await _patientDao.GetPatientDashboardStatsAsync(patientId, today);
        }

        //public async Task<List<DashboardAlert>> GetPatientAlertsAsync(int patientId, DateTime today)
        //{
        //    return await _patientDao.GetPatientAlertsAsync(patientId, today);
        //}
    }
}
