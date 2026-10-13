namespace TimeAngleApi.Services;

public class ClockAngleCalculator : IClockAngleCalculator
{
    private const double DegreesPerHourMark = 30.0;
    private const double DegreesPerMinuteMark = 6.0;
    private const double DegreesPerMinuteOnHourHand = 0.5;

    public double CalculateSumOfHandAngles(int hour, int minute) =>
        CalculateHandAngles(hour, minute).SumOfAngles;

    public (double HourHandAngle, double MinuteHandAngle, double SumOfAngles) CalculateHandAngles(int hour, int minute)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(hour, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(hour, 23);
        ArgumentOutOfRangeException.ThrowIfLessThan(minute, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minute, 59);

        int hourOnFace = hour % 12;

        double hourHandAngle = (DegreesPerHourMark * hourOnFace)
                             + (DegreesPerMinuteOnHourHand * minute);


        double minuteHandAngle = DegreesPerMinuteMark * minute;

        return (hourHandAngle, minuteHandAngle, hourHandAngle + minuteHandAngle);
    }
}
