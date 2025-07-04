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
        private readonly Mock<IPatientArvRegimenRepo> _mockRepo;
        private readonly Mock<HivSystemApiContext> _mockContext;
        private readonly PatientArvRegimenService _service;

        public PatientArvRegimenServiceTests()
        {
            _mockRepo = new Mock<IPatientArvRegimenRepo>();
            _mockContext = new Mock<HivSystemApiContext>();
            _service = new PatientArvRegimenService(_mockRepo.Object, _mockContext.Object);
        }

        #region Helper Methods
        private PatientArvRegimen CreateMockEntity(int parId = 1, int pmrId = 1, byte regimenLevel = 1, byte regimenStatus = 1)
        {
            return new PatientArvRegimen
            {
                ParId = parId,
                PmrId = pmrId,
                Notes = "Test regimen",
                RegimenLevel = regimenLevel,
                RegimenStatus = regimenStatus,
                CreatedAt = DateTime.UtcNow,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                TotalCost = 50000,
                Pmr = new PatientMedicalRecord { PmrId = pmrId, PtnId = 1 },
                PatientArvMedications = new List<PatientArvMedication>()
            };
        }

        private PatientArvRegimenRequestDTO CreateMockRequestDTO(int pmrId = 1, byte regimenLevel = 1, byte regimenStatus = 1)
        {
            return new PatientArvRegimenRequestDTO
            {
                PatientMedRecordId = pmrId,
                Notes = "Test regimen",
                RegimenLevel = regimenLevel,
                RegimenStatus = regimenStatus,
                CreatedAt = DateTime.UtcNow,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                TotalCost = 50000
            };
        }

        private void SetupMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            if (typeof(T) == typeof(PatientMedicalRecord))
                _mockContext.Setup(c => c.PatientMedicalRecords).Returns(mockSet.Object);
            else if (typeof(T) == typeof(Patient))
                _mockContext.Setup(c => c.Patients).Returns(mockSet.Object);
            else if (typeof(T) == typeof(PatientArvMedication))
                _mockContext.Setup(c => c.PatientArvMedications).Returns(mockSet.Object);
        }
        #endregion

        #region CreatePatientArvRegimenAsync Tests
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_ValidDTO_ReturnsCreatedDTO()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO();
            var expectedEntity = CreateMockEntity();
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });
            _mockRepo.Setup(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>())).ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.CreatePatientArvRegimenAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inputDto.PatientMedRecordId, result.PatientMedRecordId);
            Assert.Equal(inputDto.Notes, result.Notes);
            Assert.Equal(inputDto.RegimenLevel, result.RegimenLevel);
            Assert.Equal(inputDto.RegimenStatus, result.RegimenStatus);
            Assert.Equal(inputDto.TotalCost, result.TotalCost);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_NullDTO_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreatePatientArvRegimenAsync(null));
            Assert.Equal("patientArvRegimen", exception.ParamName);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_InvalidPmrId_ThrowsArgumentException(int pmrId)
        {
            // Arrange
            var inputDto = CreateMockRequestDTO(pmrId: pmrId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreatePatientArvRegimenAsync(inputDto));
            Assert.Contains("Invalid Patient Medical Record ID", exception.Message);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_NonExistentPmrId_ThrowsInvalidOperationException()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO();
            SetupMockDbSet(new List<PatientMedicalRecord>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePatientArvRegimenAsync(inputDto));
            Assert.Contains("Patient Medical Record with ID 1 does not exist", exception.Message);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_InvalidRegimenLevel_ThrowsArgumentException(byte regimenLevel)
        {
            // Arrange
            var inputDto = CreateMockRequestDTO(regimenLevel: regimenLevel);
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreatePatientArvRegimenAsync(inputDto));
            Assert.Contains("RegimenLevel must be between 1 and 4", exception.Message);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_InvalidRegimenStatus_ThrowsArgumentException(byte regimenStatus)
        {
            // Arrange
            var inputDto = CreateMockRequestDTO(regimenStatus: regimenStatus);
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreatePatientArvRegimenAsync(inputDto));
            Assert.Contains("RegimenStatus must be between 1 and 5", exception.Message);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_InvalidDateLogic_ThrowsArgumentException()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO();
            inputDto.StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
            inputDto.EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreatePatientArvRegimenAsync(inputDto));
            Assert.Contains("Start date cannot be later than end date", exception.Message);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_DbUpdateException_ThrowsInvalidOperationException()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO();
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });
            _mockRepo.Setup(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()))
                .ThrowsAsync(new DbUpdateException("Database error", new Exception()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePatientArvRegimenAsync(inputDto));
            Assert.Contains("Database error while creating ARV regimen", exception.Message);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Once);
        }
        #endregion

        #region GetAllPatientArvRegimensAsync Tests
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetAllPatientArvRegimensAsync_ReturnsAllRegimens()
        {
            // Arrange
            var entities = new List<PatientArvRegimen>
            {
                CreateMockEntity(1, 1),
                CreateMockEntity(2, 2)
            };
            _mockRepo.Setup(r => r.GetAllPatientArvRegimensAsync()).ReturnsAsync(entities);

            // Act
            var result = await _service.GetAllPatientArvRegimensAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].PatientArvRegiId);
            Assert.Equal(2, result[1].PatientArvRegiId);
            _mockRepo.Verify(r => r.GetAllPatientArvRegimensAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetAllPatientArvRegimensAsync_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllPatientArvRegimensAsync()).ReturnsAsync(new List<PatientArvRegimen>());

            // Act
            var result = await _service.GetAllPatientArvRegimensAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockRepo.Verify(r => r.GetAllPatientArvRegimensAsync(), Times.Once);
        }
        #endregion

        #region GetPatientArvRegimenByIdAsync Tests
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimenByIdAsync_ValidId_ReturnsDTO()
        {
            // Arrange
            var expectedEntity = CreateMockEntity();
            _mockRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1)).ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.GetPatientArvRegimenByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PatientArvRegiId);
            Assert.Equal(expectedEntity.PmrId, result.PatientMedRecordId);
            Assert.Equal(expectedEntity.Notes, result.Notes);
            _mockRepo.Verify(r => r.GetPatientArvRegimenByIdAsync(1), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimenByIdAsync_InvalidId_ThrowsArgumentException(int parId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPatientArvRegimenByIdAsync(parId));
            Assert.Contains("Invalid Patient ARV Regimen ID", exception.Message);
            _mockRepo.Verify(r => r.GetPatientArvRegimenByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimenByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(999)).ReturnsAsync((PatientArvRegimen)null);

            // Act
            var result = await _service.GetPatientArvRegimenByIdAsync(999);

            // Assert
            Assert.Null(result);
            _mockRepo.Verify(r => r.GetPatientArvRegimenByIdAsync(999), Times.Once);
        }
        #endregion

        #region UpdatePatientArvRegimenAsync Tests
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task UpdatePatientArvRegimenAsync_ValidInput_ReturnsUpdatedDTO()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO(regimenLevel: 2);
            var updatedEntity = CreateMockEntity(regimenLevel: 2);
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });
            _mockRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1)).ReturnsAsync(CreateMockEntity());
            _mockRepo.Setup(r => r.UpdatePatientArvRegimenAsync(1, It.IsAny<PatientArvRegimen>())).ReturnsAsync(updatedEntity);

            // Act
            var result = await _service.UpdatePatientArvRegimenAsync(1, inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.RegimenLevel);
            Assert.Equal(inputDto.Notes, result.Notes);
            _mockRepo.Verify(r => r.UpdatePatientArvRegimenAsync(1, It.IsAny<PatientArvRegimen>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task UpdatePatientArvRegimenAsync_NullDTO_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdatePatientArvRegimenAsync(1, null));
            Assert.Equal("patientArvRegimen", exception.ParamName);
            _mockRepo.Verify(r => r.UpdatePatientArvRegimenAsync(It.IsAny<int>(), It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task UpdatePatientArvRegimenAsync_NonExistentRegimen_ThrowsInvalidOperationException()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO();
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });
            _mockRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(999)).ReturnsAsync((PatientArvRegimen)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdatePatientArvRegimenAsync(999, inputDto));
            Assert.Contains("Patient ARV Regimen with ID 999 does not exist", exception.Message);
            _mockRepo.Verify(r => r.UpdatePatientArvRegimenAsync(It.IsAny<int>(), It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task UpdatePatientArvRegimenAsync_ConcurrencyException_ThrowsInvalidOperationException()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO();
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });
            _mockRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1)).ReturnsAsync(CreateMockEntity());
            _mockRepo.Setup(r => r.UpdatePatientArvRegimenAsync(1, It.IsAny<PatientArvRegimen>()))
                .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency violation"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdatePatientArvRegimenAsync(1, inputDto));
            Assert.Contains("Concurrency error updating ARV regimen", exception.Message);
            _mockRepo.Verify(r => r.UpdatePatientArvRegimenAsync(1, It.IsAny<PatientArvRegimen>()), Times.Once);
        }
        #endregion

        #region DeletePatientArvRegimenAsync Tests
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task DeletePatientArvRegimenAsync_ValidIdNoDependencies_ReturnsTrue()
        {
            // Arrange
            SetupMockDbSet(new List<PatientArvMedication>());
            _mockRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1)).ReturnsAsync(CreateMockEntity());
            _mockRepo.Setup(r => r.DeletePatientArvRegimenAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _service.DeletePatientArvRegimenAsync(1);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(r => r.DeletePatientArvRegimenAsync(1), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task DeletePatientArvRegimenAsync_InvalidId_ThrowsArgumentException(int parId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.DeletePatientArvRegimenAsync(parId));
            Assert.Contains("Invalid Patient ARV Regimen ID", exception.Message);
            _mockRepo.Verify(r => r.DeletePatientArvRegimenAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task DeletePatientArvRegimenAsync_HasDependencies_ThrowsInvalidOperationException()
        {
            // Arrange
            SetupMockDbSet(new List<PatientArvMedication> { new PatientArvMedication { ParId = 1 } });
            _mockRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(1)).ReturnsAsync(CreateMockEntity());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeletePatientArvRegimenAsync(1));
            Assert.Contains("Cannot delete ARV Regimen with ID 1 because it has associated medications", exception.Message);
            _mockRepo.Verify(r => r.DeletePatientArvRegimenAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task DeletePatientArvRegimenAsync_NonExistentRegimen_ThrowsInvalidOperationException()
        {
            // Arrange
            SetupMockDbSet(new List<PatientArvMedication>());
            _mockRepo.Setup(r => r.GetPatientArvRegimenByIdAsync(999)).ReturnsAsync((PatientArvRegimen)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeletePatientArvRegimenAsync(999));
            Assert.Contains("Patient ARV Regimen with ID 999 does not exist", exception.Message);
            _mockRepo.Verify(r => r.DeletePatientArvRegimenAsync(It.IsAny<int>()), Times.Never);
        }
        #endregion

        #region GetPatientArvRegimensByPatientIdAsync Tests
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimensByPatientIdAsync_ValidPatientId_ReturnsRegimens()
        {
            // Arrange
            var entities = new List<PatientArvRegimen> { CreateMockEntity(1, 1), CreateMockEntity(2, 1) };
            SetupMockDbSet(new List<Patient> { new Patient { PtnId = 1 } });
            _mockRepo.Setup(r => r.GetPatientArvRegimensByPatientIdAsync(1)).ReturnsAsync(entities);

            // Act
            var result = await _service.GetPatientArvRegimensByPatientIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].PatientArvRegiId);
            Assert.Equal(2, result[1].PatientArvRegiId);
            _mockRepo.Verify(r => r.GetPatientArvRegimensByPatientIdAsync(1), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimensByPatientIdAsync_InvalidPatientId_ThrowsArgumentException(int patientId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPatientArvRegimensByPatientIdAsync(patientId));
            Assert.Contains("Invalid Patient ID", exception.Message);
            _mockRepo.Verify(r => r.GetPatientArvRegimensByPatientIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimensByPatientIdAsync_NonExistentPatient_ThrowsInvalidOperationException()
        {
            // Arrange
            SetupMockDbSet(new List<Patient>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetPatientArvRegimensByPatientIdAsync(1));
            Assert.Contains("Patient with ID 1 does not exist", exception.Message);
            _mockRepo.Verify(r => r.GetPatientArvRegimensByPatientIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPatientArvRegimensByPatientIdAsync_NoRegimens_ReturnsEmptyList()
        {
            // Arrange
            SetupMockDbSet(new List<Patient> { new Patient { PtnId = 1 } });
            _mockRepo.Setup(r => r.GetPatientArvRegimensByPatientIdAsync(1)).ReturnsAsync(new List<PatientArvRegimen>());

            // Act
            var result = await _service.GetPatientArvRegimensByPatientIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockRepo.Verify(r => r.GetPatientArvRegimensByPatientIdAsync(1), Times.Once);
        }
        #endregion

        #region GetPersonalArvRegimensAsync Tests
        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPersonalArvRegimensAsync_ValidPersonalId_ReturnsRegimens()
        {
            // Arrange
            var entities = new List<PatientArvRegimen> { CreateMockEntity(1, 1), CreateMockEntity(2, 1) };
            SetupMockDbSet(new List<Patient> { new Patient { PtnId = 1 } });
            _mockRepo.Setup(r => r.GetPersonalArvRegimensAsync(1)).ReturnsAsync(entities);

            // Act
            var result = await _service.GetPersonalArvRegimensAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].PatientArvRegiId);
            Assert.Equal(2, result[1].PatientArvRegiId);
            _mockRepo.Verify(r => r.GetPersonalArvRegimensAsync(1), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPersonalArvRegimensAsync_InvalidPersonalId_ThrowsArgumentException(int personalId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPersonalArvRegimensAsync(personalId));
            Assert.Contains("Invalid Personal ID", exception.Message);
            _mockRepo.Verify(r => r.GetPersonalArvRegimensAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetPersonalArvRegimensAsync_NonExistentPatient_ThrowsInvalidOperationException()
        {
            // Arrange
            SetupMockDbSet(new List<Patient>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetPersonalArvRegimensAsync(1));
            Assert.Contains("Patient with ID 1 does not exist", exception.Message);
            _mockRepo.Verify(r => r.GetPersonalArvRegimensAsync(It.IsAny<int>()), Times.Never);
        }
        #endregion

        #region Additional Tests
        [Fact]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_NegativeTotalCost_ThrowsArgumentException()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO();
            inputDto.TotalCost = -100;
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreatePatientArvRegimenAsync(inputDto));
            Assert.Contains("TotalCost cannot be negative", exception.Message);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_MaxLengthNotes_ReturnsCreatedDTO()
        {
            // Arrange
            var maxLengthNotes = new string('A', 200);
            var inputDto = CreateMockRequestDTO();
            inputDto.Notes = maxLengthNotes;
            var expectedEntity = CreateMockEntity();
            expectedEntity.Notes = maxLengthNotes;
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });
            _mockRepo.Setup(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>())).ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.CreatePatientArvRegimenAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(maxLengthNotes, result.Notes);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreatePatientArvRegimenAsync_OverlappingActiveRegimen_ThrowsInvalidOperationException()
        {
            // Arrange
            var inputDto = CreateMockRequestDTO();
            SetupMockDbSet(new List<PatientMedicalRecord> { new PatientMedicalRecord { PmrId = 1, PtnId = 1 } });
            SetupMockDbSet(new List<PatientArvRegimen> { CreateMockEntity(2, 1, regimenStatus: 2) }); // Active regimen
            _mockContext.Setup(c => c.PatientArvRegimen).Returns(Mock.Of<DbSet<PatientArvRegimen>>());
            _mockContext.Setup(c => c.PatientArvRegimen.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PatientArvRegimen, bool>>>()))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePatientArvRegimenAsync(inputDto));
            Assert.Contains("An active regimen already exists for this patient medical record", exception.Message);
            _mockRepo.Verify(r => r.CreatePatientArvRegimenAsync(It.IsAny<PatientArvRegimen>()), Times.Never);
        }
        #endregion
    }
}