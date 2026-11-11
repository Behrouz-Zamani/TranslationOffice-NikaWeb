namespace TranslationOffice.Services;

public class UnavailablePaymentGateway : IPaymentGateway
{
    public Task<GatewayPaymentRequestResult> CreatePaymentAsync(
        decimal amountToman,
        string orderNumber,
        string callbackUrl,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new GatewayPaymentRequestResult(
                false,
                null,
                null,
                "درگاه پرداخت هنوز پیکربندی نشده است."));
    }

    public Task<GatewayPaymentVerifyResult> VerifyPaymentAsync(
        string authority,
        decimal amountToman,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new GatewayPaymentVerifyResult(
                false,
                null,
                "درگاه پرداخت هنوز پیکربندی نشده است."));
    }
}