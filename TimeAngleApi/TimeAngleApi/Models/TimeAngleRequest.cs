namespace TimeAngleApi.Models;

public class TimeAngleRequest
{
    public string? Time { get; init; }
    public int? Hour { get; init; }
    public int? Minute { get; init; }
}