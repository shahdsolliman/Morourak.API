using Morourak.Application.DTOs.Appointments;
using Morourak.Domain.Entities;

namespace Morourak.Application.Mapping
{
    public static class ExaminationMapping
    {
        public static AppointmentDto ToDto(this Appointment entity)
        {
            return new AppointmentDto
            {
                Type = entity.Type,
                Date = entity.Date,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Status = entity.Status,

            };
        }
    }
}
