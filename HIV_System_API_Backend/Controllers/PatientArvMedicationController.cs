using HIV_System_API_DTOs.PatientArvMedicationDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientArvMedicationController : ControllerBase
    {
        private readonly IPatientArvMedicationService _patientArvMedicationService;

        public PatientArvMedicationController()
        {
            _patientArvMedicationService = new PatientArvMedicationService();
        }

        /// <summary>
        /// Get all patient ARV medications
        /// </summary>
        /// <returns>List of all patient ARV medications</returns>
        /// <response code="200">Returns the list of medications</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("GetAllPatientArvMedications")]
        [Authorize(Roles = "1,2,4,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PatientArvMedicationResponseDTO>>> GetAllPatientArvMedications()
        {
            try
            {
                var medications = await _patientArvMedicationService.GetAllPatientArvMedicationsAsync();
                return Ok(medications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get a specific patient ARV medication by ID
        /// </summary>
        /// <param name="id">The ID of the medication to retrieve</param>
        /// <returns>The requested patient ARV medication</returns>
        /// <response code="200">Returns the requested medication</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="404">If the medication was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("GetPatientArvMedicationById/{id}")]
        [Authorize(Roles = "1,2,4,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientArvMedicationResponseDTO>> GetPatientArvMedicationById(int id)
        {
            try
            {
                var medication = await _patientArvMedicationService.GetPatientArvMedicationByIdAsync(id);
                if (medication == null)
                {
                    return NotFound($"Patient ARV medication with ID {id} not found.");
                }
                return Ok(medication);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        // <summary>
        /// Create a new patient ARV medication
        /// </summary>
        /// <param name="medicationDTO">The medication details</param>
        /// <returns>The created medication</returns>
        /// <response code="201">Returns the newly created medication</response>
        /// <response code="400">If the medication data is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("CreatePatientArvMedication")]
        [Authorize(Roles = "1,2,5")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientArvMedicationResponseDTO>> CreatePatientArvMedication([FromBody] PatientArvMedicationRequestDTO medicationDTO)
        {
            try
            {
                if (medicationDTO == null)
                {
                    return BadRequest("Medication data is required.");
                }

                var createdMedication = await _patientArvMedicationService.CreatePatientArvMedicationAsync(medicationDTO);
                return CreatedAtAction(nameof(GetPatientArvMedicationById), new { id = createdMedication.PatientArvMedId }, createdMedication);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Update an existing patient ARV medication
        /// </summary>
        /// <param name="id">The ID of the medication to update</param>
        /// <param name="medicationDTO">The updated medication details</param>
        /// <returns>The updated medication</returns>
        /// <response code="200">Returns the updated medication</response>
        /// <response code="400">If the medication data is invalid</response>
        /// <response code="404">If the medication was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("UpdatePatientArvMedication/{id}")]
        [Authorize(Roles = "1,2,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientArvMedicationResponseDTO>> UpdatePatientArvMedication(int id, [FromBody] PatientArvMedicationRequestDTO medicationDTO)
        {
            try
            {
                if (medicationDTO == null)
                {
                    return BadRequest("Medication data is required.");
                }

                var updatedMedication = await _patientArvMedicationService.UpdatePatientArvMedicationAsync(id, medicationDTO);
                return Ok(updatedMedication);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Check if it's a "not found" error
                if (ex.Message.Contains("does not exist"))
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Delete a patient ARV medication
        /// </summary>
        /// <param name="id">The ID of the medication to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the medication was successfully deleted</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="404">If the medication was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("DeletePatientArvMedication/{id}")]
        [Authorize(Roles = "1,2,5")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePatientArvMedication(int id)
        {
            try
            {
                var result = await _patientArvMedicationService.DeletePatientArvMedicationAsync(id);
                if (!result)
                {
                    return NotFound($"Patient ARV medication with ID {id} not found.");
                }
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Check if it's a "not found" error
                if (ex.Message.Contains("does not exist"))
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get patient ARV medications by patient ID
        /// </summary>
        /// <param name="patientId">The ID of the patient</param>
        /// <returns>List of ARV medications for the specified patient</returns>
        /// <response code="200">Returns the list of medications for the patient</response>
        /// <response code="400">If the patient ID is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("GetPatientArvMedicationsByPatientId/{patientId}")]
        [Authorize(Roles = "1,2,4,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PatientArvMedicationResponseDTO>>> GetPatientArvMedicationsByPatientId(int patientId)
        {
            try
            {
                var medications = await _patientArvMedicationService.GetPatientArvMedicationsByPatientIdAsync(patientId);
                return Ok(medications);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get patient ARV medications by patient regimen ID
        /// </summary>
        /// <param name="parId">The ID of the patient ARV regimen</param>
        /// <returns>List of ARV medications for the specified patient regimen</returns>
        /// <response code="200">Returns the list of medications for the patient regimen</response>
        /// <response code="400">If the patient regimen ID is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("GetPatientArvMedicationsByPatientRegimenId/{parId}")]
        [Authorize(Roles = "1,2,4,5")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PatientArvMedicationResponseDTO>>> GetPatientArvMedicationsByPatientRegimenId(int parId)
        {
            try
            {
                var medications = await _patientArvMedicationService.GetPatientArvMedicationsByPatientRegimenIdAsync(parId);
                return Ok(medications);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }
    }
}