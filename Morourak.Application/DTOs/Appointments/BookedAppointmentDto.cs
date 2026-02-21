using Morourak.Domain.Enums.Appointments;

namespace Morourak.Application.DTOs.Appointments;

public class BookedAppointmentDto
{
    public string ServiceNumber { get; set; }       
    public int ApplicationId { get; set; }          
    public DateOnly Date { get; set; }              
    public TimeOnly StartTime { get; set; }         
    public AppointmentStatus Status { get; set; }   
    public string NationalId { get; set; }          
    public AppointmentType Type { get; set; }
}