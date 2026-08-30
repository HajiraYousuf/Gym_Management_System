using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using static GymManagementSystem.Models.Payment;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ClassSchedule> ClassSchedules { get; set; }
    public DbSet<Visitor> Visitors { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<GymClass> GymClasses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<GymReservation> GymReservations { get; set; }
    public DbSet<MemberProgress> MemberProgresses { get; set; }
    public DbSet<NutritionPlan> NutritionPlans { get; set; }
    public DbSet<WorkoutPlan> WorkoutPlans { get; set; }

    // =========================================================
    // GUEST MODULE
    // =========================================================
    public DbSet<MembershipPlan> MembershipPlans { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<TrainerProfile> TrainerProfiles { get; set; }
    public DbSet<GalleryImage> GalleryImages { get; set; }
    public DbSet<Testimonial> Testimonials { get; set; }
    public DbSet<FaqItem> Faqs { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<GuestOrder> GuestOrders { get; set; }
    public DbSet<GuestOrderItem> GuestOrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserProfile Configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.Property(p => p.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(p => p.Email).IsUnique();
            entity.Property(p => p.Salary).HasColumnType("decimal(18,2)");
        });

        // Precision configurations
        modelBuilder.Entity<MembershipPlan>().Property(p => p.Price).HasPrecision(18, 2);
        modelBuilder.Entity<MembershipPlan>().Property(p => p.SignupFee).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);

        // --- XIRIIRADA (RELATIONSHIPS & FOREIGN KEYS) ---

        // 1. MemberProgress -> UserProfile (MemberId)
        // 1. MemberProgress -> UserProfile (MemberId)
        modelBuilder.Entity<MemberProgress>(entity =>
        {
            entity.HasOne(mp => mp.Member)
                  .WithMany()
                  .HasForeignKey(mp => mp.MemberId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Precision for decimals in MemberProgress
            entity.Property(p => p.Weight).HasColumnType("decimal(18,2)");
            entity.Property(p => p.BodyFat).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Chest).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Waist).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Arms).HasColumnType("decimal(18,2)");
            entity.Property(p => p.TargetWeight).HasColumnType("decimal(18,2)");
        });

        // 2. NutritionPlan -> UserProfile (AssignedMemberId)
        modelBuilder.Entity<NutritionPlan>(entity =>
        {
            entity.HasOne(np => np.AssignedMember)
                  .WithMany()
                  .HasForeignKey(np => np.AssignedMemberId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 3. WorkoutPlan -> UserProfile (AssignedMemberId)
        modelBuilder.Entity<WorkoutPlan>(entity =>
        {
            entity.HasOne(wp => wp.AssignedMember)
                  .WithMany()
                  .HasForeignKey(wp => wp.AssignedMemberId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 4. Message -> Users (SenderId & ReceiverId)
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(m => m.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(m => m.ReceiverId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 5. AttendanceRecord -> UserProfile (MemberId)
        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasOne(ar => ar.Member)
                  .WithMany()
                  .HasForeignKey(ar => ar.MemberId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 6. MembershipPlanFeature -> MembershipPlan (PlanID)
        modelBuilder.Entity<MembershipPlanFeature>(entity =>
        {
            entity.HasOne(f => f.Plan)
                  .WithMany()
                  .HasForeignKey(f => f.PlanID)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Guest Orders & Order Items
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