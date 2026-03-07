using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.DTOs.VehicleLicenses;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Application.Interfaces;
using AppEx = Morourak.Application.Exceptions;

/// <summary>
/// Controller for managing vehicle license operations including applications, renewals, and replacements.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Vehicle Licenses")]
public class VehicleLicenseController : ControllerBase
{
    private readonly IVehicleLicenseService _service;

    public VehicleLicenseController(IVehicleLicenseService service)
    {
        _service = service;
    }

    #region Helpers

    private async Task<byte[]> ToByteArrayAsync(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }

    private string GetNationalId()
    {
        var nationalId = User.FindFirst("NationalId")?.Value;
        if (string.IsNullOrEmpty(nationalId))
            throw new AppEx.ValidationException("رقم الهوية غير موجود في التوكن.", "AUTH_MISSING_NATIONAL_ID");
        return nationalId;
    }

    #endregion

    #region Upload Documents

    /// <summary>
    /// Upload initial documents for a first-time vehicle license application.
    /// </summary>
    /// <param name="apiDto">The document data and vehicle details info.</param>
    /// <returns>The created vehicle license application details.</returns>
    [Authorize(Roles = "CITIZEN")]
    [HttpPost("upload-documents")]
    [ProducesResponseType(typeof(VehicleLicenseApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocuments([FromForm] UploadVehicleLicenseDocumentsApiDto apiDto)
    {
        var nationalId = GetNationalId();

        var dto = new UploadVehicleDocsDto
        {
            VehicleType = apiDto.VehicleType,
            Brand = apiDto.Brand,
            Model = apiDto.Model,
            OwnershipProof = await ToByteArrayAsync(apiDto.OwnershipProof),
            VehicleDataCertificate = await ToByteArrayAsync(apiDto.VehicleDataCertificate),
            IdCard = await ToByteArrayAsync(apiDto.IdCard),
            InsuranceCertificate = await ToByteArrayAsync(apiDto.InsuranceCertificate),
            CustomClearance = apiDto.CustomClearance != null ? await ToByteArrayAsync(apiDto.CustomClearance) : null
        };

        var result = await _service.UploadInitialDocumentsAsync(nationalId, dto);
        return Ok(result);
    }

    #endregion

    #region Finalize License

    /// <summary>
    /// Finalize the vehicle license process by providing delivery information.
    /// </summary>
    /// <param name="requestNumber">The service request number.</param>
    /// <param name="delivery">Delivery method and address details.</param>
    /// <returns>The issued vehicle license details.</returns>
    [Authorize(Roles = "CITIZEN")]
    [HttpPost("finalize/{requestNumber}")]
    [ProducesResponseType(typeof(VehicleLicenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizeLicense(string requestNumber, [FromBody] DeliveryInfoDto delivery)
    {
        var nationalId = GetNationalId();
        var result = await _service.FinalizeLicenseAsync(requestNumber, nationalId, delivery);
        return Ok(result);
    }

    /// <summary>
    /// Finalize the vehicle license renewal by providing delivery information.
    /// </summary>
    /// <param name="requestNumber">The service request number.</param>
    /// <param name="delivery">Delivery method and address details.</param>
    /// <returns>The renewed vehicle license details.</returns>
    [Authorize(Roles = "CITIZEN")]
    [HttpPost("finalize-renewal/{requestNumber}")]
    [ProducesResponseType(typeof(VehicleLicenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizeRenewal(string requestNumber, [FromBody] DeliveryInfoDto delivery)
    {
        var nationalId = GetNationalId();
        var result = await _service.FinalizeRenewalAsync(requestNumber, nationalId, delivery);
        return Ok(result);
    }

    #endregion

    #region Replacement & Renewal

    /// <summary>
    /// Issue a replacement for a lost or damaged vehicle license.
    /// </summary>
    /// <param name="licenseNumber">The license number to replace.</param>
    /// <param name="type">The type of replacement (Lost or Damaged).</param>
    /// <param name="delivery">Delivery details.</param>
    /// <returns>The new vehicle license details.</returns>
    [Authorize(Roles = "CITIZEN")]
    [HttpPost("replacement/{licenseNumber}")]
    [ProducesResponseType(typeof(VehicleLicenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IssueReplacement(string licenseNumber, [FromQuery] string type, [FromBody] DeliveryInfoDto delivery)
    {
        var nationalId = GetNationalId();
        var result = await _service.IssueReplacementAsync(nationalId, licenseNumber, type, delivery);
        return Ok(result);
    }

    /// <summary>
    /// Submit a request for vehicle license renewal.
    /// </summary>
    /// <param name="apiDto">Renewal request data.</param>
    /// <returns>The renewal application details.</returns>
    [Authorize(Roles = "CITIZEN")]
    [HttpPost("renew")]
    [ProducesResponseType(typeof(VehicleLicenseApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Renew([FromForm] RenewVehicleLicenseApiDto apiDto)
    {
        var nationalId = GetNationalId();

        var dto = new UploadVehicleDocsDto
        {
            VehicleLicenseNumber = apiDto.VehicleLicenseNumber,
        };

        var result = await _service.SubmitRenewalRequestAsync(nationalId, dto);
        return Ok(result);
    }

    #endregion

    #region Get My Licenses

    /// <summary>
    /// Get all vehicle licenses belonging to the authenticated citizen.
    /// </summary>
    /// <returns>List of vehicle licenses.</returns>
    [Authorize(Roles = "CITIZEN")]
    [HttpGet("my-licenses")]
    [ProducesResponseType(typeof(IEnumerable<VehicleLicenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyLicenses()
    {
        var nationalId = GetNationalId();
        var licenses = await _service.GetAllLicensesByCitizenAsync(nationalId);
        return Ok(licenses);
    }

    #endregion
}
