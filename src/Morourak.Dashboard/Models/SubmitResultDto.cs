namespace Morourak.Dashboard.Models
{
    public class SubmitResultDto
    {
        public string RequestNumber { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string? Notes { get; set; }

        // Technical Checklist
        public bool BrakesOk { get; set; }
        public bool LightsOk { get; set; }
        public bool TiresOk { get; set; }
        public bool BodyOk { get; set; }

        // Driving Scoring
        public int ParkingScore { get; set; }
        public int RoadControlScore { get; set; }
        public int RulesComplianceScore { get; set; }
    }
}
