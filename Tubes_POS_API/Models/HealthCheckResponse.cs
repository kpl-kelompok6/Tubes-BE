namespace Tubes_POS_API.Models;

public sealed class HealthCheckResponse
{
    public string Status { get; set; } = "ok";

    public string Probe { get; set; } = "health";

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public Dictionary<string, string> Checks { get; set; } = [];
}
