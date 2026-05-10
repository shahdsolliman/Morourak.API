using Morourak.Application.Services.Licenses;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.Exceptions;
using AutoMapper;
using Moq;
using Xunit;
using System.Linq.Expressions;

namespace Morourak.Tests.Services
{
    public class DrivingLicenseRenewalServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILicenseValidationService> _validationServiceMock;
        private readonly Mock<IRequestNumberGenerator> _generatorMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IServiceRequestService> _serviceRequestServiceMock;
        private readonly DrivingLicenseRenewalService _service;

        public DrivingLicenseRenewalServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _validationServiceMock = new Mock<ILicenseValidationService>();
            _generatorMock = new Mock<IRequestNumberGenerator>();
            _mapperMock = new Mock<IMapper>();
            _serviceRequestServiceMock = new Mock<IServiceRequestService>();

            _service = new DrivingLicenseRenewalService(
                _unitOfWorkMock.Object,
                _validationServiceMock.Object,
                _generatorMock.Object,
                _mapperMock.Object,
                _serviceRequestServiceMock.Object
            );
        }

        [Fact]
        public async Task SubmitRenewalRequestAsync_ShouldSucceed_WhenValidLicenseNumberProvided()
        {
            // Arrange
            var nationalId = "29001011234567";
            var licenseNumber = "DL-12345";
            var citizen = new CitizenRegistry { Id = 1, NationalId = nationalId };
            var license = new DrivingLicense 
            { 
                Id = 101,
                CitizenRegistryId = 1, 
                LicenseNumber = licenseNumber,
                Category = DrivingLicenseCategory.Private
            };
            
            var dto = new SubmitRenewalRequestDto { LicenseNumber = licenseNumber };

            SetupCitizen(citizen);
            SetupLicense(license);
            
            _validationServiceMock.Setup(v => v.ValidateRenewalEligibilityAsync(license, It.IsAny<DrivingLicenseCategory>()))
                .Returns(Task.CompletedTask);
            
            _generatorMock.Setup(g => g.GenerateAsync(It.IsAny<Morourak.Domain.Enums.Request.ServiceType>()))
                .ReturnsAsync("REQ-REN-123");

            // Act
            var result = await _service.SubmitRenewalRequestAsync(nationalId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(licenseNumber, result.DrivingLicenseNumber);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.AtLeastOnce());
        }

        #region Helpers

        private void SetupCitizen(CitizenRegistry citizen)
        {
            var repo = new Mock<Morourak.Application.Interfaces.Repositories.IGenericRepository<CitizenRegistry>>();
            repo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<CitizenRegistry, bool>>>(), It.IsAny<Expression<Func<CitizenRegistry, object>>[]>()))
                .ReturnsAsync(citizen);
            _unitOfWorkMock.Setup(u => u.Repository<CitizenRegistry>()).Returns(repo.Object);
        }

        private void SetupLicense(DrivingLicense? license)
        {
            var repo = new Mock<Morourak.Application.Interfaces.Repositories.IGenericRepository<DrivingLicense>>();
            repo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<DrivingLicense, bool>>>(), It.IsAny<Expression<Func<DrivingLicense, object>>[]>()))
                .ReturnsAsync(license);
            _unitOfWorkMock.Setup(u => u.Repository<DrivingLicense>()).Returns(repo.Object);
            
            var renewalRepo = new Mock<Morourak.Application.Interfaces.Repositories.IGenericRepository<RenewalApplication>>();
            _unitOfWorkMock.Setup(u => u.Repository<RenewalApplication>()).Returns(renewalRepo.Object);

            var reqRepo = new Mock<Morourak.Application.Interfaces.Repositories.IGenericRepository<ServiceRequest>>();
            _unitOfWorkMock.Setup(u => u.Repository<ServiceRequest>()).Returns(reqRepo.Object);
        }

        #endregion
    }
}
