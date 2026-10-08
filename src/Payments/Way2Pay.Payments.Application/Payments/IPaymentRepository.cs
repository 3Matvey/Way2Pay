using Way2Pay.Payments.Domain.Payments;

namespace Way2Pay.Payments.Application.Payments;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
