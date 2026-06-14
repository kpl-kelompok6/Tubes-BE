using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tubes_POS_API.Models;

public sealed class ApiErrorResponse
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public bool Success { get; init; } = false;

    public string Message { get; init; } = string.Empty;

    public int StatusCode { get; init; }

    public IEnumerable<string> Errors { get; init; } = [];
}
