using TimeAngleApi.Services;

namespace TimeAngleApi.Tests;

public class ClockAngleCalculatorTests
{
    private readonly ClockAngleCalculator _calculator = new();

    [Theory]
    [InlineData(0, 0, 0.0)]
    [InlineData(12, 0, 0.0)]
    [InlineData(3, 0, 90.0)]
    [InlineData(15, 0, 90.0)]
    [InlineData(6, 0, 180.0)]
    [InlineData(9, 0, 270.0)]
    [InlineData(0, 30, 195.0)]
    [InlineData(2, 20, 190.0)]
    [InlineData(11, 59, 713.5)]
    [InlineData(23, 59, 713.5)]
    [InlineData(0, 59, 383.5)]
    [InlineData(1, 59, 413.5)]

    public void CalculateSumOfHandAngles_ReturnsExpectedSum(int hour, int minute, double expected)
    {
        double actual = _calculator.CalculateSumOfHandAngles(hour, minute);
        Assert.Equal(expected, actual, precision: 10);
    }

    [Theory]
    [InlineData(0, 0, 0.0, 0.0, 0.0)]
    [InlineData(12, 0, 0.0, 0.0, 0.0)]
    [InlineData(6, 0, 180.0, 0.0, 180.0)]
    [InlineData(0, 59, 29.5, 354.0, 383.5)]
    [InlineData(1, 59, 59.5, 354.0, 413.5)]
    [InlineData(11, 59, 359.5, 354.0, 713.5)]
    [InlineData(23, 59, 359.5, 354.0, 713.5)]
    public void CalculateHandAngles_ReturnsExpectedAngles(
        int hour, int minute, double expectedHourAngle, double expectedMinuteAngle, double expectedSum)
    {
        var actual = _calculator.CalculateHandAngles(hour, minute);

        Assert.Equal(expectedHourAngle, actual.HourHandAngle, precision: 10);
        Assert.Equal(expectedMinuteAngle, actual.MinuteHandAngle, precision: 10);
        Assert.Equal(expectedSum, actual.SumOfAngles, precision: 10);
        Assert.Equal(actual.HourHandAngle + actual.MinuteHandAngle, actual.SumOfAngles);
    }

    [Theory]
    [InlineData(-1, 0, "hour")]
    [InlineData(24, 0, "hour")]
    [InlineData(int.MinValue, 0, "hour")]
    [InlineData(int.MaxValue, 0, "hour")]
    [InlineData(0, -1, "minute")]
    [InlineData(0, 60, "minute")]
    [InlineData(0, int.MinValue, "minute")]
    [InlineData(0, int.MaxValue, "minute")]
    public void Calculate_WithInvalidInput_ThrowsArgumentOutOfRangeException(
        int hour, int minute, string expectedParameter)
    {
        var anglesException = Assert.Throws<ArgumentOutOfRangeException>(
            () => _calculator.CalculateHandAngles(hour, minute));
        var sumException = Assert.Throws<ArgumentOutOfRangeException>(
            () => _calculator.CalculateSumOfHandAngles(hour, minute));

        Assert.Equal(expectedParameter, anglesException.ParamName);
        Assert.Equal(expectedParameter, sumException.ParamName);
    }

    [Fact]
    public void HourHandMovesContinuouslyWithMinutes()
    {
        double atThree = _calculator.CalculateSumOfHandAngles(3, 0);
        Assert.Equal(90.0, atThree);

        double atThreeThirty = _calculator.CalculateSumOfHandAngles(3, 30);
        Assert.Equal(285.0, atThreeThirty);
    }
}
