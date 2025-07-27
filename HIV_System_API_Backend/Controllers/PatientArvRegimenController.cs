    using HIV_System_API_Backend.Common;
using HIV_System_API_DTOs.PatientARVRegimenDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientArvRegimenController : ControllerBase
    {
        private readonly IPatientArvRegimenService _patientArvRegimenService;
        public PatientArvRegimenController()
        {
            _patientArvRegimenService = new PatientArvRegimenService();
        }

        /// <summary>
        /// Gets all patient ARV regimens
        /// </summary>
        /// <returns>List of all patient ARV regimens</returns>
        [HttpGet("GetAllPatientArvRegimens")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<IActionResult> GetAllPatientArvRegimens()
        {
            try
            {
                var regimens = await _patientArvRegimenService.GetAllPatientArvRegimensAsync();

                if (regimens == null || regimens.Count == 0)
                {
                    return Ok(new List<PatientArvRegimenResponseDTO>()); // Return empty list with 200 OK
                }

                return Ok(regimens);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving ARV regimens: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error occurred: {ex.InnerException}");
            }
        }
        // <summary>
        /// Gets a specific patient ARV regimen by ID
        /// </summary>
        /// <param name="parId">Patient ARV Regimen ID</param>
        /// <returns>Patient ARV regimen details</returns>
        [HttpGet("GetPatientArvRegimenById/{parId}")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<IActionResult> GetPatientArvRegimenById(int parId)
        {
            try
            {
                var regimen = await _patientArvRegimenService.GetPatientArvRegimenByIdAsync(parId);

                if (regimen == null)
                {
                    return NotFound($"Patient ARV regimen with ID {parId} not found.");
                }

                return Ok(regimen);
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving ARV regimen: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error occurred: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Creates a new patient ARV regimen
        /// </summary>
        /// <param name="patientArvRegimen">Patient ARV regimen data</param>
        /// <returns>Created patient ARV regimen</returns>
        [HttpPost("CreatePatientArvRegimen")]
        [Authorize(Roles = "1,2,5")]
        public async Task<IActionResult> CreatePatientArvRegimen([FromBody] PatientArvRegimenRequestDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var createdRegimen = await _patientArvRegimenService.CreatePatientArvRegimenAsync(patientArvRegimen);

                return CreatedAtAction(
                    nameof(GetPatientArvRegimenById),
                    new { parId = createdRegimen.PatientArvRegiId }, // Fixed: Use PatientArvRegiId instead of PatientMedRecordId
                    createdRegimen);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Missing required data: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while creating ARV regimen: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error creating regimen: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Deletes a patient ARV regimen
        /// </summary>
        /// <param name="parId">Patient ARV Regimen ID</param>
        /// <returns>NoContent if successful</returns>
        [HttpDelete("DeletePatientArvRegimen/{parId}")]
        [Authorize(Roles = "1,2,5")]
        public async Task<IActionResult> DeletePatientArvRegimen(int parId)
        {
            try
            {
                var result = await _patientArvRegimenService.DeletePatientArvRegimenAsync(parId);

                if (!result)
                {
                    return NotFound($"Patient ARV regimen with ID {parId} not found.");
                }

                return NoContent(); // 204 No Content
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while deleting ARV regimen: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error deleting regimen: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Updates an existing patient ARV regimen
        /// </summary>
        /// <param name="parId">Patient ARV Regimen ID</param>
        /// <param name="patientArvRegimen">Updated patient ARV regimen data</param>
        /// <returns>Updated patient ARV regimen</returns>
        [HttpPut("UpdatePatientArvRegimen/{parId}")]
        [Authorize(Roles = "1,2,5")]
        public async Task<IActionResult> UpdatePatientArvRegimen(int parId, [FromBody] PatientArvRegimenRequestDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var updatedRegimen = await _patientArvRegimenService.UpdatePatientArvRegimenAsync(parId, patientArvRegimen);

                return Ok(updatedRegimen);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Missing required data: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while updating ARV regimen: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error updating regimen: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Gets all ARV regimens for a specific patient
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <returns>List of ARV regimens for the patient</returns>
        [HttpGet("GetPatientArvRegimensByPatientId")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<IActionResult> GetPatientArvRegimensByPatientId(int patientId)
        {
            try
            {
                var regimens = await _patientArvRegimenService.GetPatientArvRegimensByPatientIdAsync(patientId);

                if (regimens == null || !regimens.Any())
                {
                    return Ok(new List<PatientArvRegimenResponseDTO>()); // Return empty list with 200 OK
                }

                return Ok(regimens);
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid argument: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while retrieving ARV regimens: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error occurred while retrieving ARV regimens for Patient ID {patientId}: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Gets personal ARV regimens for a specific person
        /// </summary>
        /// <param name="personalId">Personal ID</param>
        /// <returns>List of personal ARV regimens</returns>
        [HttpGet("GetPersonalArvRegimens")]
        [Authorize]
        public async Task<IActionResult> GetPersonalArvRegimens()
        {
            try
            {
                var personalId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (personalId == null)
                {
                    return Unauthorized("Personal ID not found in token.");
                }

                var regimens = await _patientArvRegimenService.GetPersonalArvRegimensAsync(personalId.Value);
                if (regimens == null || !regimens.Any())
                {
                    return Ok(new List<object>()); // Return empty list with 200 OK
                }
                return Ok(regimens);
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid argument: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while retrieving personal ARV regimens: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error occurred while retrieving personal ARV regimens for Personal ID : {ex.InnerException}");
            }
        }

        /// <summary>
        /// Partially updates a patient ARV regimen
        /// </summary>
        /// <param name="parId">Patient ARV Regimen ID</param>
        /// <param name="patientArvRegimen">Updated patient ARV regimen data</param>
        /// <returns>Updated patient ARV regimen</returns>
        [HttpPatch("PatchPatientArvRegimen/{parId}")]
        [Authorize(Roles = "1,2,5")]
        public async Task<IActionResult> PatchPatientArvRegimen(int parId, [FromBody] PatientArvRegimenPatchDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var updatedRegimen = await _patientArvRegimenService.PatchPatientArvRegimenAsync(parId, patientArvRegimen);
                return Ok(updatedRegimen);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Missing required data: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error while updating ARV regimen: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Unexpected error updating regimen: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Initiates a new patient ARV regimen with validation
        /// </summary>
        /// <param name="patientId">Patient ID to initiate ARV regimen for</param>
        /// <returns>Initiated patient ARV regimen</returns>
        [HttpPost("InitiatePatientArvRegimen")]
        [Authorize(Roles = "1,2,5")]
        public async Task<IActionResult> InitiatePatientArvRegimen(int patientId)
        {
            try
            {
                if (patientId <= 0)
                {
                    return BadRequest("Invalid Patient ID. ID must be greater than 0.");
                }

                var initiatedRegimen = await _patientArvRegimenService.InitiatePatientArvRegimenAsync(patientId);

                return CreatedAtAction(
                    nameof(GetPatientArvRegimenById),
                    new { parId = initiatedRegimen.PatientArvRegiId },
                    initiatedRegimen);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Missing required data: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while initiating ARV regimen: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error initiating regimen: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Creates a new patient ARV regimen with associated medications
        /// </summary>
        /// <param name="request">Object containing regimen and medication data</param>
        /// <returns>Created patient ARV regimen with medications</returns>
        [HttpPost("CreatePatientArvRegimenWithMedications")]
        [Authorize(Roles = "1,2,5")]
        public async Task<IActionResult> CreatePatientArvRegimenWithMedications([FromBody] CreatePatientArvRegimenWithMedicationsRequestDTO request)
        {
            if (request == null || request.Regimen == null || request.Medications == null)
            {
                return BadRequest("Request body is null or missing required regimen or medications data.");
            }

            try
            {
                var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (accId == null)
                {
                    return Unauthorized("Account ID not found in token.");
                }

                var createdRegimen = await _patientArvRegimenService.CreatePatientArvRegimenWithMedicationsAsync(
                    request.Regimen,
                    request.Medications,
                    accId.Value);

                return CreatedAtAction(
                    nameof(GetPatientArvRegimenById),
                    new { parId = createdRegimen.PatientArvRegiId },
                    createdRegimen);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Missing required data: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while creating ARV regimen with medications: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error creating regimen with medications: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPost("UpdatePatientArvRegimenStatus/{parId}")]
        [Authorize (Roles = "1,2,5")]
        public async Task<PatientArvRegimenResponseDTO> UpdatePatientArvRegimenStatusAsync(int parId,PatientArvRegimenStatusRequestDTO request)
        {
            try
            {
                var updatedStatus = await _patientArvRegimenService.UpdatePatientArvRegimenStatusAsync(parId, request);
                return updatedStatus;
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException($"Database error while updating ARV regimen status: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error updating regimen status: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("UpdatePatientArvRegimenWithMedications/{parId}")]
        [Authorize(Roles = "1,2,5")]
        public async Task<PatientArvRegimenResponseDTO> UpdatePatientArvRegimenWithMedications(
        int parId,
        [FromBody] UpdatePatientArvRegimenWithMedicationsRequestDTO request)
        {
            try
            {
                // Get the current user's account ID from claims
                int accId = ClaimsHelper.ExtractAccountIdFromClaims(User).Value;
                if (accId == null)
                {
                    throw new UnauthorizedAccessException("Invalid account ID in token");
                }

                var updatedRegimen = await _patientArvRegimenService.UpdatePatientArvRegimenWithMedicationsAsync(
                    parId,
                    request.RegimenRequest,
                    request.MedicationRequests,
                    accId);

                return updatedRegimen;
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException($"Required parameter is missing: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Operation failed: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"Authorization failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException($"Database error while updating ARV regimen with medications: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error updating regimen with medications: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}

