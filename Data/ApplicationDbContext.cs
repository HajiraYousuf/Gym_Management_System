using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    // =========================================================
    // GUEST MODULE
    // =========================================================
    public DbSet<MembershipPlan> MembershipPlans { get; set; }
    public DbSet<MembershipPlanFeature> MembershipPlanFeatures { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<GymClass> GymClasses { get; set; }
    public DbSet<TrainerProfile> TrainerProfiles { get; set; }
    public DbSet<GalleryImage> GalleryImages { get; set; }
    public DbSet<Testimonial> Testimonials { get; set; }
    public DbSet<FaqItem> Faqs { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<GuestOrder> GuestOrders { get; set; }
    public DbSet<GuestOrderItem> GuestOrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MembershipPlan>().Property(p => p.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);

        modelBuilder.Entity<GuestOrder>(entity =>
        {
            entity.Property(o => o.Subtotal).HasPrecision(18, 2);
            entity.Property(o => o.ShippingFee).HasPrecision(18, 2);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.Property(o => o.OrderNumber).HasMaxLength(50);
            entity.Property(o => o.SessionId).HasMaxLength(100);
            entity.HasIndex(o => o.OrderNumber).IsUnique();
        });

        modelBuilder.Entity<GuestOrderItem>(entity =>
        {
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.Property(i => i.TotalPrice).HasPrecision(18, 2);
            entity.HasOne(i => i.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(i => i.OrderID)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MembershipPlanFeature>()
            .HasOne(f => f.Plan)
            .WithMany(p => p.Features)
            .HasForeignKey(f => f.PlanID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.Property(c => c.SessionId).HasMaxLength(100);
            entity.HasIndex(c => new { c.SessionId, c.ProductID }).IsUnique();
            entity.HasOne(c => c.Product)
                  .WithMany()
                  .HasForeignKey(c => c.ProductID)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.SeedGuestData();
    }
}
