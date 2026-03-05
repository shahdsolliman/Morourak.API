using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.DTOs.VehicleLicenses;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Application.Interfaces.Services;
using AppEx = Morourak.Application.Exceptions;

[ApiController]
[Route("api/[controller]")]
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

    [Authorize(Roles = "CITIZEN")]
    [HttpPost("upload-documents")]
    public async Task<IActionResult> UploadDocuments([FromForm] UploadVehicleLicenseDocumentsApiDto apiDto)
    {
        var nationalId = GetNationalId();

        // Prepare DTO from uploaded files
        var dto = new UploadVehicleDocsDto
        {
            VehicleType = apiDto.VehicleType,
            Brand = apiDto.Brand,
            Model = apiDto.Model,
            ManufactureYear = apiDto.ManufactureYear,
            Governorate = apiDto.Governorate,
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

    [Authorize(Roles = "CITIZEN")]
    [HttpPost("finalize/{requestNumber}")]
    public async Task<IActionResult> FinalizeLicense(string requestNumber, [FromBody] DeliveryInfoDto delivery)
    {
        var nationalId = GetNationalId();
        var result = await _service.FinalizeLicenseAsync(requestNumber, nationalId, delivery);
        return Ok(result);
    }

    [Authorize(Roles = "CITIZEN")]
    [HttpPost("finalize-renewal/{requestNumber}")]
    public async Task<IActionResult> FinalizeRenewal(string requestNumber, [FromBody] DeliveryInfoDto delivery)
    {
        var nationalId = GetNationalId();
        var result = await _service.FinalizeRenewalAsync(requestNumber, nationalId, delivery);
        return Ok(result);
    }

    #endregion

    #region Replacement & Renewal

    [Authorize(Roles = "CITIZEN")]
    [HttpPost("replacement/{licenseNumber}")]
    public async Task<IActionResult> IssueReplacement(string licenseNumber, [FromQuery] string type, [FromBody] DeliveryInfoDto delivery)
    {
        var nationalId = GetNationalId();
        var result = await _service.IssueReplacementAsync(nationalId, licenseNumber, type, delivery);
        return Ok(result);
    }

    [Authorize(Roles = "CITIZEN")]
    [HttpPost("renew")]
    public async Task<IActionResult> Renew([FromForm] RenewVehicleLicenseApiDto apiDto)
    {
        var nationalId = GetNationalId();

        // Prepare DTO for renewal
        var dto = new UploadVehicleDocsDto
        {
            VehicleLicenseNumber = apiDto.VehicleLicenseNumber,
            Governorate = apiDto.Governorate
        };

        var result = await _service.RenewLicenseAsync(nationalId, dto);
        return Ok(result);
    }

    #endregion

    #region Get My Licenses

    [Authorize(Roles = "CITIZEN")]
    [HttpGet("my-licenses")]
    public async Task<IActionResult> GetMyLicenses()
    {
        var nationalId = GetNationalId();
        var licenses = await _service.GetAllLicensesByCitizenAsync(nationalId);
        return Ok(licenses);
    }

    #endregion
}