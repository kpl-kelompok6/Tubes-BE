using Microsoft.AspNetCore.Http;

namespace Tubes_POS_API.Tests;

public sealed class NullHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }
}
