using HIV_System_API_BOs;
using HIV_System_API_DTOs.ArvMedicationDetailDTO;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Implements;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HIV_System_API_Tests
{
    public class ArvMedicationDetailServiceTests
    {
        private readonly Mock<IArvMedicationDetailRepo> _mockRepo;
        private readonly ArvMedicationDetailService _service;

        public ArvMedicationDetailServiceTests()
        {
            _mockRepo = new Mock<IArvMedicationDetailRepo>();
            _service = new ArvMedicationDetailService(_mockRepo.Object);
        }

        #region Helper Methods

        private ArvMedicationDetail CreateMockEntity(int id = 1, string name = "TestMed",
            string description = "Test Description", string dosage = "10mg",
            double price = 100.0, string manufacturer = "Test Manufacturer")
        {
            return new ArvMedicationDetail
            {
                AmdId = id,
                MedName = name,
                MedDescription = description,
                Dosage = dosage,
                Price = price,
                Manufactorer = manufacturer
            };
        }

        private ArvMedicationDetailResponseDTO CreateMockDTO(string name = "TestMed",
            string description = "Test Description", string dosage = "10mg",
            double price = 100.0, string manufacturer = "Test Manufacturer")
        {
            return new ArvMedicationDetailResponseDTO
            {
                ARVMedicationName = name,
                ARVMedicationDescription = description,
                ARVMedicationDosage = dosage,
                ARVMedicationPrice = price,
                ARVMedicationManufacturer = manufacturer
            };
        }

        #endregion

        #region CreateArvMedicationDetailAsync Tests

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreateArvMedicationDetailAsync_ValidDTO_ReturnsCreatedDTO()
        {
            // Arrange
            var inputDto = CreateMockDTO();
            var expectedEntity = CreateMockEntity();

            _mockRepo.Setup(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()))
                    .ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.CreateArvMedicationDetailAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inputDto.ARVMedicationName, result.ARVMedicationName);
            Assert.Equal(inputDto.ARVMedicationDescription, result.ARVMedicationDescription);
            Assert.Equal(inputDto.ARVMedicationDosage, result.ARVMedicationDosage);
            Assert.Equal(inputDto.ARVMedicationPrice, result.ARVMedicationPrice);
            Assert.Equal(inputDto.ARVMedicationManufacturer, result.ARVMedicationManufacturer);

            _mockRepo.Verify(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreateArvMedicationDetailAsync_NullDTO_ThrowsArgumentNullException()
        {
            // Arrange
            ArvMedicationDetailResponseDTO inputDto = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreateArvMedicationDetailAsync(inputDto));

            Assert.Equal("dto", exception.ParamName);
            _mockRepo.Verify(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreateArvMedicationDetailAsync_InvalidName_ThrowsArgumentException(string invalidName)
        {
            // Arrange
            var inputDto = CreateMockDTO(name: invalidName);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateArvMedicationDetailAsync(inputDto));

            Assert.Contains("ARV Medication Name is required", exception.Message);
            _mockRepo.Verify(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreateArvMedicationDetailAsync_NegativePrice_ThrowsArgumentException()
        {
            // Arrange
            var inputDto = CreateMockDTO(price: -50.0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateArvMedicationDetailAsync(inputDto));

            Assert.Contains("ARV Medication Price cannot be negative", exception.Message);
            _mockRepo.Verify(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreateArvMedicationDetailAsync_ZeroPrice_ReturnsCreatedDTO()
        {
            // Arrange
            var inputDto = CreateMockDTO(price: 0.0);
            var expectedEntity = CreateMockEntity(price: 0.0);

            _mockRepo.Setup(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()))
                    .ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.CreateArvMedicationDetailAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result.ARVMedicationPrice);
            _mockRepo.Verify(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreateArvMedicationDetailAsync_RepoThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = CreateMockDTO();
            _mockRepo.Setup(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()))
                    .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateArvMedicationDetailAsync(inputDto));

            Assert.Equal("Database error", exception.Message);
            _mockRepo.Verify(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()), Times.Once);
        }

        #endregion

        #region GetAllArvMedicationDetailsAsync Tests

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetAllArvMedicationDetailsAsync_ReturnsAllMedications_Success()
        {
            // Arrange
            var entities = new List<ArvMedicationDetail>
            {
                CreateMockEntity(1, "Med1"),
                CreateMockEntity(2, "Med2"),
                CreateMockEntity(3, "Med3")
            };

            _mockRepo.Setup(r => r.GetAllArvMedicationDetailsAsync())
                    .ReturnsAsync(entities);

            // Act
            var result = await _service.GetAllArvMedicationDetailsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Med1", result[0].ARVMedicationName);
            Assert.Equal("Med2", result[1].ARVMedicationName);
            Assert.Equal("Med3", result[2].ARVMedicationName);

            _mockRepo.Verify(r => r.GetAllArvMedicationDetailsAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetAllArvMedicationDetailsAsync_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllArvMedicationDetailsAsync())
                    .ReturnsAsync(new List<ArvMedicationDetail>());

            // Act
            var result = await _service.GetAllArvMedicationDetailsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockRepo.Verify(r => r.GetAllArvMedicationDetailsAsync(), Times.Once);
        }

        #endregion

        #region GetArvMedicationDetailByIdAsync Tests

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetArvMedicationDetailByIdAsync_ValidId_ReturnsDTO()
        {
            // Arrange
            var expectedEntity = CreateMockEntity();
            _mockRepo.Setup(r => r.GetArvMedicationDetailByIdAsync(1))
                    .ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.GetArvMedicationDetailByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEntity.MedName, result.ARVMedicationName);
            Assert.Equal(expectedEntity.Price, result.ARVMedicationPrice);
            _mockRepo.Verify(r => r.GetArvMedicationDetailByIdAsync(1), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetArvMedicationDetailByIdAsync_InvalidId_ThrowsArgumentException(int invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetArvMedicationDetailByIdAsync(invalidId));

            Assert.Contains("Invalid ARV Medication Detail ID", exception.Message);
            _mockRepo.Verify(r => r.GetArvMedicationDetailByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task GetArvMedicationDetailByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetArvMedicationDetailByIdAsync(999))
                    .ReturnsAsync((ArvMedicationDetail)null);

            // Act
            var result = await _service.GetArvMedicationDetailByIdAsync(999);

            // Assert
            Assert.Null(result);
            _mockRepo.Verify(r => r.GetArvMedicationDetailByIdAsync(999), Times.Once);
        }

        #endregion

        #region UpdateArvMedicationDetailAsync Tests

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task UpdateArvMedicationDetailAsync_ValidInput_ReturnsUpdatedDTO()
        {
            // Arrange
            var inputDto = CreateMockDTO(name: "Updated Med");
            var updatedEntity = CreateMockEntity(name: "Updated Med");

            _mockRepo.Setup(r => r.UpdateArvMedicationDetailAsync(1, It.IsAny<ArvMedicationDetail>()))
                    .ReturnsAsync(updatedEntity);

            // Act
            var result = await _service.UpdateArvMedicationDetailAsync(1, inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Med", result.ARVMedicationName);
            _mockRepo.Verify(r => r.UpdateArvMedicationDetailAsync(1, It.IsAny<ArvMedicationDetail>()), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task UpdateArvMedicationDetailAsync_InvalidId_ThrowsArgumentException(int invalidId)
        {
            // Arrange
            var inputDto = CreateMockDTO();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.UpdateArvMedicationDetailAsync(invalidId, inputDto));

            Assert.Contains("Invalid ARV Medication Detail ID", exception.Message);
            _mockRepo.Verify(r => r.UpdateArvMedicationDetailAsync(It.IsAny<int>(), It.IsAny<ArvMedicationDetail>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task UpdateArvMedicationDetailAsync_NullDTO_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.UpdateArvMedicationDetailAsync(1, null));

            Assert.Equal("dto", exception.ParamName);
            _mockRepo.Verify(r => r.UpdateArvMedicationDetailAsync(It.IsAny<int>(), It.IsAny<ArvMedicationDetail>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task UpdateArvMedicationDetailAsync_RepoReturnsNull_ReturnsNull()
        {
            // Arrange
            var inputDto = CreateMockDTO();
            _mockRepo.Setup(r => r.UpdateArvMedicationDetailAsync(1, It.IsAny<ArvMedicationDetail>()))
                    .ReturnsAsync((ArvMedicationDetail)null);

            // Act
            var result = await _service.UpdateArvMedicationDetailAsync(1, inputDto);

            // Assert
            Assert.Null(result);
            _mockRepo.Verify(r => r.UpdateArvMedicationDetailAsync(1, It.IsAny<ArvMedicationDetail>()), Times.Once);
        }

        #endregion

        #region DeleteArvMedicationDetailAsync Tests

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task DeleteArvMedicationDetailAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            _mockRepo.Setup(r => r.DeleteArvMedicationDetailAsync(1))
                    .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteArvMedicationDetailAsync(1);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(r => r.DeleteArvMedicationDetailAsync(1), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task DeleteArvMedicationDetailAsync_InvalidId_ThrowsArgumentException(int invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.DeleteArvMedicationDetailAsync(invalidId));

            Assert.Contains("Invalid ARV Medication Detail ID", exception.Message);
            _mockRepo.Verify(r => r.DeleteArvMedicationDetailAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task DeleteArvMedicationDetailAsync_NotFound_ReturnsFalse()
        {
            // Arrange
            _mockRepo.Setup(r => r.DeleteArvMedicationDetailAsync(999))
                    .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteArvMedicationDetailAsync(999);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(r => r.DeleteArvMedicationDetailAsync(999), Times.Once);
        }

        #endregion

        #region SearchArvMedicationDetailsByNameAsync Tests

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task SearchArvMedicationDetailsByNameAsync_ValidSearchTerm_ReturnsMatchingResults()
        {
            // Arrange
            var searchTerm = "Test";
            var entities = new List<ArvMedicationDetail>
            {
                CreateMockEntity(1, "TestMed1"),
                CreateMockEntity(2, "TestMed2")
            };

            _mockRepo.Setup(r => r.SearchArvMedicationDetailsByNameAsync(searchTerm))
                    .ReturnsAsync(entities);

            // Act
            var result = await _service.SearchArvMedicationDetailsByNameAsync(searchTerm);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("TestMed1", result[0].ARVMedicationName);
            Assert.Equal("TestMed2", result[1].ARVMedicationName);
            _mockRepo.Verify(r => r.SearchArvMedicationDetailsByNameAsync(searchTerm), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task SearchArvMedicationDetailsByNameAsync_InvalidSearchTerm_ThrowsArgumentException(string invalidSearchTerm)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.SearchArvMedicationDetailsByNameAsync(invalidSearchTerm));

            Assert.Contains("Search term cannot be empty", exception.Message);
            _mockRepo.Verify(r => r.SearchArvMedicationDetailsByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task SearchArvMedicationDetailsByNameAsync_NoResults_ReturnsEmptyList()
        {
            // Arrange
            var searchTerm = "NonExistent";
            _mockRepo.Setup(r => r.SearchArvMedicationDetailsByNameAsync(searchTerm))
                    .ReturnsAsync(new List<ArvMedicationDetail>());

            // Act
            var result = await _service.SearchArvMedicationDetailsByNameAsync(searchTerm);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockRepo.Verify(r => r.SearchArvMedicationDetailsByNameAsync(searchTerm), Times.Once);
        }

        #endregion

        #region Integration and Edge Cases Tests

        [Fact]
        [Trait("Category", "BlackBox_EP")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreateArvMedicationDetailAsync_ValidDTOWithNullOptionalFields_ReturnsCreatedDTO()
        {
            // Arrange
            var inputDto = new ArvMedicationDetailResponseDTO
            {
                ARVMedicationName = "TestMed",
                ARVMedicationPrice = 100.0,
                ARVMedicationDescription = null,
                ARVMedicationDosage = null,
                ARVMedicationManufacturer = null
            };

            var expectedEntity = new ArvMedicationDetail
            {
                AmdId = 1,
                MedName = "TestMed",
                Price = 100.0,
                MedDescription = null,
                Dosage = null,
                Manufactorer = null
            };

            _mockRepo.Setup(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()))
                    .ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.CreateArvMedicationDetailAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestMed", result.ARVMedicationName);
            Assert.Equal(100.0, result.ARVMedicationPrice);
            Assert.Null(result.ARVMedicationDescription);
            Assert.Null(result.ARVMedicationDosage);
            Assert.Null(result.ARVMedicationManufacturer);
        }

        [Fact]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreateArvMedicationDetailAsync_MaximumValidPrice_ReturnsCreatedDTO()
        {
            // Arrange
            var inputDto = CreateMockDTO(price: double.MaxValue);
            var expectedEntity = CreateMockEntity(price: double.MaxValue);

            _mockRepo.Setup(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()))
                    .ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.CreateArvMedicationDetailAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(double.MaxValue, result.ARVMedicationPrice);
        }

        [Fact]
        [Trait("Category", "BlackBox_BVA")]
        [Trait("Category", "WhiteBox_Statement_Decision")]
        public async Task CreateArvMedicationDetailAsync_SingleCharacterName_ReturnsCreatedDTO()
        {
            // Arrange
            var inputDto = CreateMockDTO(name: "A");
            var expectedEntity = CreateMockEntity(name: "A");

            _mockRepo.Setup(r => r.CreateArvMedicationDetailAsync(It.IsAny<ArvMedicationDetail>()))
                    .ReturnsAsync(expectedEntity);

            // Act
            var result = await _service.CreateArvMedicationDetailAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("A", result.ARVMedicationName);
        }

        #endregion
    }
}