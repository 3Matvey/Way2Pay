using Way2Pay.Merchants.Application.Common;
using Way2Pay.Merchants.Domain.Merchants;

namespace Way2Pay.Merchants.Application.Merchants.CreateMerchant;

public sealed class CreateMerchantUseCase(
    IMerchantRepository merchantRepository, IUnitOfWork unitOfWork) : IUseCase
{
    /// <summary>Creates an active merchant and returns its normalized name and identifier after saving.</summary>
    public async Task<Result<CreateMerchantResponse>> ExecuteAsync(
        CreateMerchantRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 200)
            return CreateMerchantErrors.InvalidName();

        var merchant = new Merchant(request.Name);
        merchantRepository.Add(merchant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateMerchantResponse(merchant.Id, merchant.Name, merchant.Status);
    }
}