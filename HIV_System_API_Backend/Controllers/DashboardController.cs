using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using HIV_System_API_DAOs;
using Microsoft.AspNetCore.Authorization;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> GetAdminDashboard(int userId)
        {
            try
            {
                var today = DateTime.Now;
                var stats = await _dashboardService.GetAdminDashboardStatsAsync(userId, today);
                return Ok(stats);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "1,2,5")]
        public async Task<IActionResult> GetDoctorDashboard(int doctorId)
        {
            try
            {
                var today = DateTime.Now;
                var stats = await _dashboardService.GetDoctorDashboardStatsAsync(doctorId, today);
                return Ok(stats);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "1,3,5")]
        public async Task<IActionResult> GetPatientDashboard(int patientId)
        {
            try
            {
                var today = DateTime.Now;
                var stats = await _dashboardService.GetPatientDashboardStatsAsync(patientId, today);
                return Ok(stats);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("staff/{staffId}")]
        [Authorize(Roles = "1,4,5")]
        public async Task<IActionResult> GetStaffDashboard(int staffId)
        {
            try
            {
                var today = DateTime.Now;
                var stats = await _dashboardService.GetStaffDashboardStatsAsync(staffId, today);
                return Ok(stats);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("manager")]
        [Authorize(Roles = "1,5")]
        public async Task<IActionResult> GetManagerDashboard(int userId)
        {
            try
            {
                var today = DateTime.Now;
                var stats = await _dashboardService.GetManagerDashboardStatsAsync(userId, today);
                return Ok(stats);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //[HttpGet("alerts")]
        //[Authorize(Roles ="1,2,3,4,5")]
        //public async Task<IActionResult> GetDashboardAlerts(int userId, string role)
        //{
        //    try
        //    {
        //        var today = DateTime.Now;
        //        var alerts = await _dashboardService.GetDashboardAlertsAsync(userId, role, today);
        //        return Ok(alerts);
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(ex.Message);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        //[HttpGet("charts/user-distribution")]
        //[Authorize(Roles = "1,5")]
        //public async Task<IActionResult> GetUserDistributionChart(int userId)
        //{
        //    try
        //    {
        //        var chart = await _dashboardService.GetUserDistributionChartAsync(userId);
        //        return Ok(chart);
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        //[HttpGet("charts/manager-service")]
        //[Authorize(Roles = "1,5")]
        //public async Task<IActionResult> GetManagerServiceChart(int userId)
        //{
        //    try
        //    {
        //        var chart = await _dashboardService.GetManagerServiceChartAsync(userId);
        //        return Ok(chart);
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
    }
}
