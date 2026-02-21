using Morourak.Domain.Common;
using Morourak.Domain.Enums;
using Morourak.Domain.Enums.Driving;

namespace Morourak.Domain.Entities
{
    public class RenewalApplication : BaseEntity<int>
    {

        public int CitizenRegistryId { get; set; }
        public CitizenRegistry Citizen { get; set; }

        public int DrivingLicenseId { get; set; }
        public DrivingLicense DrivingLicense { get; set; }

        // ================= Category =================
        public DrivingLicenseCategory CurrentCategory { get; set; }
        public DrivingLicenseCategory RequestedCategory { get; set; }

        public string MedicalCertificatePath { get; set; }

        public LicenseStatus Status { get; set; }

        public DateTime SubmittedAt { get; set; }

        // ================= Navigation =================
        public ICollection<Appointment> Appointments { get; set; }
    }
}