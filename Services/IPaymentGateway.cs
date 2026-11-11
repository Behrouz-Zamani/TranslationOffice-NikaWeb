namespace TranslationOffice.Services;

public record GatewayPaymentRequestResult(
    bool IsSuccess,
    string? Authority,
    string? PaymentUrl,
    string? Message);

public record GatewayPaymentVerifyResult(
    bool IsSuccess,
    string? ReferenceId,
    string? Message);

public interface IPaymentGateway
{
    Task<GatewayPaymentRequestResult> CreatePaymentAsync(
        decimal amountToman,
        string orderNumber,
        string callbackUrl,
        CancellationToken cancellationToken = default);

    Task<GatewayPaymentVerifyResult> VerifyPaymentAsync(
        string authority,
        decimal amountToman,
        CancellationToken cancellationToken = default);
}