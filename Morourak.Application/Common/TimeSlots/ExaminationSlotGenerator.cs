namespace Morourak.Application.Common.TimeSlots;

public static class ExaminationSlotGenerator
{
    public static List<(TimeOnly Start, TimeOnly End)> GenerateDailySlots()
    {
        var slots = new List<(TimeOnly, TimeOnly)>();

        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(14, 0);

        while (start < end)
        {
            var next = start.AddMinutes(15);
            slots.Add((start, next));
            start = next;
        }

        return slots;
    }
}
