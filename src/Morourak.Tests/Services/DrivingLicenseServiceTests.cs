using Morourak.Application.Services.Licenses;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.Exceptions;
using Moq;
using Xunit;

namespace Morourak.Tests.Services
{
    public class DrivingLicenseServiceFacadeTests
    {
        private readonly Mock<IDrivingLicenseIssuanceService> _issuanceServiceMock;
        private readonly Mock<IDrivingLicenseRenewalService> _renewalServiceMock;
        private readonly Mock<IDrivingLicenseReplacementService> _replacementServiceMock;
        private readonly Mock<IDrivingLicenseQueryService> _queryServiceMock;
        private readonly Mock<IDrivingLicenseResultService> _resultServiceMock;
        private readonly DrivingLicenseService _facade;

        public DrivingLicenseServiceFacadeTests()
        {
            _issuanceServiceMock = new Mock<IDrivingLicenseIssuanceService>();
            _renewalServiceMock = new Mock<IDrivingLicenseRenewalService>();
            _replacementServiceMock = new Mock<IDrivingLicenseReplacementService>();
            _queryServiceMock = new Mock<IDrivingLicenseQueryService>();
            _resultServiceMock = new Mock<IDrivingLicenseResultService>();

            _facade = new DrivingLicenseService(
                _issuanceServiceMock.Object,
                _renewalServiceMock.Object,
                _replacementServiceMock.Object,
                _queryServiceMock.Object,
                _resultServiceMock.Object
            );
        }

        [Fact]
        public async Task UploadInitialDocumentsAsync_ShouldDelegateToIssuanceService()
        {
            // Arrange
            var nationalId = "29001011234567";
            var dto = new UploadDrivingLicenseDocumentsDto { Category = DrivingLicenseCategory.Private };
            var expectedResponse = new DrivingLicenseApplicationDto();
            
            _issuanceServiceMock.Setup(s => s.UploadInitialDocumentsAsync(nationalId, dto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _facade.UploadInitialDocumentsAsync(nationalId, dto);

            // Assert
            Assert.Equal(expectedResponse, result);
            _issuanceServiceMock.Verify(s => s.UploadInitialDocumentsAsync(nationalId, dto), Times.Once);
        }

        [Fact]
        public async Task SubmitRenewalRequestAsync_ShouldDelegateToRenewalService()
        {
            // Arrange
            var nationalId = "29001011234567";
            var dto = new SubmitRenewalRequestDto { LicenseNumber = "DL-123" };
            var expectedResponse = new RenewalApplicationDto();

            _renewalServiceMock.Setup(s => s.SubmitRenewalRequestAsync(nationalId, dto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _facade.SubmitRenewalRequestAsync(nationalId, dto);

            // Assert
            Assert.Equal(expectedResponse, result);
            _renewalServiceMock.Verify(s => s.SubmitRenewalRequestAsync(nationalId, dto), Times.Once);
        }
    }
}
