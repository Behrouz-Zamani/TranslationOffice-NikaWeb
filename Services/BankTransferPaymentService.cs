using Microsoft.EntityFrameworkCore;
using TranslationOffice.Data;
using TranslationOffice.Models;

namespace TranslationOffice.Services;

public record SubmitBankTransferResult(
    bool IsSuccess,
    int? PaymentId,
    string Message);

public record ReviewBankTransferResult(
    bool IsSuccess,
    string Message);

public class BankTransferPaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly PaymentReceiptStorageService _storage;

    public BankTransferPaymentService(
        ApplicationDbContext db,
        PaymentReceiptStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<SubmitBankTransferResult> SubmitReceiptAsync(
        int orderId,
        string userId,
        string originalFileName,
        byte[] content,
        string? payerName,
        string? transferReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new(false, null, "حساب کاربری معتبر نیست.");

        if (payerName?.Length > 150 || transferReference?.Length > 100)
            return new(false, null, "طول نام واریزکننده یا شماره پیگیری معتبر نیست.");

        var order = await _db.Orders
            .FirstOrDefaultAsync(
                x => x.Id == orderId && x.UserId == userId,
                cancellationToken);

        if (order == null)
            return new(false, null, "سفارش پیدا نشد یا متعلق به حساب شما نیست.");

        if (order.Status != "PendingPayment")
            return new(false, null, "این سفارش در وضعیت قابل پرداخت نیست.");

        var paidAmount = await _db.Payments
            .Where(x => x.OrderId == orderId && x.Status == PaymentStatus.Paid)
            .SumAsync(x => (decimal?)x.AmountToman, cancellationToken) ?? 0m;

        var remaining = order.TotalPrice - paidAmount;
        if (remaining <= 0)
            return new(false, null, "مبلغ این سفارش قبلاً تسویه شده است.");

        var hasPendingReceipt = await _db.Payments.AnyAsync(
            x => x.OrderId == orderId && x.Status == PaymentStatus.Pending,
            cancellationToken);

        if (hasPendingReceipt)
            return new(false, null, "رسید قبلی شما هنوز در انتظار بررسی مدیر است.");

        (string StoredFileName, string ContentType) saved;
        try
        {
            saved = await _storage.SaveAsync(
                originalFileName,
                content,
                cancellationToken);
        }
        catch (Exception ex) when (
            ex is InvalidOperationException or IOException)
        {
            return new(false, null, ex.Message);
        }

        try
        {
            var payment = new Payment
            {
                OrderId = order.Id,
                AmountToman = remaining,
                Status = PaymentStatus.Pending,
                Gateway = "BankTransfer",
                ReceiptOriginalFileName = Path.GetFileName(originalFileName),
                ReceiptStoredFileName = saved.StoredFileName,
                ReceiptContentType = saved.ContentType,
                ReceiptFileSize = content.LongLength,
                PayerName = string.IsNullOrWhiteSpace(payerName)
                    ? null
                    : payerName.Trim(),
                TransferReference = string.IsNullOrWhiteSpace(transferReference)
                    ? null
                    : transferReference.Trim(),
                CreatedAt = DateTime.Now
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(cancellationToken);

            return new(
                true,
                payment.Id,
                "رسید شما با موفقیت ثبت شد و پس از بررسی مدیر نتیجه اعلام می‌شود.");
        }
        catch
        {
            await _storage.DeleteAsync(saved.StoredFileName);
            throw;
        }
    }

    public async Task<ReviewBankTransferResult> ReviewAsync(
        int paymentId,
        string adminUserId,
        bool approve,
        string? rejectionReason,
        CancellationToken cancellationToken = default)
    {
        var isAdmin = await _db.Users
            .AnyAsync(
                x => x.Id == adminUserId && x.Role == "Admin",
                cancellationToken);

        if (!isAdmin)
            return new(false, "دسترسی مدیر معتبر نیست.");

        if (!approve && string.IsNullOrWhiteSpace(rejectionReason))
            return new(false, "برای رد رسید، درج علت الزامی است.");

        if (rejectionReason?.Length > 500)
            return new(false, "علت رد رسید نباید بیشتر از ۵۰۰ کاراکتر باشد.");

        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var payment = await _db.Payments
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment == null ||
            payment.Gateway != "BankTransfer" ||
            string.IsNullOrWhiteSpace(payment.ReceiptStoredFileName))
            return new(false, "رسید پرداخت پیدا نشد.");

        if (payment.Status != PaymentStatus.Pending)
            return new(false, "این رسید قبلاً بررسی شده است.");

        if (payment.Order.Status != "PendingPayment")
            return new(false, "وضعیت سفارش اجازه بررسی این پرداخت را نمی‌دهد.");

        if (approve)
        {
            var otherPaidAmount = await _db.Payments
                .Where(x =>
                    x.OrderId == payment.OrderId &&
                    x.Id != payment.Id &&
                    x.Status == PaymentStatus.Paid)
                .SumAsync(x => (decimal?)x.AmountToman, cancellationToken) ?? 0m;

            var remaining = payment.Order.TotalPrice - otherPaidAmount;

            if (payment.AmountToman <= 0 || payment.AmountToman > remaining)
                return new(false, "مبلغ این تراکنش با مانده سفارش مطابقت ندارد.");

            payment.Status = PaymentStatus.Paid;
            payment.FailureReason = null;
            payment.VerifiedAt = DateTime.Now;

            if (otherPaidAmount + payment.AmountToman >= payment.Order.TotalPrice)
                payment.Order.Status = "Paid";
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = rejectionReason!.Trim();
        }

        payment.ReviewedAt = DateTime.Now;
        payment.ReviewedByUserId = adminUserId;

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new(
            true,
            approve
                ? "رسید تأیید شد و مبلغ در سوابق پرداخت ثبت شد."
                : "رسید رد شد؛ مشتری می‌تواند رسید جدیدی ارسال کند.");
    }
}
