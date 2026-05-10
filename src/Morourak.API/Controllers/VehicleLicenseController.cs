using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Morourak.Application.Interfaces;
using Morourak.Application.Exceptions;
using Morourak.Domain.Entities;
using Morourak.Application.DTOs.Delivery;
using Morourak.API.DTOs.VehicleLicenses;
using Morourak.Domain.Enums.Common;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for managing vehicle license operations including applications, renewals, and replacements.
    /// </summary>
    [Route("api/v1/[controller]")]
    [Tags("Vehicle Licenses")]
    public class VehicleLicenseController : BaseApiController
    {
        private readonly IVehicleLicenseService _service;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleLicenseController(IVehicleLicenseService service, IUnitOfWork unitOfWork)
        {
            _service = service;
            _unitOfWork = unitOfWork;
        }


        // ================= UPLOAD DOCUMENTS =================

        /// <summary>
        /// Uploads required documents for a new vehicle license application.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("upload-documents")]
        public async Task<IActionResult> UploadDocuments([FromForm] UploadVehicleLicenseDocumentsApiDto apiDto)
        {
            var nationalId = NationalId;

            var dto = new Morourak.Application.DTOs.Vehicles.UploadVehicleDocsDto
            {
                VehicleType = apiDto.VehicleType,
                Brand = apiDto.Brand,
                Model = apiDto.Model,
                OwnershipProof = await ToByteArrayAsync(apiDto.OwnershipProof),
                VehicleDataCertificate = await ToByteArrayAsync(apiDto.VehicleDataCertificate),
                IdCard = await ToByteArrayAsync(apiDto.IdCard),
                InsuranceCertificate = apiDto.InsuranceCertificate != null ? await ToByteArrayAsync(apiDto.InsuranceCertificate) : null,
                InsuranceCompanyId = await ResolveInsuranceCompanyId(apiDto.InsuranceCompanyId),
                CustomClearance = apiDto.CustomClearance != null ? await ToByteArrayAsync(apiDto.CustomClearance) : null
            };

            var result = await _service.UploadInitialDocumentsAsync(nationalId, dto);
            return Success(result, "تم رفع المستندات بنجاح");
        }

        private async Task<int?> ResolveInsuranceCompanyId(object? idOrName)
        {
            if (idOrName == null) return null;

            string input = idOrName.ToString() ?? "";
            if (int.TryParse(input, out int id))
                return id;

            var company = await _unitOfWork.Repository<InsuranceCompany>()
                .GetAsync(c => c.NameAr == input || c.Name == input);

            return company?.Id;
        }

        // ================= FINALIZE LICENSE =================

        /// <summary>
        /// Finalizes a vehicle license application by providing delivery information.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("finalize/{requestNumber}")]
        public async Task<IActionResult> FinalizeLicense(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            var nationalId = NationalId;
            var result = await _service.FinalizeLicenseAsync(requestNumber, nationalId, delivery);
            return Success(result, "تم إصدار الرخصة بنجاح");
        }

        /// <summary>
        /// Finalizes a vehicle license renewal request by providing delivery info.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("finalize-renewal/{requestNumber}")]
        public async Task<IActionResult> FinalizeRenewal(string requestNumber, [FromBody] DeliveryInfoDto delivery)
        {
            var nationalId = NationalId;
            var result = await _service.FinalizeRenewalAsync(requestNumber, nationalId, delivery);
            return Success(result, "تم تجديد الرخصة بنجاح");
        }

        // ================= REPLACEMENT & RENEWAL =================

        /// <summary>
        /// Requests a replacement for a lost or damaged vehicle license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("issue-replacement/{licenseNumber}")]
        public async Task<IActionResult> IssueReplacement(
            string licenseNumber,
            [FromBody] IssueReplacementVehicleLicenseApiDto apiDto)
        {
            var nationalId = NationalId;
            var result = await _service.IssueReplacementAsync(
                nationalId, 
                licenseNumber, 
                apiDto.ReplacementType, 
                apiDto.Delivery);
            return Success(result, "تم استخراج بدل الرخصة بنجاح");
        }

        /// <summary>
        /// Submits a request to renew an existing vehicle license.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpPost("renew")]
        public async Task<IActionResult> Renew([FromForm] RenewVehicleLicenseApiDto apiDto)
        {
            var nationalId = NationalId;

            var dto = new Morourak.Application.DTOs.Vehicles.UploadVehicleDocsDto
            {
                VehicleLicenseNumber = apiDto.VehicleLicenseNumber,
            };

            var result = await _service.SubmitRenewalRequestAsync(nationalId, dto);
            return Success(result, "تم تقديم طلب التجديد بنجاح");
        }

        // ================= GET MY LICENSES =================

        /// <summary>
        /// Retrieves all vehicle licenses owned by the currently authenticated citizen.
        /// </summary>
        [Authorize(Roles = "CITIZEN")]
        [HttpGet("my-licenses")]
        public async Task<IActionResult> GetMyLicenses()
        {
            var nationalId = NationalId;
            var licenses = await _service.GetAllLicensesByCitizenAsync(nationalId);
            return Success(licenses);
        }

        /// <summary>
        /// Retrieves the list of predefined insurance companies and their fees.
        /// </summary>
        [HttpGet("insurance-companies")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInsuranceCompanies()
        {
            var companies = await _unitOfWork.Repository<InsuranceCompany>().GetAllAsync();
            var result = companies.Select(c => new
            {
                c.Id,
                c.Name,
                c.NameAr,
                c.Fee,
                c.Description,
                c.DescriptionAr,
                c.LogoPath
            });
            return Success(result);
        }
    }
}
