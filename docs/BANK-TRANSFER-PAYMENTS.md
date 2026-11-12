# راه‌اندازی پرداخت کارت‌به‌کارت

## تنظیم اطلاعات حساب مقصد

اطلاعات حساب مقصد عمداً داخل مخزن Git ثبت نشده است. در محیط توسعه، از User Secrets استفاده کنید:

```powershell
dotnet user-secrets set "BankTransfer:BankName" "نام بانک"
dotnet user-secrets set "BankTransfer:AccountHolder" "نام صاحب حساب"
dotnet user-secrets set "BankTransfer:CardNumber" "شماره کارت"
dotnet user-secrets set "BankTransfer:Sheba" "شماره شبا"
dotnet user-secrets set "BankTransfer:AccountNumber" "شماره حساب"
```

در سرور، همین مقادیر را با متغیرهای محیطی زیر تنظیم کنید:

- `BankTransfer__BankName`
- `BankTransfer__AccountHolder`
- `BankTransfer__CardNumber`
- `BankTransfer__Sheba`
- `BankTransfer__AccountNumber`

حداقل یکی از شماره کارت، شبا یا شماره حساب باید تنظیم شود تا فرم ارسال رسید برای مشتری فعال شود.

## اعمال تغییرات دیتابیس

پس از دریافت آخرین نسخه شاخه، در پوشه پروژه اجرا کنید:

```powershell
dotnet ef database update
```

این دستور migration با نام `AddBankTransferReceipts` را اعمال می‌کند.

## مسیر ذخیره رسیدها

فایل‌های رسید خارج از wwwroot و در مسیر زیر ذخیره می‌شوند:

```
App_Data/PaymentReceipts
```

کاربر فقط در صورت مالک بودن سفارش و مدیر فقط با نقش Admin امکان دانلود رسید را دارد. پوشه بالا باید روی سرور قابل نوشتن و در برنامه پشتیبان‌گیری سرور لحاظ شود.

## گردش کار

1. مشتری پس از ورود به حساب و اتصال سفارش مهمان به حساب، جزئیات سفارش را باز می‌کند.
2. مبلغ مانده و اطلاعات حساب مقصد را می‌بیند و رسید PDF/JPG/PNG حداکثر ۵ مگابایت ارسال می‌کند.
3. تراکنش با وضعیت Pending ذخیره می‌شود؛ این وضعیت به معنی «در انتظار بررسی رسید» است و سفارش هنوز Paid نمی‌شود.
4. مدیر از صفحه جزئیات سفارش، رسید را دانلود و تأیید یا با ذکر علت رد می‌کند.
5. فقط پس از تأیید مدیر، تراکنش Paid می‌شود و اگر کل مبلغ سفارش تسویه شده باشد، وضعیت سفارش به Paid تغییر می‌کند.

پرداخت ناقص در این نسخه پشتیبانی نمی‌شود؛ مشتری باید مبلغ مانده نمایش‌داده‌شده را یکجا واریز کند.
