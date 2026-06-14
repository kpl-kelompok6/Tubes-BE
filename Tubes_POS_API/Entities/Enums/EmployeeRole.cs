using System.Text.Json.Serialization;

namespace Tubes_POS_API.Entities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmployeeRole
{
    Admin,
    Kasir
}
