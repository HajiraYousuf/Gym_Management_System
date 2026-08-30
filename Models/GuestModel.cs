using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagementSystem.Models
{
    public class GuestModel
    {
        public List<MembershipPlan> MembershipPlans { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public List<TrainerProfile> Trainers { get; set; } = new();
        public List<GalleryImage> GalleryImages { get; set; } = new();
        public List<Testimonial> Testimonials { get; set; } = new();
        public List<FaqItem> Faqs { get; set; } = new();
        public List<CartItem> CartItems { get; set; } = new();

        public decimal ShippingFee { get; set; }
        public decimal Subtotal => CartItems.Sum(i => i.TotalPrice);
        public decimal TotalDue => Subtotal + ShippingFee;
        public int TotalQuantity => CartItems.Sum(i => i.Quantity);
        public bool IsCartEmpty => CartItems.Count == 0;
    }

    public class MembershipPlanFeature
    {
        [Key]
        public int FeatureID { get; set; }
        public int PlanID { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        [ForeignKey(nameof(PlanID))]
        public MembershipPlan? Plan { get; set; }
    }

    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ProductImage { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class TrainerProfile
    {
        [Key]
        public int TrainerID { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Testimonial
    {
        [Key]
        public int TestimonialID { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string MemberImage { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public bool IsApproved { get; set; } = true;
    }

    public class FaqItem
    {
        [Key]
        public int FaqID { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ContactMessage
    {
        [Key]
        public int ContactID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }

        public string SessionId { get; set; } = string.Empty;
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }

        [ForeignKey(nameof(ProductID))]
        public Product? Product { get; set; }

        [NotMapped]
        public string ProductName => Product?.ProductName ?? string.Empty;

        [NotMapped]
        public string ProductImage => Product?.ProductImage ?? string.Empty;

        [NotMapped]
        public string Category => Product?.Category ?? string.Empty;

        [NotMapped]
        public decimal Price => Product?.Price ?? 0m;

        [NotMapped]
        public decimal TotalPrice => Price * Quantity;
    }

    public class GuestOrder
    {
        [Key]
        public int OrderID { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; }

        public List<GuestOrderItem> Items { get; set; } = new();
    }

    public class GuestOrderItem
    {
        [Key]
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        [ForeignKey(nameof(OrderID))]
        public GuestOrder? Order { get; set; }
    }
}