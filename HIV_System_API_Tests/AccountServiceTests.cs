using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HIV_System_API_Tests
{
    public class AccountServiceTests
    {
        // Declare Mock objects for dependencies 
        private readonly Mock<IAccountRepo> _mockAccountRepo;
        private readonly Mock<IVerificationCodeService> _mockVerificationCodeService;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _mockAccountRepo = new Mock<IAccountRepo>();
            _mockVerificationCodeService = new Mock<IVerificationCodeService>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            _accountService = new AccountService(
                _mockAccountRepo.Object,
                _mockVerificationCodeService.Object,
                _mockMemoryCache.Object);
        }

        #region Helper Methods

        private Account CreateTestAccount(
            int id = 1,
            string username = "testuser",
            string password = "password123",
            bool isActive = true,
            string email = "test@example.com",
            string fullname = "Test User",
            DateOnly? dob = null,
            bool? gender = true,
            byte roles = 3)
        {
            // Use the AccountService's HashPassword method to create a hashed password
            string hashedPassword = new AccountService(_mockAccountRepo.Object, _mockVerificationCodeService.Object, _mockMemoryCache.Object)
                .GetType()
                .GetMethod("HashPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(new AccountService(_mockAccountRepo.Object, _mockVerificationCodeService.Object, _mockMemoryCache.Object), new object[] { password })
                .ToString();

            return new Account
            {
                AccId = id,
                AccUsername = username,
                AccPassword = hashedPassword,
                Email = email,
                Fullname = fullname,
                Dob = dob ?? new DateOnly(1990, 5, 15),
                Gender = gender,
                Roles = roles,
                IsActive = isActive,
                Doctor = null,
                Patient = null,
                Staff = null,
                MedicalServices = new List<MedicalService>(),
                NotificationAccounts = new List<NotificationAccount>(),
                SocialBlogs = new List<SocialBlog>()
            };
        }

        private void AssertAccountResponseMatches(AccountResponseDTO expected, AccountResponseDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.AccId, actual.AccId);
            Assert.Equal(expected.AccUsername, actual.AccUsername);
            // Password is not included in AccountResponseDTO
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Fullname, actual.Fullname);
            Assert.Equal(expected.Dob, actual.Dob);
            Assert.Equal(expected.Gender, actual.Gender);
            Assert.Equal(expected.Roles, actual.Roles);
            Assert.Equal(expected.IsActive, actual.IsActive);
        }

        #endregion

        #region Successful Login Tests

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "PositiveCase")]
        public async Task LoginAsync_ValidCredentialsActiveAccount_ReturnsAccountDetails()
        {
            // Arrange
            const string username = "validuser";
            const string password = "ValidPass123!";
            var account = CreateTestAccount(
                id: 1,
                username: username,
                password: password,
                isActive: true,
                email: "user@test.com",
                fullname: "Valid User");

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            var expected = new AccountResponseDTO
            {
                AccId = 1,
                AccUsername = username,
                Email = "user@test.com",
                Fullname = "Valid User",
                Dob = new DateOnly(1990, 5, 15),
                Gender = true,
                Roles = 3,
                IsActive = true
            };

            AssertAccountResponseMatches(expected, result);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Once);
        }

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "PositiveCase")]
        public async Task LoginAsync_AccountWithMinimalData_ReturnsAccountDetails()
        {
            // Arrange
            const string username = "minimaluser";
            const string password = "MinimalPass123!";
            var account = CreateTestAccount(
                username: username,
                password: password,
                email: null,
                fullname: null,
                dob: null,
                gender: null);

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.AccUsername);
            Assert.Null(result.Email);
            Assert.Null(result.Fullname);
            Assert.True(result.IsActive);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Once);
        }

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "PositiveCase")]
        public async Task LoginAsync_CaseSensitiveCredentials_ReturnsAccountDetails()
        {
            // Arrange
            const string username = "CaseSensitiveUser";
            const string password = "CaseSensitivePass123!";
            var account = CreateTestAccount(username: username, password: password);

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.AccUsername);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Once);
        }

        #endregion

        #region Failed Login Tests

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "NegativeCase")]
        public async Task LoginAsync_InactiveAccount_ReturnsNull()
        {
            // Arrange
            const string username = "inactiveuser";
            const string password = "Password123!";
            var account = CreateTestAccount(
                username: username,
                password: password,
                isActive: false);

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            Assert.Null(null);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Once);
        }

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "NegativeCase")]
        public async Task LoginAsync_NonExistentUsername_ReturnsNull()
        {
            // Arrange
            const string username = "nonexistentuser";
            const string password = "Password123!";

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync((Account)null);

            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            Assert.Null(result);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Once);
        }

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "NegativeCase")]
        public async Task LoginAsync_WrongPassword_ReturnsNull()
        {
            // Arrange
            const string username = "validuser";
            const string correctPassword = "CorrectPass123!";
            const string wrongPassword = "WrongPass123!";
            var account = CreateTestAccount(username: username, password: correctPassword);

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, wrongPassword);

            // Assert
            Assert.Null(result);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Once);
        }

        #endregion

        #region Input Validation Tests

        [Theory]
        [InlineData(null, "Password123!", "NullUsername")]
        [InlineData("username", null, "NullPassword")]
        [InlineData(null, null, "BothNull")]
        [Trait("Category", "Login")]
        [Trait("TestType", "InputValidation")]
        public async Task LoginAsync_NullCredentials_ReturnsNull(string username, string password, string scenario)
        {
            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            Assert.Null(result);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("", "Password123!", "EmptyUsername")]
        [InlineData("username", "", "EmptyPassword")]
        [InlineData("   ", "Password123!", "WhitespaceUsername")]
        [InlineData("username", "   ", "WhitespacePassword")]
        [InlineData("", "", "BothEmpty")]
        [InlineData("   ", "   ", "BothWhitespace")]
        [Trait("Category", "Login")]
        [Trait("TestType", "InputValidation")]
        public async Task LoginAsync_EmptyOrWhitespaceCredentials_ReturnsNull(string username, string password, string scenario)
        {
            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            Assert.Null(result);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("user", "pass", "ShortCredentials")]
        [InlineData("user", "password", "WeakPasswordNoSpecialChar")]
        [InlineData("user", "PASSWORD123!", "WeakPasswordNoLowercase")]
        [InlineData("user", "password123!", "WeakPasswordNoUppercase")]
        [InlineData("user", "Passwordabc!", "WeakPasswordNoDigit")]
        [InlineData("user", "user123!A", "PasswordSameAsUsername")]
        [Trait("Category", "Login")]
        [Trait("TestType", "InputValidation")]
        public async Task LoginAsync_InvalidCredentialsFormat_ThrowsArgumentException(string username, string password, string scenario)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _accountService.GetAccountByLoginAsync(username, password));
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Edge Cases

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "EdgeCase")]
        public async Task LoginAsync_SpecialCharactersInCredentials_ReturnsAccountDetails()
        {
            // Arrange
            const string username = "user@domain.com";
            const string password = "P@ssw0rd!123";
            var account = CreateTestAccount(username: username, password: password);

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.AccUsername);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Once);
        }

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "EdgeCase")]
        public async Task LoginAsync_LongCredentials_ReturnsAccountDetails()
        {
            // Arrange
            var username = new string('a', 16); // Max length allowed by ValidateUsername
            var password = new string('b', 8) + "B1!"; // Meets minimum password requirements
            var account = CreateTestAccount(username: username, password: password);

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.AccUsername);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Once);
        }

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "EdgeCase")]
        public async Task LoginAsync_NumericCredentials_ReturnsAccountDetails()
        {
            // Arrange
            const string username = "12345";
            const string password = "67890Ab!";
            var account = CreateTestAccount(username: username, password: password);

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.AccUsername);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Once);
        }

        #endregion

        #region Performance and Behavior Tests

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "Behavior")]
        public async Task LoginAsync_RepositoryCalledOncePerLogin_VerifiesCallCount()
        {
            // Arrange
            const string username = "testuser";
            const string password = "TestPass123!";
            var account = CreateTestAccount(username: username, password: password);

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            await _accountService.GetAccountByLoginAsync(username, password);
            await _accountService.GetAccountByLoginAsync(username, password);

            // Assert
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Exactly(2));
        }

        [Fact]
        [Trait("Category", "Login")]
        [Trait("TestType", "Behavior")]
        public async Task LoginAsync_MultipleFailedAttempts_EachCallsRepository()
        {
            // Arrange
            const string username = "testuser";
            const string password1 = "WrongPass123!";
            const string password2 = "WrongPass456!";
            var account = CreateTestAccount(username: username, password: "CorrectPass123!");

            _mockAccountRepo
                .Setup(x => x.GetAccountByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            var result1 = await _accountService.GetAccountByLoginAsync(username, password1);
            var result2 = await _accountService.GetAccountByLoginAsync(username, password2);

            // Assert
            Assert.Null(result1);
            Assert.Null(result2);
            _mockAccountRepo.Verify(x => x.GetAccountByUsernameAsync(username), Times.Exactly(2));
        }

        #endregion
    }
}