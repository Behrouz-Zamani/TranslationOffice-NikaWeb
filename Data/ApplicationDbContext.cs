
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TranslationOffice.Models;

namespace TranslationOffice.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<OrderDocument> OrderDocuments { get; set; }
    public DbSet<OrderDeliveryFile> OrderDeliveryFiles { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // تنظیمات سفارش
        builder.Entity<Order>(entity =>
        {
            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.ProgressStatus)
                .HasMaxLength(30)
                .IsRequired()
                .HasDefaultValue("Pending");

            entity.Property(x => x.GuestAccessTokenHash)
                .HasMaxLength(64);

            entity.HasIndex(x => x.GuestAccessTokenHash);

            entity.Property(x => x.TotalPrice)
                .HasPrecision(18, 0);
        });

        // سبدهای سفارش مشتری و مهمان
        builder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CartKey).HasMaxLength(500).IsRequired();
            entity.Property(x => x.CartJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.HasIndex(x => x.CartKey).IsUnique();
        });

        // پرداخت
        builder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AmountToman)
                .HasPrecision(18, 0);

            entity.Property(x => x.Gateway)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Authority)
                .HasMaxLength(100);

            entity.Property(x => x.ReferenceId)
                .HasMaxLength(100);

            entity.Property(x => x.FailureReason)
                .HasMaxLength(500);

            entity.Property(x => x.ReceiptOriginalFileName)
                .HasMaxLength(255);

            entity.Property(x => x.ReceiptStoredFileName)
                .HasMaxLength(100);

            entity.Property(x => x.ReceiptContentType)
                .HasMaxLength(100);

            entity.Property(x => x.PayerName)
                .HasMaxLength(150);

            entity.Property(x => x.TransferReference)
                .HasMaxLength(100);

            entity.Property(x => x.ReviewedByUserId)
                .HasMaxLength(450);

            entity.HasOne(x => x.Order)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.Authority)
                .IsUnique()
                .HasFilter("[Authority] IS NOT NULL");
        });

        // مدارک اولیه مشتری
        builder.Entity<OrderDocument>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OriginalFileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.StoredFileName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ReviewStatus)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.ReviewNote)
                .HasMaxLength(1000);

            entity.HasOne(x => x.Order)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrderItem)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.OrderId);
            entity.HasIndex(x => x.OrderItemId);
        });

        // فایل‌های ترجمه‌شده و تحویلی
        builder.Entity<OrderDeliveryFile>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TranslatorId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(x => x.OriginalFileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.StoredFileName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrderItem)
                .WithMany(x => x.DeliveryFiles)
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.OrderId);
            entity.HasIndex(x => x.OrderItemId);
            entity.HasIndex(x => x.TranslatorId);
        });

        // اطلاعات اولیه انواع مدارک
        builder.Entity<DocumentType>().HasData(
            new DocumentType
            {
                Id = 1,
                Name = "شناسنامه طرح جدید",
                PricingType = PricingType.Fixed,
                BasePrice = 1800000,
                FreeUnits = 0,
                PricePerExtraUnit = 0,
                ExtraCopyPrice = 700000,
                JudiciaryFee = 600000,
                ForeignAffairsFee = 1000000,
                IsActive = true
            },
            new DocumentType
            {
                Id = 2,
                Name = "ریزنمرات دانشگاهی",
                PricingType = PricingType.PerPage,
                BasePrice = 2500000,
                FreeUnits = 1,
                PricePerExtraUnit = 800000,
                ExtraCopyPrice = 900000,
                JudiciaryFee = 600000,
                ForeignAffairsFee = 2500000,
                IsActive = true
            },
            new DocumentType
            {
                Id = 3,
                Name = "قرارداد",
                PricingType = PricingType.PerLine,
                BasePrice = 2000000,
                FreeUnits = 20,
                PricePerExtraUnit = 45000,
                ExtraCopyPrice = 800000,
                JudiciaryFee = 600000,
                ForeignAffairsFee = 1200000,
                IsActive = true
            }
        );
    }
}
