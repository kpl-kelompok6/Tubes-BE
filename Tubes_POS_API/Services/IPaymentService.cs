using Tubes_POS_API.Models.DTOs;

namespace Tubes_POS_API.Services;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
    Task<List<PaymentResponse>> GetAllPaymentsAsync(DateTime? startDate, DateTime? endDate, string? paymentMethod, int page, int limit);
    Task<PaymentResponse?> GetPaymentByIdAsync(int id);
}
