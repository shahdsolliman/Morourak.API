using AutoMapper;
using Morourak.Application.DTOs.DrivingLicenses;
using Morourak.Application.DTOs.Licenses;
using Morourak.Application.DTOs.Delivery;
using Morourak.Application.DTOs.Violations;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Violations;
using Morourak.Domain.Extensions;
using Morourak.Application.DTOs;
using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Enums.Appointments;
using System.Globalization;
using Morourak.Application.DTOs.Vehicles;
using Morourak.Application.DTOs.Governorates;

namespace Morourak.Application.Mapping
{
    public class GeneralMappingProfile : Profile
    {
        public GeneralMappingProfile()
        {
            // Driving License
            CreateMap<DrivingLicense, DrivingLicenseDto>()
                .ForMember(d => d.Category, opt => opt.MapFrom(src => src.Category.GetDisplayName()))
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.CurrentStatus.GetDisplayName()))
                .ForMember(d => d.CitizenNationalId, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.NationalId : string.Empty))
                .ForMember(d => d.IssueDate, opt => opt.MapFrom(src => FormatSystemDate(src.IssueDate)))
                .ForMember(d => d.ExpiryDate, opt => opt.MapFrom(src => FormatSystemDate(src.ExpiryDate)))
                .ForMember(d => d.Governorate, opt => opt.Ignore()) // Populated dynamically in service
                .ForMember(d => d.LicensingUnit, opt => opt.Ignore()); // Populated dynamically in service

            CreateMap<DrivingLicense, DrivingLicenseResponseDto>()
                .ForMember(d => d.Category, opt => opt.MapFrom(src => src.Category.GetDisplayName()))
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.CurrentStatus.GetDisplayName()))
                .ForMember(d => d.CitizenName, opt => opt.MapFrom(src => src.Citizen != null ? $"{src.Citizen.FirstName} {src.Citizen.LastName}".Trim() : string.Empty))
                .ForMember(d => d.IssueDate, opt => opt.MapFrom(src => FormatSystemDate(src.IssueDate)))
                .ForMember(d => d.ExpiryDate, opt => opt.MapFrom(src => FormatSystemDate(src.ExpiryDate)))
                .ForMember(d => d.Governorate, opt => opt.Ignore()) // Populated dynamically in service
                .ForMember(d => d.LicensingUnit, opt => opt.Ignore()) // Populated dynamically in service
                .ForMember(d => d.Delivery, opt => opt.MapFrom(src => new Morourak.Application.DTOs.Delivery.DeliveryInfoDto
                {
                    Method = src.DeliveryMethod,
                    Address = src.DeliveryAddress == null ? null : new Morourak.Application.DTOs.Delivery.AddressDto
                    {
                        Governorate = src.DeliveryAddress.Governorate,
                        City = src.DeliveryAddress.City,
                        Details = src.DeliveryAddress.Details
                    }
                }));

            // Driving License Application
            CreateMap<DrivingLicenseApplication, DrivingLicenseApplicationDto>()
                .ForMember(d => d.Category, opt => opt.MapFrom(src => src.Category.GetDisplayName()))
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.GetDisplayName()));

            // Renewal Application
            CreateMap<RenewalApplication, RenewalApplicationDto>()
                .ForMember(d => d.CurrentCategory, opt => opt.MapFrom(src => src.CurrentCategory.GetDisplayName()))
                .ForMember(d => d.RequestedCategory, opt => opt.MapFrom(src => src.RequestedCategory.GetDisplayName()));

            // Payment Receipt
            CreateMap<Payment, Morourak.Application.DTOs.Paymob.PaymentReceiptDto>()
                .ForMember(d => d.TotalAmount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(d => d.PaidAt, opt => opt.MapFrom(src => FormatSystemDateTime(src.PaidAt ?? DateTime.UtcNow)))
                .ForMember(d => d.CitizenName, opt => opt.Ignore()) // Manual in service
                .ForMember(d => d.Items, opt => opt.MapFrom(src => src.PaymentItems));

            CreateMap<PaymentItem, Morourak.Application.DTOs.Paymob.ReceiptItemDto>();

            // Vehicle License
            CreateMap<VehicleLicense, VehicleLicenseDto>()
                .ForMember(d => d.VehicleType, opt => opt.MapFrom(src => src.VehicleType.GetDisplayName()))
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.CurrentStatus.GetDisplayName()))
                .ForMember(d => d.CitizenNationalId, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.NationalId : string.Empty))
                .ForMember(d => d.IssueDate, opt => opt.MapFrom(src => FormatSystemDate(src.IssueDate)))
                .ForMember(d => d.ExpiryDate, opt => opt.MapFrom(src => FormatSystemDate(src.ExpiryDate)));

            CreateMap<VehicleLicense, VehicleLicenseResponseDto>()
                .ForMember(d => d.VehicleType, opt => opt.MapFrom(src => src.VehicleType.GetDisplayName()))
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.CurrentStatus.GetDisplayName()))
                .ForMember(d => d.CitizenName, opt => opt.MapFrom(src => src.Citizen != null ? $"{src.Citizen.FirstName} {src.Citizen.LastName}".Trim() : string.Empty))
                .ForMember(d => d.IssueDate, opt => opt.MapFrom(src => FormatSystemDate(src.IssueDate)))
                .ForMember(d => d.ExpiryDate, opt => opt.MapFrom(src => FormatSystemDate(src.ExpiryDate)))
                .ForMember(d => d.Delivery, opt => opt.MapFrom(src => new DeliveryInfoDto
                {
                    Method = src.DeliveryMethod,
                    Address = src.DeliveryAddress == null ? null : new AddressDto
                    {
                        Governorate = src.DeliveryAddress.Governorate,
                        City = src.DeliveryAddress.City,
                        Details = src.DeliveryAddress.Details
                    }
                }));

            // Vehicle Application
            CreateMap<VehicleLicenseApplication, VehicleLicenseApplicationDto>()
                .ForMember(d => d.VehicleType, opt => opt.MapFrom(src => src.VehicleType.GetDisplayName()))
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.GetDisplayName()));

            // Traffic Violations
            CreateMap<TrafficViolation, ViolationDto>()
                .ForMember(d => d.ViolationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(d => d.ViolationType, opt => opt.MapFrom(src => src.ViolationType.ToString()))
                .ForMember(d => d.ViolationDateTime, opt => opt.MapFrom(src => src.ViolationDateTime.ToString("d MMMM yyyy - hh:mm tt", new CultureInfo("ar-EG"))))
                .ForMember(d => d.StatusAr, opt => opt.MapFrom(src => GetStatusArabic(src.Status)));

            CreateMap<TrafficViolation, ViolationDetailsDto>()
                .ForMember(d => d.ViolationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(d => d.CitizenName, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.FirstName : "غير معروف"))
                .ForMember(d => d.NationalId, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.NationalId : "غير معروف"))
                .ForMember(d => d.LicenseType, opt => opt.MapFrom(src => src.LicenseType.ToString()))
                .ForMember(d => d.LicenseTypeAr, opt => opt.MapFrom(src => src.LicenseType == LicenseType.Driving ? "رخصة قيادة" : "رخصة مركبة"))
                .ForMember(d => d.ViolationType, opt => opt.MapFrom(src => src.ViolationType.ToString()))
                .ForMember(d => d.ViolationTypeAr, opt => opt.MapFrom(src => GetViolationTypeArabic(src.ViolationType)))
                .ForMember(d => d.ViolationDateTime, opt => opt.MapFrom(src => src.ViolationDateTime.ToString("hh:mm tt - d/M/yyyy", new CultureInfo("ar-EG"))))
                .ForMember(d => d.StatusAr, opt => opt.MapFrom(src => GetStatusArabic(src.Status)));

            // Service Request
            CreateMap<ServiceRequest, ServiceRequestSummaryDto>()
                .ForMember(d => d.ServiceType, opt => opt.MapFrom(src => src.ServiceType.GetDisplayName()))
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.GetDisplayName()))
                .ForMember(d => d.SubmittedAt, opt => opt.MapFrom(src => FormatSystemDateTime(src.SubmittedAt)));

            CreateMap<ServiceRequest, ServiceRequestDto>()
                .ForMember(d => d.ServiceType, opt => opt.MapFrom(src => src.ServiceType.GetDisplayName()))
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.GetDisplayName()))
                .ForMember(d => d.SubmittedAt, opt => opt.MapFrom(src => FormatSystemDateTime(src.SubmittedAt)))
                .ForMember(d => d.LastUpdatedAt, opt => opt.MapFrom(src => src.LastUpdatedAt.HasValue ? FormatSystemDateTime(src.LastUpdatedAt.Value) : null))
                .ForMember(d => d.ReferenceId, opt => opt.MapFrom(src => src.ReferenceId))
                .ForMember(d => d.Fees, opt => opt.MapFrom(src => new ServiceRequestFeesDto
                {
                    BaseFee = src.BaseFee,
                    DeliveryFee = src.DeliveryFee,
                    TotalAmount = src.TotalAmount
                }))
                .ForMember(d => d.Delivery, opt => opt.MapFrom(src => new ServiceRequestDeliveryDto
                {
                    Method = src.DeliveryMethod.HasValue ? src.DeliveryMethod.Value.GetDisplayName() : null,
                    Address = src.DeliveryAddressDetail
                }))
                .ForMember(d => d.Payment, opt => opt.MapFrom(src => new ServiceRequestPaymentDto
                {
                    Status = src.PaymentStatus.GetDisplayName(),
                    TransactionId = src.PaymentTransactionId,
                    Amount = src.PaymentAmount,
                    Timestamp = src.PaymentTimestamp.HasValue ? FormatSystemDateTime(src.PaymentTimestamp.Value) : null
                }));

            // Appointment
            CreateMap<Morourak.Domain.Entities.Appointment, AppointmentDto>()
                .ForMember(d => d.TypeName, opt => opt.MapFrom(src => GetAppointmentTypeName(src.Type)))
                .ForMember(d => d.ServiceName, opt => opt.MapFrom(src => GetAppointmentTypeName(src.Type)))
                .ForMember(d => d.DateFormatted, opt => opt.MapFrom(src => src.Date.ToString("d MMMM yyyy", new CultureInfo("ar-EG"))))
                .ForMember(d => d.TimeFormatted, opt => opt.MapFrom(src => FormatTimeArabic(src.StartTime)))
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(src => FormatDateTimeArabic(src.CreatedAt)))
                .ForMember(d => d.CompletedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? FormatDateTimeArabic(src.UpdatedAt.Value) : "غير مكتمل"))
                .ForMember(d => d.RequestNumberRelated, opt => opt.MapFrom(src => src.RequestNumber))
                .ForMember(d => d.GovernorateName, opt => opt.MapFrom(src => src.Governorate != null ? src.Governorate.Name : "غير محدد"))
                .ForMember(d => d.TrafficUnitName, opt => opt.MapFrom(src => src.TrafficUnit != null ? src.TrafficUnit.Name : "غير محدد"))
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.GetDisplayName()))
                .ForMember(d => d.AssignedToUserId, opt => opt.MapFrom(src => src.StaffId ?? ResolveAssignedToUserId(src.Type)));

            // Governorate & Traffic Unit
            CreateMap<Governorate, GovernorateDto>();
            CreateMap<TrafficUnit, TrafficUnitDto>();
        }

        private static string GetStatusArabic(ViolationStatus status) => status switch
        {
            ViolationStatus.Unpaid => "غير مدفوعة",
            ViolationStatus.PartiallyPaid => "مدفوعة جزئياً",
            ViolationStatus.Paid => "مدفوعة",
            _ => "غير معروف"
        };

        private static string GetViolationTypeArabic(ViolationType type) => type switch
        {
            ViolationType.SpeedLimitExceeded => "تجاوز السرعة القصوى",
            ViolationType.RedLightViolation => "تجاوز الإشارة الحمراء",
            ViolationType.SeatBeltViolation => "عدم ربط حزام الأمان",
            ViolationType.IllegalParking => "وقوف غير قانوني",
            ViolationType.MobilePhoneUsage => "استخدام الهاتف أثناء القيادة",
            ViolationType.DrivingWithoutLicense => "القيادة بدون رخصة",
            ViolationType.ExpiredLicense => "القيادة برخصة منتهية",
            ViolationType.UnauthorizedModification => "تعديلات غير مصرح بها على المركبة",
            _ => "مخالفة مرورية"
        };

        private static string GetAppointmentTypeName(AppointmentType type) => type switch
        {
            AppointmentType.Medical => "كشف طبي",
            AppointmentType.Driving => "اختبار قيادة",
            AppointmentType.Technical => "فحص فني",
            _ => "غير محدد"
        };

        private static string ResolveAssignedToUserId(AppointmentType type) => type switch
        {
            AppointmentType.Medical => "DOCTOR",
            AppointmentType.Technical => "INSPECTOR",
            AppointmentType.Driving => "EXAMINATOR",
            _ => "STAFF"
        };

        private static string FormatTimeArabic(TimeOnly time)
        {
            return time
                .ToString("hh:mm tt", new CultureInfo("en-US"))
                .Replace("AM", "صباحاً")
                .Replace("PM", "مساءً");
        }

        private static string FormatSystemDate(DateTime dateTime)
        {
            var formatted = dateTime.ToString("d/M/yyyy", new CultureInfo("ar-EG"));
            return formatted.Replace("\u200F", "").Replace("\u200E", "");
        }

        private static string FormatSystemDate(DateOnly dateOnly)
        {
            var dateTime = dateOnly.ToDateTime(TimeOnly.MinValue);
            return FormatSystemDate(dateTime);
        }

        private static string FormatSystemDateTime(DateTime dateTime)
        {
            // Use en-US to get AM/PM instead of Arabic ص/م
            var formatted = dateTime.ToString("hh:mm tt - d/M/yyyy", new CultureInfo("en-US"));
            
            // Strip bidirectional marks just in case
            return formatted.Replace("\u200F", "").Replace("\u200E", "");
        }

        private static string FormatDateTimeArabic(DateTime dateTime)
        {
            var datePart = dateTime.ToString("d MMMM yyyy", new CultureInfo("ar-EG"));
            var timePart = dateTime.ToString("hh:mm tt", new CultureInfo("en-US"))
                           .Replace("AM", "صباحاً")
                           .Replace("PM", "مساءً");

            return $"{datePart} {timePart}".Replace("\u200F", "").Replace("\u200E", "");
        }
    }
}
