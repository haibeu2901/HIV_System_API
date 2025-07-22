using HIV_System_API_Backend.Common;
using HIV_System_API_DTOs.MedicationAlarmDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationAlarmController : ControllerBase
    {
        private readonly IMedicationAlarmService _medicationAlarmService;

        public MedicationAlarmController()
        {
            _medicationAlarmService = new MedicationAlarmService();
        }

        /// <summary>
        /// Create a new medication alarm
        /// </summary>
        /// <param name="request">The medication alarm details</param>
        /// <returns>The created medication alarm</returns>
        /// <response code="201">Returns the newly created medication alarm</response>
        /// <response code="400">If the alarm data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have permission</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("CreateMedicationAlarm")]
        [Authorize(Roles = "3")] // Only patients
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicationAlarmResponseDTO>> CreateMedicationAlarm([FromBody] MedicationAlarmRequestDTO request)
        {
            try
            {
                var patientId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (patientId == null)
                    return Unauthorized("ID bệnh nhân không tìm thấy trong token.");

                var createdAlarm = await _medicationAlarmService.CreateMedicationAlarmAsync(request, patientId.Value);
                return CreatedAtAction(nameof(GetMedicationAlarmById), new { id = createdAlarm.AlarmId }, createdAlarm);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific medication alarm by ID
        /// </summary>
        /// <param name="id">The ID of the medication alarm to retrieve</param>
        /// <returns>The requested medication alarm</returns>
        /// <response code="200">Returns the requested medication alarm</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="404">If the medication alarm was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("GetMedicationAlarmById/{id}")]
        [Authorize(Roles = "3")] // Only patients
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicationAlarmResponseDTO>> GetMedicationAlarmById(int id)
        {
            try
            {
                var patientId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (patientId == null)
                    return Unauthorized("ID bệnh nhân không tìm thấy trong token.");

                var alarm = await _medicationAlarmService.GetMedicationAlarmByIdAsync(id, patientId.Value);
                if (alarm == null)
                    return NotFound($"Báo động thuốc với ID {id} không tìm thấy.");

                return Ok(alarm);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {ex.Message}");
            }
        }

        /// <summary>
        /// Get personal medication alarms for the current patient
        /// </summary>
        /// <returns>List of medication alarms for the current patient</returns>
        /// <response code="200">Returns the list of personal medication alarms</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("GetPersonalMedicationAlarms")]
        [Authorize(Roles = "3")] // Only patients
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MedicationAlarmResponseDTO>>> GetPersonalMedicationAlarms()
        {
            try
            {
                var patientId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (patientId == null)
                    return Unauthorized("ID bệnh nhân không tìm thấy trong token.");

                var alarms = await _medicationAlarmService.GetPersonalMedicationAlarmsAsync(patientId.Value);
                return Ok(alarms);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {ex.Message}");
            }
        }

        /// <summary>
        /// Update a medication alarm
        /// </summary>
        /// <param name="id">The ID of the medication alarm to update</param>
        /// <param name="request">The updated medication alarm details</param>
        /// <returns>The updated medication alarm</returns>
        /// <response code="200">Returns the updated medication alarm</response>
        /// <response code="400">If the alarm data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have permission</response>
        /// <response code="404">If the medication alarm was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("UpdateMedicationAlarm/{id}")]
        [Authorize(Roles = "3")] // Only patients
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicationAlarmResponseDTO>> UpdateMedicationAlarm(int id, [FromBody] MedicationAlarmUpdateDTO request)
        {
            try
            {
                var patientId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (patientId == null)
                    return Unauthorized("ID bệnh nhân không tìm thấy trong token.");

                var updatedAlarm = await _medicationAlarmService.UpdateMedicationAlarmAsync(id, request, patientId.Value);
                return Ok(updatedAlarm);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("không tồn tại"))
                    return NotFound(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a medication alarm
        /// </summary>
        /// <param name="id">The ID of the medication alarm to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the medication alarm was successfully deleted</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have permission</response>
        /// <response code="404">If the medication alarm was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("DeleteMedicationAlarm/{id}")]
        [Authorize(Roles = "3")] // Only patients
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMedicationAlarm(int id)
        {
            try
            {
                var patientId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (patientId == null)
                    return Unauthorized("ID bệnh nhân không tìm thấy trong token.");

                var result = await _medicationAlarmService.DeleteMedicationAlarmAsync(id, patientId.Value);
                if (!result)
                    return NotFound($"Báo động thuốc với ID {id} không tìm thấy.");

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle the active status of a medication alarm
        /// </summary>
        /// <param name="id">The ID of the medication alarm</param>
        /// <param name="isActive">The new active status</param>
        /// <returns>Success status</returns>
        /// <response code="200">If the status was successfully changed</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have permission</response>
        /// <response code="404">If the medication alarm was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPatch("ToggleAlarmStatus/{id}")]
        [Authorize(Roles = "3")] // Only patients
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ToggleAlarmStatus(int id, [FromQuery] bool isActive)
        {
            try
            {
                var patientId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (patientId == null)
                    return Unauthorized("ID bệnh nhân không tìm thấy trong token.");

                var result = await _medicationAlarmService.ToggleAlarmStatusAsync(id, isActive, patientId.Value);
                if (!result)
                    return NotFound($"Báo động thuốc với ID {id} không tìm thấy.");

                return Ok(new { Message = $"Trạng thái báo động đã được {(isActive ? "bật" : "tắt")} thành công." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {ex.Message}");
            }
        }

        /// <summary>
        /// Manually trigger medication alarm processing (Admin only)
        /// </summary>
        /// <returns>Success status</returns>
        /// <response code="200">If the processing was triggered successfully</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("ProcessMedicationAlarms")]
        [Authorize(Roles = "1")] // Only admin
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessMedicationAlarms()
        {
            try
            {
                await _medicationAlarmService.ProcessMedicationAlarmsAsync();
                return Ok(new { Message = "Xử lý báo động thuốc thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {ex.Message}");
            }
        }
    }
}