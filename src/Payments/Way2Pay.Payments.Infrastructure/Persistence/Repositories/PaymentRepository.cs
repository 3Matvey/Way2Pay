using Microsoft.EntityFrameworkCore;
using Way2Pay.Payments.Application.Payments;
using Way2Pay.Payments.Domain.Payments;

namespace Way2Pay.Payments.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository(PaymentsDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(
        Guid merchantId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payments = dbContext.Payments.AsNoTracking();
        
        return payments
            .Include(payment => payment.Operations)
            .ThenInclude(operation => operation.Attempts.OrderBy(attempt => attempt.Number))
            .Include(payment => payment.Operations).ThenInclude(operation => operation.Route!)
            .ThenInclude(route => route.Steps.OrderBy(step => step.Position))
            .SingleOrDefaultAsync(payment => payment.Id == paymentId && payment.MerchantId == merchantId,
                cancellationToken);
    }

    public void Add(Payment payment) => dbContext.Payments.Add(payment);
}
