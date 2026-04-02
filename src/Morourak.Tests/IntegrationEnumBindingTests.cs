using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Morourak.API.Formatting;
using Morourak.Application.DTOs.Appointments;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs;
using Morourak.Application.DTOs.Licenses;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Application.Interfaces;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Common;
using Xunit;

namespace Morourak.Tests;

public sealed class IntegrationEnumBindingTests : IClassFixture<IntegrationEnumBindingTests.TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public IntegrationEnumBindingTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("بدل فاقد", ReplacementType.Lost)]
    [InlineData("Lost", ReplacementType.Lost)]
    [InlineData("lost", ReplacementType.Lost)]
    [InlineData(0, ReplacementType.Lost)]
    public async Task DrivingReplacement_Binds_ReplacementType(object replacementTypeValue, ReplacementType expected)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var body = new
        {
            replacementType = replacementTypeValue,
            delivery = new
            {
                method = "وحدة المرور"
            }
        };

        var response = await client.PostAsJsonAsync("/api/v1/DrivingLicense/issue-replacement/DL-0001", body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, _factory.DrivingLicenseService.LastReplacementType);
    }

    [Theory]
    [InlineData("بدل تالف", ReplacementType.Damaged)]
    [InlineData("Damaged", ReplacementType.Damaged)]
    [InlineData("damaged", ReplacementType.Damaged)]
    [InlineData(1, ReplacementType.Damaged)]
    public async Task VehicleReplacement_Binds_ReplacementType_FromQuery(object replacementTypeValue, ReplacementType expected)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var queryValue = Uri.EscapeDataString(replacementTypeValue.ToString()!);
        var response = await client.PostAsJsonAsync($"/api/v1/VehicleLicense/replacement/VL-0001?type={queryValue}", new
        {
            method = "وحدة المرور"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, _factory.VehicleLicenseService.LastReplacementType);
    }

    [Theory]
    [InlineData("كشف طبي", AppointmentType.Medical)]
    [InlineData("Medical", AppointmentType.Medical)]
    [InlineData("medical", AppointmentType.Medical)]
    [InlineData(1, AppointmentType.Medical)]
    public async Task AppointmentBook_Binds_NewAppointmentType(object appointmentTypeValue, AppointmentType expected)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var response = await client.PostAsJsonAsync("/api/v1/Appointments/book", new
        {
            appointmentType = appointmentTypeValue,
            date = "2026-04-03",
            time = "09:00",
            governorateId = 1,
            trafficUnitId = 1
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, _factory.Mediator.LastAppointmentType);
    }

    [Fact]
    public async Task AppointmentBook_Binds_LegacyServiceType()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var response = await client.PostAsJsonAsync("/api/v1/Appointments/book", new
        {
            serviceType = "كشف طبي",
            date = "2026-04-03",
            time = "09:00",
            governorateId = 1,
            trafficUnitId = 1
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(AppointmentType.Medical, _factory.Mediator.LastAppointmentType);
    }

    [Fact]
    public async Task AppointmentBook_ConflictingInputs_Returns400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var response = await client.PostAsJsonAsync("/api/v1/Appointments/book", new
        {
            appointmentType = "كشف طبي",
            serviceType = "فحص فني",
            date = "2026-04-03",
            time = "09:00",
            governorateId = 1,
            trafficUnitId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AppointmentBook_InvalidNumericEnum_Returns400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var response = await client.PostAsJsonAsync("/api/v1/Appointments/book", new
        {
            appointmentType = 0, // not defined (AppointmentType starts at 1)
            date = "2026-04-03",
            time = "09:00",
            governorateId = 1,
            trafficUnitId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvalidEnumValue_InBody_Returns400_Not500()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var response = await client.PostAsJsonAsync("/api/v1/DrivingLicense/issue-replacement/DL-0001", new
        {
            replacementType = "INVALID_VALUE",
            delivery = new { method = "وحدة المرور" }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvalidEnumValue_InQuery_Returns400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var response = await client.PostAsJsonAsync("/api/v1/VehicleLicense/replacement/VL-0001?type=INVALID", new
        {
            method = "وحدة المرور"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public sealed class TestApiFactory : WebApplicationFactory<Morourak.API.Program>
    {
        public FakeDrivingLicenseService DrivingLicenseService { get; } = new();
        public FakeVehicleLicenseService VehicleLicenseService { get; } = new();
        public FakeMediator Mediator { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var values = new Dictionary<string, string?>
                {
                    ["SkipDatabaseInit"] = "true",
                    ["ConnectionStrings:IdentityConnection"] = "Server=(localdb)\\\\MSSQLLocalDB;Database=Morourak_Test_Identity;Trusted_Connection=True;TrustServerCertificate=True",
                    ["ConnectionStrings:PersistenceConnection"] = "Server=(localdb)\\\\MSSQLLocalDB;Database=Morourak_Test_Persistence;Trusted_Connection=True;TrustServerCertificate=True"
                };

                config.AddInMemoryCollection(values);
            });

            builder.ConfigureServices(services =>
            {
                // Disable background services in test host.
                services.RemoveAll<IHostedService>();

                // Force test auth.
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                services.RemoveAll<IDrivingLicenseService>();
                services.AddSingleton<IDrivingLicenseService>(DrivingLicenseService);

                services.RemoveAll<IVehicleLicenseService>();
                services.AddSingleton<IVehicleLicenseService>(VehicleLicenseService);

                services.RemoveAll<IMediator>();
                services.AddSingleton<IMediator>(Mediator);

                services.RemoveAll<IAppointmentArabicFormatter>();
                services.AddSingleton<IAppointmentArabicFormatter>(new NoOpArabicFormatter());
            });
        }
    }

    private sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim("NationalId", "29001011234567"),
                new Claim(ClaimTypes.NameIdentifier, "test-user"),
                new Claim(ClaimTypes.Role, "CITIZEN")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    private sealed class NoOpArabicFormatter : IAppointmentArabicFormatter
    {
        public BookingConfirmationDto FormatBookingConfirmation(BookingConfirmationDto dto) => dto;
        public AppointmentSummaryDto FormatAppointmentSummary(AppointmentSummaryDto dto) => dto;
        public AppointmentDetailsDto FormatAppointmentDetails(AppointmentDetailsDto dto) => dto;
    }

    public sealed class FakeMediator : IMediator
    {
        public AppointmentType? LastAppointmentType { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is Morourak.Application.CQRS.Appointment.Commands.CreateAppointment.CreateAppointmentCommand cmd)
            {
                LastAppointmentType = cmd.AppointmentType;
                object result = new BookingConfirmationDto();
                return Task.FromResult((TResponse)result);
            }

            throw new NotSupportedException("Only CreateAppointmentCommand is supported in this test mediator.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
            => throw new NotImplementedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    public sealed class FakeDrivingLicenseService : IDrivingLicenseService
    {
        public ReplacementType? LastReplacementType { get; private set; }

        public Task<ServiceRequestDto> IssueReplacementAsync(string nationalId, string drivingLicenseNumber, ReplacementType replacementType, DeliveryInfoDto delivery)
        {
            LastReplacementType = replacementType;
            return Task.FromResult(new ServiceRequestDto { RequestNumber = "REQ-1", CitizenNationalId = nationalId });
        }

        public Task<DrivingLicenseApplicationDto> UploadInitialDocumentsAsync(string nationalId, UploadDrivingLicenseDocumentsDto dto) => throw new NotImplementedException();
        public Task<ServiceRequestDto> FinalizeLicenseAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery) => throw new NotImplementedException();
        public Task<RenewalApplicationDto> SubmitRenewalRequestAsync(string nationalId, SubmitRenewalRequestDto dto) => throw new NotImplementedException();
        public Task<ServiceRequestDto> FinalizeRenewalAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery) => throw new NotImplementedException();
        public Task<DrivingLicenseResponseDto> CompleteIssuanceAsync(string requestNumber) => throw new NotImplementedException();
        public Task<IEnumerable<DrivingLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId) => throw new NotImplementedException();
        public Task<DrivingLicenseApplication> GetApplicationByIdAsync(int applicationId, string nationalId) => throw new NotImplementedException();
        public Task SubmitAppointmentResultAsync(int applicationId, AppointmentType type, bool passed, string? notes) => throw new NotImplementedException();
    }

    public sealed class FakeVehicleLicenseService : IVehicleLicenseService
    {
        public ReplacementType? LastReplacementType { get; private set; }

        public Task<ServiceRequestDto> IssueReplacementAsync(string nationalId, string vehicleLicenseNumber, ReplacementType replacementType, DeliveryInfoDto delivery)
        {
            LastReplacementType = replacementType;
            return Task.FromResult(new ServiceRequestDto { RequestNumber = "REQ-1", CitizenNationalId = nationalId });
        }

        public Task<VehicleLicenseApplicationDto> UploadInitialDocumentsAsync(string nationalId, UploadVehicleDocsDto dto) => throw new NotImplementedException();
        public Task<ServiceRequestDto> FinalizeLicenseAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery) => throw new NotImplementedException();
        public Task<VehicleLicenseApplicationDto> SubmitRenewalRequestAsync(string nationalId, UploadVehicleDocsDto dto) => throw new NotImplementedException();
        public Task<ServiceRequestDto> FinalizeRenewalAsync(string requestNumber, string nationalId, DeliveryInfoDto delivery) => throw new NotImplementedException();
        public Task SubmitAppointmentResultAsync(int applicationId, AppointmentType type, bool passed, string? notes) => throw new NotImplementedException();
        public Task<VehicleLicenseResponseDto> CompleteIssuanceAsync(string requestNumber) => throw new NotImplementedException();
        public Task<VehicleLicenseApplication?> GetApplicationByIdAsync(int id, string nationalId) => throw new NotImplementedException();
        public Task<IEnumerable<VehicleLicenseDto>> GetAllLicensesByCitizenAsync(string nationalId) => throw new NotImplementedException();
        public Task<IEnumerable<VehicleTypeDetailDto>> GetVehicleTypesAsync() => throw new NotImplementedException();
    }
}
