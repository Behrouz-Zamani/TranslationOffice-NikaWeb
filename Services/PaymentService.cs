using Microsoft.EntityFrameworkCore;
using TranslationOffice.Data;
using TranslationOffice.Models;

namespace TranslationOffice.Services;

public record StartPaymentResult(
    bool IsSuccess,
    int PaymentId,
    string? PaymentUrl,
    string? Message);

public record VerifyPaymentResult(
    bool IsSuccess,
    int OrderId,
    string? ReferenceId,
    string? Message);

public class PaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly IPaymentGateway _gateway;

    public PaymentService(
        ApplicationDbContext db,
        IPaymentGateway gateway)
    {
        _db = db;
        _gateway = gateway;
    }

    // شروع پرداخت برای سفارشی که متعلق به کاربر واردشده است
    public async Task<StartPaymentResult> StartAsync(
        int orderId,
        string userId,
        string callbackUrl,
        CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(
                x => x.Id == orderId && x.UserId == userId,
                cancellationToken);

        if (order == null)
        {
            return new(false, 0, null, "سفارش پیدا نشد.");
        }

        if (order.Status != "PendingPayment")
        {
            return new(false, 0, null, "وضعیت سفارش اجازه پرداخت نمی‌دهد.");
        }

        if (order.TotalPrice <= 0)
        {
            return new(false, 0, null, "مبلغ سفارش معتبر نیست.");
        }

        // جلوگیری از ایجاد چند پرداخت هم‌زمان برای یک سفارش
        var pendingPayment = await _db.Payments
            .AnyAsync(
                x => x.OrderId == orderId &&
                     x.Status == PaymentStatus.Pending,
                cancellationToken);

        if (pendingPayment)
        {
            return new(false, 0, null,
                "برای این سفارش یک درخواست پرداخت در انتظار وجود دارد.");
        }

        var payment = new Payment
        {
            OrderId = order.Id,
            AmountToman = order.TotalPrice,
            Status = PaymentStatus.Pending,
            Gateway = "Zarinpal",
            CreatedAt = DateTime.Now
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);

        // درگاه مبلغ را به تومان دریافت می‌کند.
        // تبدیل تومان به ریال در آداپتور اختصاصی زرین‌پال انجام می‌شود.
        var actualCallbackUrl = callbackUrl.Replace(
            "{paymentId}",
            payment.Id.ToString());

        var gatewayResult = await _gateway.CreatePaymentAsync(
            payment.AmountToman,
            order.OrderNumber,
            actualCallbackUrl,
            cancellationToken);

        if (!gatewayResult.IsSuccess ||
            string.IsNullOrWhiteSpace(gatewayResult.Authority) ||
            string.IsNullOrWhiteSpace(gatewayResult.PaymentUrl))
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason =
                gatewayResult.Message ?? "ایجاد درخواست پرداخت ناموفق بود.";

            await _db.SaveChangesAsync(cancellationToken);

            return new(
                false,
                payment.Id,
                null,
                payment.FailureReason);
        }

        payment.Authority = gatewayResult.Authority;

        await _db.SaveChangesAsync(cancellationToken);

        return new(
            true,
            payment.Id,
            gatewayResult.PaymentUrl,
            "درخواست پرداخت ایجاد شد.");
    }

    // تأیید پرداخت از طریق استعلام مستقیم از درگاه
    public async Task<VerifyPaymentResult> VerifyAsync(
        int paymentId,
        string authority,
        CancellationToken cancellationToken = default)
    {
        var payment = await _db.Payments
            .Include(x => x.Order)
            .FirstOrDefaultAsync(
                x => x.Id == paymentId,
                cancellationToken);

        if (payment == null)
        {
            return new(false, 0, null, "تراکنش پیدا نشد.");
        }

        if (string.IsNullOrWhiteSpace(payment.Authority) ||
            !string.Equals(
                payment.Authority,
                authority,
                StringComparison.Ordinal))
        {
            return new(false, payment.OrderId, null,
                "شناسه تراکنش معتبر نیست.");
        }

        // Callback تکراری نباید باعث ثبت مجدد پرداخت شود.
        if (payment.Status == PaymentStatus.Paid)
        {
            return new(
                true,
                payment.OrderId,
                payment.ReferenceId,
                "این پرداخت قبلاً تأیید شده است.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return new(false, payment.OrderId, null,
                "این تراکنش در وضعیت قابل تأیید نیست.");
        }

        // تأیید واقعی باید از خود درگاه گرفته شود؛
        // پارامتر موفقیت Callback به‌تنهایی قابل اعتماد نیست.
        var gatewayResult = await _gateway.VerifyPaymentAsync(
            payment.Authority,
            payment.AmountToman,
            cancellationToken);

        if (!gatewayResult.IsSuccess)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason =
                gatewayResult.Message ?? "پرداخت توسط درگاه تأیید نشد.";

            await _db.SaveChangesAsync(cancellationToken);

            return new(
                false,
                payment.OrderId,
                null,
                payment.FailureReason);
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        // قبل از ثبت نتیجه، رکورد را مجدداً از دیتابیس می‌خوانیم.
        var currentPayment = await _db.Payments
            .Include(x => x.Order)
            .FirstOrDefaultAsync(
                x => x.Id == paymentId,
                cancellationToken);

        if (currentPayment == null)
        {
            return new(false, 0, null, "تراکنش پیدا نشد.");
        }

        if (currentPayment.Status == PaymentStatus.Paid)
        {
            await transaction.CommitAsync(cancellationToken);

            return new(
                true,
                currentPayment.OrderId,
                currentPayment.ReferenceId,
                "این پرداخت قبلاً تأیید شده است.");
        }

        if (currentPayment.Status != PaymentStatus.Pending)
        {
            return new(
                false,
                currentPayment.OrderId,
                null,
                "وضعیت تراکنش تغییر کرده است.");
        }

        if (currentPayment.Order.Status != "PendingPayment")
        {
            return new(
                false,
                currentPayment.OrderId,
                null,
                "وضعیت سفارش اجازه ثبت این پرداخت را نمی‌دهد.");
        }

        currentPayment.Status = PaymentStatus.Paid;
        currentPayment.ReferenceId = gatewayResult.ReferenceId;
        currentPayment.VerifiedAt = DateTime.Now;
        currentPayment.FailureReason = null;

        currentPayment.Order.Status = "Paid";

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new(
            true,
            currentPayment.OrderId,
            currentPayment.ReferenceId,
            "پرداخت با موفقیت تأیید شد.");
    }

    // ثبت انصراف یا پرداخت ناموفق اعلام‌شده در Callback
    public async Task MarkFailedAsync(
        int paymentId,
        string authority,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var payment = await _db.Payments
            .FirstOrDefaultAsync(
                x => x.Id == paymentId,
                cancellationToken);

        if (payment == null ||
            payment.Status != PaymentStatus.Pending ||
            payment.Authority != authority)
        {
            return;
        }

        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = reason;

        await _db.SaveChangesAsync(cancellationToken);
    }
}