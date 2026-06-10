namespace Tubes_POS_API.Helpers;

public static class CodeHelper
{
    public static string GenerateCode(string prefix)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{suffix}";
    }
}
