using GymManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Data
{
    /// <summary>
    /// Xogta bilowga ah (seed data) ee Guest module-ka. Waxaa lagu daraa
    /// migration-ka, sidaa darteed tables-ka iyo xogtooda waxay si automatic
    /// ah u samaysmayaan marka "dotnet ef database update" la ordiyo.
    /// </summary>
    public static class GuestSeedData
    {
        public static void SeedGuestData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MembershipPlan>().HasData(
                new MembershipPlan { PlanId = 1, Name = "Standard Plan", ShortCode = "STD-01", Type = "Standard", DurationDays = 30, Price = 29.99m, Features = "Gym Access|Locker", IsActive = true },
                new MembershipPlan { PlanId = 2, Name = "Pro Plan", ShortCode = "PRO-02", Type = "Pro", DurationDays = 30, Price = 49.99m, Features = "Gym Access|Trainer", IsActive = true },
                new MembershipPlan { PlanId = 3, Name = "VIP Elite", ShortCode = "VIP-03", Type = "VIP", DurationDays = 30, Price = 89.99m, Features = "Full Access|Sauna", IsActive = true }
                );

            

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductID = 1,
                    ProductName = "Whey Protein Isolate",
                    Category = "Supplements",
                    Description = "Fast absorbing whey protein isolate, 2kg.",
                    Price = 59.99m,
                    ProductImage = "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?auto=format&fit=crop&w=500&q=80",
                    StockQuantity = 40,
                    IsActive = true
                },
                new Product
                {
                    ProductID = 2,
                    ProductName = "Gym Shaker Bottle",
                    Category = "Accessories",
                    Description = "700ml leak proof shaker bottle.",
                    Price = 12.50m,
                    ProductImage = "https://images.unsplash.com/photo-1544816155-12df9643f363?auto=format&fit=crop&w=500&q=80",
                    StockQuantity = 100,
                    IsActive = true
                },
                new Product
                {
                    ProductID = 3,
                    ProductName = "Adjustable Dumbbells",
                    Category = "Equipment",
                    Description = "Pair of adjustable dumbbells, 2.5kg - 24kg.",
                    Price = 120.00m,
                    ProductImage = "https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?auto=format&fit=crop&w=500&q=80",
                    StockQuantity = 15,
                    IsActive = true
                },
                new Product
                {
                    ProductID = 4,
                    ProductName = "Resistance Bands Set",
                    Category = "Equipment",
                    Description = "Set of 5 resistance bands with carry bag.",
                    Price = 24.99m,
                    ProductImage = "https://images.unsplash.com/photo-1598289431512-b97b09177c42?auto=format&fit=crop&w=500&q=80",
                    StockQuantity = 60,
                    IsActive = true
                }
            );

            modelBuilder.Entity<GymClass>().HasData(
                new GymClass
                {
                    Id = 1,
                    Name = "Strength & Lifting",
                    Category = "Strength",
                    DurationMinutes = 60,
                    Capacity = 30,
                    TrainerName = "John Doe",
                    Days = "Mon, Wed, Fri",
                    TimeSlot = "08:00 AM - 10:00 AM",
                    IconName = "dumbbell",
                    ImageUrl = "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=1000&q=80",
                    Status = "Active"
                },
new GymClass
{
    Id = 2,
    Name = "Cardio & Burn",
    Category = "Cardio",
    DurationMinutes = 45,
    Capacity = 25,
    TrainerName = "Jane Smith",
    Days = "Tue, Thu, Sat",
    TimeSlot = "10:00 AM - 11:30 AM",
    IconName = "activity",
    ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?auto=format&fit=crop&w=1000&q=80",
    Status = "Active"
},
new GymClass
{
    Id = 3,
    Name = "CrossFit Pro",
    Category = "CrossFit",
    DurationMinutes = 90,
    Capacity = 20,
    TrainerName = "Mike Johnson",
    Days = "Mon, Wed, Fri",
    TimeSlot = "04:00 PM - 06:00 PM",
    IconName = "target",
    ImageUrl = "https://images.unsplash.com/photo-1540497077202-7c8a3999166f?auto=format&fit=crop&w=1000&q=80",
    Status = "Active"
}
            );



            modelBuilder.Entity<GalleryImage>().HasData(
                new GalleryImage { Id = -1, Title = "Heavy Lifting", Category = "Training", ImageUrl = "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=1000&q=80", IsVisible = true, UploadedAt = new DateTime(2026, 1, 1) },
                new GalleryImage { Id = -2, Title = "Cardio Session", Category = "Gym", ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?auto=format&fit=crop&w=1000&q=80", IsVisible = true, UploadedAt = new DateTime(2026, 1, 1) },
                new GalleryImage { Id = -3, Title = "Modern Equipment", Category = "Gym", ImageUrl = "https://images.unsplash.com/photo-1540497077202-7c8a3999166f?auto=format&fit=crop&w=1000&q=80", IsVisible = true, UploadedAt = new DateTime(2026, 1, 1) },
                new GalleryImage { Id = -4, Title = "Personal Training", Category = "Training", ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?auto=format&fit=crop&w=1000&q=80", IsVisible = true, UploadedAt = new DateTime(2026, 1, 1) }
            );

            modelBuilder.Entity<Testimonial>().HasData(
                new Testimonial
                {
                    TestimonialID = 1,
                    MemberName = "Ahmed Mohamed",
                    MemberImage = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=300&q=80",
                    Feedback = "Joining FitZone completely changed my lifestyle. The trainers are certified and professional, and the equipment is top-notch!",
                    Rating = 5,
                    IsApproved = true
                },
                new Testimonial
                {
                    TestimonialID = 2,
                    MemberName = "Amina Ali",
                    MemberImage = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=300&q=80",
                    Feedback = "The atmosphere is extremely motivating. I've lost over 10kg and gained so much strength thanks to the personalized workout plan.",
                    Rating = 5,
                    IsApproved = true
                },
                new Testimonial
                {
                    TestimonialID = 3,
                    MemberName = "Liban Hassan",
                    MemberImage = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=300&q=80",
                    Feedback = "Best gym in town hands down! Clean facilities, friendly staff, and the supplement store has everything I need after a heavy workout.",
                    Rating = 5,
                    IsApproved = true
                }
            );

            modelBuilder.Entity<FaqItem>().HasData(
                new FaqItem
                {
                    FaqID = 1,
                    Question = "What are your gym hours?",
                    Answer = "Our gym is open 24/7 for premium members. Standard hours are Monday to Saturday from 5:00 AM to 11:00 PM.",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new FaqItem
                {
                    FaqID = 2,
                    Question = "Do I need to bring my own equipment?",
                    Answer = "No, we provide all state-of-the-art gym equipment, mats, and accessories. Just bring a water bottle and athletic shoes!",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new FaqItem
                {
                    FaqID = 3,
                    Question = "Can I freeze my membership?",
                    Answer = "Yes, you can pause or freeze your active membership for up to 3 months per year through our reception or online dashboard.",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new FaqItem
                {
                    FaqID = 4,
                    Question = "Do you offer personal training?",
                    Answer = "Yes! We have certified personal trainers available for 1-on-1 sessions included in Standard and Premium plans.",
                    DisplayOrder = 4,
                    IsActive = true
                }
            );
        }
    }
}
