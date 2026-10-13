using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using TimeAngleApi.Models;
using TimeAngleApi.Services;

namespace TimeAngleApi.Controllers;

[ApiController]
[Route("CalculateTimeAngle")]
public class TimeAngleController : ControllerBase
{
    private readonly IClockAngleCalculator _calculator;

    public TimeAngleController(IClockAngleCalculator calculator)
    {
        _calculator = calculator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(TimeAngleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<TimeAngleResponse> Calculate(
        [FromQuery] TimeAngleRequest request)
    {
        int resolvedHour;
        int resolvedMinute;

        if (!string.IsNullOrWhiteSpace(request.Time))
        {
            if (!TryParseTime(request.Time, out resolvedHour, out resolvedMinute))
            {
                return BadRequest(CreateProblem(
                    "Invalid time format. Expected H:mm or HH:mm (e.g. \"3:00\" or \"15:30\")."));
            }
        }
        else if (request.Hour.HasValue && request.Minute.HasValue)
        {
            resolvedHour = request.Hour.Value;
            resolvedMinute = request.Minute.Value;
        }
        else
        {
            return BadRequest(CreateProblem(
                "Provide either a 'time' query parameter (e.g. time=03:00) " +
                "or both 'hour' and 'minute' query parameters."));
        }

        if (resolvedHour is < 0 or > 23)
        {
            return BadRequest(CreateProblem("Hour must be between 0 and 23 inclusive."));
        }

        if (resolvedMinute is < 0 or > 59)
        {
            return BadRequest(CreateProblem("Minute must be between 0 and 59 inclusive."));
        }

        var angles = _calculator.CalculateHandAngles(resolvedHour, resolvedMinute);

        var response = new TimeAngleResponse
        {
            Time = $"{resolvedHour:D2}:{resolvedMinute:D2}",
            HourHandAngle = angles.HourHandAngle,
            MinuteHandAngle = angles.MinuteHandAngle,
            SumOfAngles = angles.SumOfAngles
        };

        return Ok(response);
    }

    private static bool TryParseTime(string time, out int hour, out int minute)
    {
        bool parsed = TimeOnly.TryParseExact(
            time,
            ["H:mm", "HH:mm"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedTime);

        hour = parsedTime.Hour;
        minute = parsedTime.Minute;
        return parsed;
    }

    private static ProblemDetails CreateProblem(string detail) =>
        new()
        {
            Title = "Invalid request",
            Detail = detail,
            Status = StatusCodes.Status400BadRequest
        };
}
