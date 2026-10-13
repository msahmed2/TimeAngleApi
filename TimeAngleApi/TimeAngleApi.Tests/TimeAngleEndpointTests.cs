using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using TimeAngleApi.Models;
using Xunit;

namespace TimeAngleApi.Tests;

public  class TimeAngleEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string InvalidTimeDetail = "Invalid time format. Expected H:mm or HH:mm (e.g. \"3:00\" or \"15:30\").";
    private const string MissingInputDetail = "Provide either a 'time' query parameter (e.g. time=03:00) or both 'hour' and 'minute' query parameters.";
    private const string InvalidHourDetail = "Hour must be between 0 and 23 inclusive.";
    private const string InvalidMinuteDetail = "Minute must be between 0 and 59 inclusive.";
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TimeAngleEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("03:00", "03:00", 90.0, 0.0, 90.0)]
    [InlineData("23:59", "23:59", 359.5, 354.0, 713.5)]
    [InlineData("00:59", "00:59", 29.5, 354.0, 383.5)]
    [InlineData("12:00", "12:00", 0.0, 0.0, 0.0)]
    [InlineData("06:00", "06:00", 180.0, 0.0, 180.0)]
    [InlineData("3:05", "03:05", 92.5, 30.0, 122.5)]
    [InlineData("1:59", "01:59", 59.5, 354.0, 413.5)]
    [InlineData("11:59", "11:59", 359.5, 354.0, 713.5)]
    public async Task Calculate_WithTimeParameter_ReturnsCorrectAngles(
        string time, string expectedTime,
        double expectedHourAngle, double expectedMinuteAngle, double expectedSum)
    {
        var response = await _client.GetAsync($"/CalculateTimeAngle?time={Uri.EscapeDataString(time)}");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TimeAngleResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(expectedTime, body.Time);
        Assert.Equal(expectedHourAngle, body.HourHandAngle, precision: 10);
        Assert.Equal(expectedMinuteAngle, body.MinuteHandAngle, precision: 10);
        Assert.Equal(expectedSum, body.SumOfAngles, precision: 10);
    }

    [Theory]
    [InlineData(3, 0, "03:00", 90.0, 0.0, 90.0)]
    [InlineData(0, 0, "00:00", 0.0, 0.0, 0.0)]
    [InlineData(0, 30, "00:30", 15.0, 180.0, 195.0)]
    [InlineData(15, 30, "15:30", 105.0, 180.0, 285.0)]
    [InlineData(23, 59, "23:59", 359.5, 354.0, 713.5)]
    [InlineData(0, 59, "00:59", 29.5, 354.0, 383.5)]
    [InlineData(12, 0, "12:00", 0.0, 0.0, 0.0)]
    [InlineData(6, 0, "06:00", 180.0, 0.0, 180.0)]
    [InlineData(1, 59, "01:59", 59.5, 354.0, 413.5)]
    [InlineData(11, 59, "11:59", 359.5, 354.0, 713.5)]
    public async Task Calculate_WithHourAndMinuteParameters_ReturnsCorrectAngles(
        int hour, int minute, string expectedTime,
        double expectedHourAngle, double expectedMinuteAngle, double expectedSum)
    {
        var response = await _client.GetAsync($"/CalculateTimeAngle?hour={hour}&minute={minute}");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TimeAngleResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(expectedTime, body.Time);
        Assert.Equal(expectedHourAngle, body.HourHandAngle, precision: 10);
        Assert.Equal(expectedMinuteAngle, body.MinuteHandAngle, precision: 10);
        Assert.Equal(expectedSum, body.SumOfAngles, precision: 10);
    }

    [Theory]
    [InlineData("time=03:05&hour=23&minute=59")]
    [InlineData("time=03:05&hour=24&minute=60")]
    [InlineData("time=03:05&hour=23")]
    [InlineData("time=03:05&minute=59")]
    public async Task Calculate_WithBothInputForms_PrefersTime(string query)
    {
        var response = await _client.GetAsync($"/CalculateTimeAngle?{query}");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TimeAngleResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("03:05", body.Time);
        Assert.Equal(92.5, body.HourHandAngle, precision: 10);
        Assert.Equal(30.0, body.MinuteHandAngle, precision: 10);
        Assert.Equal(122.5, body.SumOfAngles, precision: 10);
    }

    [Fact]
    public async Task Calculate_With24HourTime_NormalizesCorrectly()
    {
        var response = await _client.GetAsync("/CalculateTimeAngle?time=15:00");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TimeAngleResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("15:00", body.Time);
        Assert.Equal(90.0, body.SumOfAngles);
    }

    [Theory]
    [InlineData("/CalculateTimeAngle", MissingInputDetail)]
    [InlineData("/CalculateTimeAngle?hour=3", MissingInputDetail)]
    [InlineData("/CalculateTimeAngle?minute=0", MissingInputDetail)]
    [InlineData("/CalculateTimeAngle?hour=0", MissingInputDetail)]
    [InlineData("/CalculateTimeAngle?time=", MissingInputDetail)]
    [InlineData("/CalculateTimeAngle?hour=abc&minute=0", null)]
    [InlineData("/CalculateTimeAngle?hour=3&minute=abc", null)]
    [InlineData("/CalculateTimeAngle?time=03:00&hour=abc&minute=0", null)]
    [InlineData("/CalculateTimeAngle?hour=-1&minute=0", InvalidHourDetail)]
    [InlineData("/CalculateTimeAngle?hour=3&minute=-1", InvalidMinuteDetail)]
    [InlineData("/CalculateTimeAngle?time=abc", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=3::00", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=:3:00", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=3:00:", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=3:0", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=03:5", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=%2003:00%20", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=%2003:00", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=03:00%20", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=003:00", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=3:00:00", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=25:00", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=24:00", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=23:60", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?time=abc&hour=3&minute=0", InvalidTimeDetail)]
    [InlineData("/CalculateTimeAngle?hour=24&minute=0", InvalidHourDetail)]
    [InlineData("/CalculateTimeAngle?hour=3&minute=60", InvalidMinuteDetail)]
    public async Task Calculate_WithInvalidInput_ReturnsProblemDetails(string url, string? expectedDetail)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal(expectedDetail, problem.Detail);
        Assert.Equal(expectedDetail is null ? "One or more validation errors occurred." : "Invalid request", problem.Title);

        if (expectedDetail is null)
        {
            var errors = Assert.IsType<JsonElement>(problem.Extensions["errors"]);
            string parameter = url.Contains("hour=abc") ? "Hour" : "Minute";
            Assert.Equal("The value 'abc' is not valid for " + parameter + ".", errors.GetProperty(parameter)[0].GetString());
        }
    }

    [Fact]
    public async Task Calculate_WithSingleDigitHour_ParsesSuccessfully()
    {
        var response = await _client.GetAsync("/CalculateTimeAngle?time=3:00");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TimeAngleResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(90.0, body.SumOfAngles);
    }
}
