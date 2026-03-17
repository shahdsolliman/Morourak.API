using Morourak.Application.DTOs.Auth;
using Morourak.Application.Exceptions;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;

namespace Morourak.Application.Services
{
    public class CitizenRegistryService : ICitizenRegistryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CitizenRegistryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CitizenMatchResult> ValidateFullMatchAsync(
            string nationalId,
            string firstName,
            string lastName,
            string mobileNumber)
        {
            var result = new CitizenMatchResult();

            var citizen = (await _unitOfWork
                .Repository<CitizenRegistry>()
                .FindAsync(c => c.NationalId == nationalId))
                .FirstOrDefault();

            if (citizen == null)
            {
                result.Errors.Add(new ErrorDetail
                {
                    Field = "nationalId",
                    Error = "لا يوجد سجل لهذا المواطن."
                });

                return result;
            }

            result.Citizen = citizen;

            if (!string.Equals(firstName?.Trim(), citizen.FirstName?.Trim(), StringComparison.OrdinalIgnoreCase))
                result.Errors.Add(new ErrorDetail { Field = "firstName", Error = "الاسم الأول غير مطابق للسجلات الرسمية." });

            if (!string.Equals(lastName?.Trim(), citizen.LastName?.Trim(), StringComparison.OrdinalIgnoreCase))
                result.Errors.Add(new ErrorDetail { Field = "lastName", Error = "اسم العائلة غير مطابق للسجلات الرسمية." });

            if (NormalizePhoneNumber(mobileNumber) != NormalizePhoneNumber(citizen.MobileNumber))
                result.Errors.Add(new ErrorDetail { Field = "mobileNumber", Error = "رقم الهاتف المحمول غير مطابق للسجلات الرسمية." });

            result.IsMatch = !result.Errors.Any();

            return result;
        }

        public async Task<int?> GetCitizenIdByNationalIdAsync(string nationalId)
        {
            var citizen = (await _unitOfWork
                .Repository<CitizenRegistry>()
                .FindAsync(c => c.NationalId == nationalId))
                .FirstOrDefault();

            return citizen?.Id;
        }

        private static string NormalizePhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            phone = phone.Replace(" ", "").Trim();

            if (phone.StartsWith("+20"))
                phone = "0" + phone.Substring(3);

            return phone;
        }
    }
}