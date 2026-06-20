using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tubes_POS_API.Models;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Controllers;

[Authorize]
[ApiController]
[Route("api/payments")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PaymentResponse>>>> GetAll(
        DateTime? startDate, DateTime? endDate, string? paymentMethod, int page = 1, int limit = 20)
    {
        var result = await _paymentService.GetAllPaymentsAsync(startDate, endDate, paymentMethod, page, limit);

        return Ok(new ApiResponse<List<PaymentResponse>>
        {
            Message = $"Ditemukan {result.Count} pembayaran.",
            Data = result
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetById(int id)
    {
        var result = await _paymentService.GetPaymentByIdAsync(id);

        if (result is null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Pembayaran tidak ditemukan.",
                Data = null
            });
        }

        return Ok(new ApiResponse<PaymentResponse>
        {
            Message = "Detail pembayaran.",
            Data = result
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> Process([FromBody] PaymentRequest request)
    {
        var result = await _paymentService.ProcessPaymentAsync(request);

        return Ok(new ApiResponse<PaymentResponse>
        {
            Message = "Pembayaran berhasil diproses.",
            Data = result
        });
    }
}
