using HIV_System_API_DTOs.PatientArvMedicationDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
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
        /// <response code="404">If the medication was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("GetPatientArvMedicationById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Create a new patient ARV medication
        /// </summary>
        /// <param name="medicationDTO">The medication details</param>
        /// <returns>The created medication</returns>
        /// <response code="201">Returns the newly created medication</response>
        /// <response code="400">If the medication data is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("CreatePatientArvMedication")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientArvMedicationResponseDTO>> CreatePatientArvMedication([FromBody] PatientArvMedicationRequestDTO medicationDTO)
        {
            try
            {
                var createdMedication = await _patientArvMedicationService.CreatePatientArvMedicationAsync(medicationDTO);
                return CreatedAtAction(nameof(GetPatientArvMedicationById), new { id = createdMedication.PatientArvMedId }, createdMedication);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.InnerException);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.InnerException);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientArvMedicationResponseDTO>> UpdatePatientArvMedication(int id, [FromBody] PatientArvMedicationRequestDTO medicationDTO)
        {
            try
            {
                var updatedMedication = await _patientArvMedicationService.UpdatePatientArvMedicationAsync(id, medicationDTO);
                return Ok(updatedMedication);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.InnerException);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.InnerException);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Patient ARV medication with ID {id} not found.");
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
        /// <response code="404">If the medication was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("DeletePatientArvMedication/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }
    }
}