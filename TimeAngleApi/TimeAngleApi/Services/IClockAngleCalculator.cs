namespace TimeAngleApi.Services;

public interface IClockAngleCalculator
{
    double CalculateSumOfHandAngles(int hour, int minute);
    (double HourHandAngle, double MinuteHandAngle, double SumOfAngles) CalculateHandAngles(int hour, int minute);
}
