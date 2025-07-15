using HIV_System_API_BOs;
using HIV_System_API_DTOs.PatientARVRegimenDTO;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Implements;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HIV_System_API_Tests
{
    public class PatientArvRegimenServiceTests
    {
        private readonly Mock<IPatientArvRegimenRepo> _mockParRepo;
        private readonly Mock<HivSystemApiContext> _mockContext;
        private readonly PatientArvRegimenService _service;
        private readonly Mock<INotificationRepo> _mockNotiRepo;
        private readonly Mock<IPatientArvMedicationRepo> _mockPamRepo;

        public PatientArvRegimenServiceTests()
        {
            _mockParRepo = new Mock<IPatientArvRegimenRepo>();
            _mockContext = new Mock<HivSystemApiContext>();
            _mockNotiRepo = new Mock<INotificationRepo>();
            _mockPamRepo = new Mock<IPatientArvMedicationRepo>();
            _service = new PatientArvRegimenService(_mockParRepo.Object, _mockPamRepo.Object, _mockNotiRepo.Object, _mockContext.Object);
        }

        #region Helper Methods

        private PatientArvRegimen CreateMockEntity(int id = 1, int pmrId = 1,
            byte? regimenLevel = 1, byte? regimenStatus = 1, double? totalCost = 100.0)
        {
            return new PatientArvRegimen
            {
                ParId = id,
                PmrId = pmrId,
                Notes = "Test Notes",
                RegimenLevel = regimenLevel,
                CreatedAt = DateTime.UtcNow,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                RegimenStatus = regimenStatus,
                TotalCost = totalCost
            };
        }

        private PatientArvRegimenRequestDTO CreateMockRequestDTO(int pmrId = 1,
            byte? regimenLevel = 1, byte? regimenStatus = 1, double? totalCost = 100.0)
        {
            return new PatientArvRegimenRequestDTO
            {
                PatientMedRecordId = pmrId,
                Notes = "Test Notes",
                RegimenLevel = regimenLevel,
                CreatedAt = DateTime.UtcNow,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                RegimenStatus = regimenStatus,
                TotalCost = totalCost
            };
        }

        private PatientArvRegimenPatchDTO CreateMockPatchDTO(byte? regimenLevel = 2,
            byte? regimenStatus = 2, double? totalCost = 200.0)
        {
            return new PatientArvRegimenPatchDTO
            {
                Notes = "Updated Notes",
                RegimenLevel = regimenLevel,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(31)),
                RegimenStatus = regimenStatus,
                TotalCost = totalCost
            };
        }

        #endregion

        #region CreatePatientArvRegimenAsync Tests

        /**
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_ValidDTO_ReturnsCreatedDTO()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO();
            var expectedEntity = CreateMockEntity();

            _mockContext.Setup(c => c.PatientMedicalRecords.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PatientMedicalRecord, bool>>>(), default))
                      .ReturnsAsync(true);

            _mockParRepo.Setup(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()))
                    .ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.CreatePatientArvRegimenAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEntity.ParId, result.PatientArvRegiId);
            Assert.Equal(inputDto.PatientMedRecordId, result.PatientMedRecordId);
            Assert.Equal(inputDto.Notes, result.Notes);
            Assert.Equal(inputDto.RegimenLevel, result.RegimenLevel);
            Assert.Equal(inputDto.RegimenStatus, result.RegimenStatus);
            Assert.Equal(inputDto.TotalCost, result.TotalCost);

            _mockParRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Once);
        }
        **/

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_NullDTO_ThrowsArgumentNullException()
        {
            // Arrange
            PatientArvRegimenRequestDTO inputDto = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreatePatientArvRegimenAsync(inputDto));

            Assert.Equal("patientArvRegimen", exception.ParamName);
            _mockParRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_InvalidPmrId_ThrowsArgumentException(int invalidPmrId)
        {
            // Arrange
            var inputDto = CreateMockRequestDTO(pmrId: invalidPmrId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreatePatientArvRegimenAsync(inputDto));

            Assert.Contains("Invalid Patient Medical Record ID", exception.Message);
            _mockParRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        /**
        [Theory]
        [InlineData((byte)0)]
        [InlineData((byte)5)]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_InvalidRegimenLevel_ThrowsArgumentException(byte invalidLevel)
        {
            // Arrange
            var inputDto = CreateMockRequestDTO(regimenLevel: invalidLevel);

            _mockContext.Setup(c => c.PatientMedicalRecords.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PatientMedicalRecord, bool>>>(), default))
                      .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreatePatientArvRegimenAsync(inputDto));

            Assert.Contains("RegimenLevel must be between 1 and 4", exception.Message);
            _mockParRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Theory]
        [InlineData((byte)0)]
        [InlineData((byte)6)]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_InvalidRegimenStatus_ThrowsArgumentException(byte invalidStatus)
        {
            // Arrange
            var inputDto = CreateMockRequestDTO(regimenStatus: invalidStatus);

            _mockContext.Setup(c => c.PatientMedicalRecords.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PatientMedicalRecord, bool>>>(), default))
                      .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreatePatientArvRegimenAsync(inputDto));

            Assert.Contains("RegimenStatus must be between 1 and 5", exception.Message);
            _mockParRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_InvalidDateRange_ThrowsArgumentException()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO();
            inputDto.StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
            inputDto.EndDate = DateOnly.FromDateTime(DateTime.Today);

            _mockContext.Setup(c => c.PatientMedicalRecords.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PatientMedicalRecord, bool>>>(), default))
                      .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreatePatientArvRegimenAsync(inputDto));

            Assert.Contains("Start date cannot be later than end date", exception.Message);
            _mockParRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }
        **/
        #endregion

        #region GetPatientArvRegimenByIdAsync Tests

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimenByIdAsync_ValidId_ReturnsDTO()
        {
            // Arrange
            var expectedEntity = CreateMockEntity();
            _mockParRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1))
                    .ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.GetPatientArvRegimenByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEntity.ParId, result.PatientArvRegiId);
            Assert.Equal(expectedEntity.PmrId, result.PatientMedRecordId);
            Assert.Equal(expectedEntity.Notes, result.Notes);
            _mockParRepo.Verify(r => r.GetPatientArvRegimenByIdAsync(1), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimenByIdAsync_InvalidId_ThrowsArgumentException(int invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetPatientArvRegimenByIdAsync(invalidId));

            Assert.Contains("Invalid Patient ARV Regimen ID", exception.Message);
            _mockParRepo.Verify(r => r.GetPatientArvRegimenByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimenByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            _mockParRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(999))
                    .ReturnsAsync((PatientArvRegimen)null);

            // Act
            var result = await _service.GetPatientArvRegimenByIdAsync(999);

            // Assert
            Assert.Null(result);
            _mockParRepo.Verify(r => r.GetPatientArvRegimenByIdAsync(999), Times.Once);
        }

        #endregion

        #region GetAllPatientArvRegimensAsync Tests

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetAllPatientArvRegimensAsync_ReturnsAllRegimens_Success()
        {
            // Arrange
            var entities = new List<PatientArvRegimen>
            {
                CreateMockEntity(1, 1),
                CreateMockEntity(2, 2),
                CreateMockEntity(3, 3)
            };

            _mockParRepo.Setup(r => r.GetAllPatientArvRegimensAsync())
                    .ReturnsAsync(entities);

            // Act
            var result = await _service.GetAllPatientArvRegimensAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].PatientArvRegiId);
            Assert.Equal(2, result[1].PatientArvRegiId);
            Assert.Equal(3, result[2].PatientArvRegiId);

            _mockParRepo.Verify(r => r.GetAllPatientArvRegimensAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetAllPatientArvRegimensAsync_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockParRepo.Setup(r => r.GetAllPatientArvRegimensAsync())
                    .ReturnsAsync(new List<PatientArvRegimen>());

            // Act
            var result = await _service.GetAllPatientArvRegimensAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockParRepo.Verify(r => r.GetAllPatientArvRegimensAsync(), Times.Once);
        }

        #endregion

        #region DeletePatientArvRegimenAsync Tests

        /**
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task DeletePatientArvRegimenAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            var existingRegimen = CreateMockEntity();
            _mockParRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1))
                    .ReturnsAsync(existingRegimen);

            _mockContext.Setup(c => c.PatientArvMedications.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PatientArvMedication, bool>>>(), default))
                      .ReturnsAsync(false);

            _mockParRepo.Setup(r => r.DeletePatientArvRegimenAsync(1))
                    .ReturnsAsync(true);

            // Act
            var result = await _service.DeletePatientArvRegimenAsync(1);

            // Assert
            Assert.True(result);
            _mockParRepo.Verify(r => r.DeletePatientArvRegimenAsync(1), Times.Once);
        }
        **/

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task DeletePatientArvRegimenAsync_InvalidId_ThrowsArgumentException(int invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.DeletePatientArvRegimenAsync(invalidId));

            Assert.Contains("Invalid Patient ARV Regimen ID", exception.Message);
            _mockParRepo.Verify(r => r.DeletePatientArvRegimenAsync(It.IsAny<int>()), Times.Never);
        }

        /**
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task DeletePatientArvRegimenAsync_HasDependentMedications_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingRegimen = CreateMockEntity();
            _mockParRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1))
                    .ReturnsAsync(existingRegimen);

            _mockContext.Setup(c => c.PatientArvMedications.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PatientArvMedication, bool>>>(), default))
                      .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.DeletePatientArvRegimenAsync(1));

            Assert.Contains("Cannot delete ARV Regimen with ID 1 because it has associated medications", exception.Message);
            _mockParRepo.Verify(r => r.DeletePatientArvRegimenAsync(It.IsAny<int>()), Times.Never);
        }
        **/
        #endregion

        #region PatchPatientArvRegimenAsync Tests

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task PatchPatientArvRegimenAsync_ValidInput_ReturnsUpdatedDTO()
        {
            // Arrange
            var existingRegimen = CreateMockEntity(regimenStatus: 1); // Not completed
            var patchDto = CreateMockPatchDTO();
            var updatedEntity = CreateMockEntity();

            _mockParRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1))
                    .ReturnsAsync(existingRegimen);

            _mockParRepo.Setup(r => r.UpdatePatientArvRegimenAsync(1, It.IsAny<PatientArvRegimen>()))
                    .ReturnsAsync(updatedEntity);

            // Act
            var result = await _service.PatchPatientArvRegimenAsync(1, patchDto);

            // Assert
            Assert.NotNull(result);
            _mockParRepo.Verify(r => r.UpdatePatientArvRegimenAsync(1, It.IsAny<PatientArvRegimen>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task PatchPatientArvRegimenAsync_CompletedRegimen_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingRegimen = CreateMockEntity(regimenStatus: 5); // Completed
            var patchDto = CreateMockPatchDTO();

            _mockParRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1))
                    .ReturnsAsync(existingRegimen);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.PatchPatientArvRegimenAsync(1, patchDto));

            Assert.Contains("Cannot update ARV Regimen with ID 1 because it is marked as Completed", exception.Message);
            _mockParRepo.Verify(r => r.UpdatePatientArvRegimenAsync(It.IsAny<int>(), It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task PatchPatientArvRegimenAsync_ActiveRegimenStartDateUpdate_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingRegimen = CreateMockEntity(regimenStatus: 2); // Active
            var patchDto = CreateMockPatchDTO();
            patchDto.StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

            _mockParRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1))
                    .ReturnsAsync(existingRegimen);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.PatchPatientArvRegimenAsync(1, patchDto));

            Assert.Contains("Cannot update StartDate for ARV Regimen with ID 1 because it is Active", exception.Message);
            _mockParRepo.Verify(r => r.UpdatePatientArvRegimenAsync(It.IsAny<int>(), It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task PatchPatientArvRegimenAsync_NullDTO_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.PatchPatientArvRegimenAsync(1, null));

            Assert.Equal("patientArvRegimen", exception.ParamName);
            _mockParRepo.Verify(r => r.UpdatePatientArvRegimenAsync(It.IsAny<int>(), It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        #endregion

        #region GetPatientArvRegimensByPatientIdAsync Tests

        /**
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimensByPatientIdAsync_ValidPatientId_ReturnsRegimens()
        {
            // Arrange
            var entities = new List<PatientArvRegimen>
            {
                CreateMockEntity(1, 1),
                CreateMockEntity(2, 2)
            };

            _mockContext.Setup(c => c.Patients.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Patient, bool>>>(), default))
                      .ReturnsAsync(true);

            _mockParRepo.Setup(r => r.GetPatientArvRegimensByPatientIdAsync(1))
                    .ReturnsAsync(entities);

            // Act
            var result = await _service.GetPatientArvRegimensByPatientIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockParRepo.Verify(r => r.GetPatientArvRegimensByPatientIdAsync(1), Times.Once);
        }
        **/

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimensByPatientIdAsync_InvalidPatientId_ThrowsArgumentException(int invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetPatientArvRegimensByPatientIdAsync(invalidId));

            Assert.Contains("Invalid Patient ID", exception.Message);
            _mockParRepo.Verify(r => r.GetPatientArvRegimensByPatientIdAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion
    }
}