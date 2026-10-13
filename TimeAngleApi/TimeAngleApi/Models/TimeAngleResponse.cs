namespace TimeAngleApi.Models;

public class TimeAngleResponse
{
    public required string Time { get; init; }
    public required double HourHandAngle { get; init; }
    public required double MinuteHandAngle { get; init; }
    public required double SumOfAngles { get; init; }
}
