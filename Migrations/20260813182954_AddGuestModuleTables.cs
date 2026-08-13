using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestModuleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    ContactID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.ContactID);
                });

            migrationBuilder.CreateTable(
                name: "Faqs",
                columns: table => new
                {
                    FaqID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faqs", x => x.FaqID);
                });

            migrationBuilder.CreateTable(
                name: "GalleryImages",
                columns: table => new
                {
                    GalleryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryImages", x => x.GalleryID);
                });

            migrationBuilder.CreateTable(
                name: "GuestOrders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestOrders", x => x.OrderID);
                });

            migrationBuilder.CreateTable(
                name: "GymClasses",
                columns: table => new
                {
                    ClassID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrainerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Days = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeSlot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymClasses", x => x.ClassID);
                });

            migrationBuilder.CreateTable(
                name: "MembershipPlans",
                columns: table => new
                {
                    PlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DurationInMonths = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPlans", x => x.PlanID);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                });

            migrationBuilder.CreateTable(
                name: "Testimonials",
                columns: table => new
                {
                    TestimonialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testimonials", x => x.TestimonialID);
                });

            migrationBuilder.CreateTable(
                name: "TrainerProfiles",
                columns: table => new
                {
                    TrainerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Experience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerProfiles", x => x.TrainerID);
                });

            migrationBuilder.CreateTable(
                name: "GuestOrderItems",
                columns: table => new
                {
                    OrderItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestOrderItems", x => x.OrderItemID);
                    table.ForeignKey(
                        name: "FK_GuestOrderItems_GuestOrders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "GuestOrders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MembershipPlanFeatures",
                columns: table => new
                {
                    FeatureID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPlanFeatures", x => x.FeatureID);
                    table.ForeignKey(
                        name: "FK_MembershipPlanFeatures_MembershipPlans_PlanID",
                        column: x => x.PlanID,
                        principalTable: "MembershipPlans",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemID);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Faqs",
                columns: new[] { "FaqID", "Answer", "DisplayOrder", "IsActive", "Question" },
                values: new object[,]
                {
                    { 1, "Our gym is open 24/7 for premium members. Standard hours are Monday to Saturday from 5:00 AM to 11:00 PM.", 1, true, "What are your gym hours?" },
                    { 2, "No, we provide all state-of-the-art gym equipment, mats, and accessories. Just bring a water bottle and athletic shoes!", 2, true, "Do I need to bring my own equipment?" },
                    { 3, "Yes, you can pause or freeze your active membership for up to 3 months per year through our reception or online dashboard.", 3, true, "Can I freeze my membership?" },
                    { 4, "Yes! We have certified personal trainers available for 1-on-1 sessions included in Standard and Premium plans.", 4, true, "Do you offer personal training?" }
                });

            migrationBuilder.InsertData(
                table: "GalleryImages",
                columns: new[] { "GalleryID", "DisplayOrder", "ImageURL", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, 1, "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=1000&q=80", true, "Heavy Lifting" },
                    { 2, 2, "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?auto=format&fit=crop&w=1000&q=80", true, "Cardio Session" },
                    { 3, 3, "https://images.unsplash.com/photo-1540497077202-7c8a3999166f?auto=format&fit=crop&w=1000&q=80", true, "Modern Equipment" },
                    { 4, 4, "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?auto=format&fit=crop&w=1000&q=80", true, "Personal Training" }
                });

            migrationBuilder.InsertData(
                table: "GymClasses",
                columns: new[] { "ClassID", "ClassName", "Days", "Icon", "ImageUrl", "IsActive", "TimeSlot", "TrainerName" },
                values: new object[,]
                {
                    { 1, "Strength & Lifting", "Mon, Wed, Fri", "dumbbell", "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=1000&q=80", true, "08:00 AM - 10:00 AM", "John Doe" },
                    { 2, "Cardio & Burn", "Tue, Thu, Sat", "activity", "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?auto=format&fit=crop&w=1000&q=80", true, "10:00 AM - 11:30 AM", "Jane Smith" },
                    { 3, "CrossFit Pro", "Mon, Wed, Fri", "target", "https://images.unsplash.com/photo-1540497077202-7c8a3999166f?auto=format&fit=crop&w=1000&q=80", true, "04:00 PM - 06:00 PM", "Mike Johnson" }
                });

            migrationBuilder.InsertData(
                table: "MembershipPlans",
                columns: new[] { "PlanID", "DisplayOrder", "DurationInMonths", "IsActive", "Price", "Title" },
                values: new object[,]
                {
                    { 1, 1, 1, true, 29.99m, "Standard Plan" },
                    { 2, 2, 1, true, 49.99m, "Pro Plan" },
                    { 3, 3, 1, true, 89.99m, "VIP Elite" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductID", "Category", "Description", "IsActive", "Price", "ProductImage", "ProductName", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "Supplements", "Fast absorbing whey protein isolate, 2kg.", true, 59.99m, "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?auto=format&fit=crop&w=500&q=80", "Whey Protein Isolate", 40 },
                    { 2, "Accessories", "700ml leak proof shaker bottle.", true, 12.50m, "https://images.unsplash.com/photo-1544816155-12df9643f363?auto=format&fit=crop&w=500&q=80", "Gym Shaker Bottle", 100 },
                    { 3, "Equipment", "Pair of adjustable dumbbells, 2.5kg - 24kg.", true, 120.00m, "https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?auto=format&fit=crop&w=500&q=80", "Adjustable Dumbbells", 15 },
                    { 4, "Equipment", "Set of 5 resistance bands with carry bag.", true, 24.99m, "https://images.unsplash.com/photo-1598289431512-b97b09177c42?auto=format&fit=crop&w=500&q=80", "Resistance Bands Set", 60 }
                });

            migrationBuilder.InsertData(
                table: "Testimonials",
                columns: new[] { "TestimonialID", "Feedback", "IsApproved", "MemberImage", "MemberName", "Rating" },
                values: new object[,]
                {
                    { 1, "Joining FitZone completely changed my lifestyle. The trainers are certified and professional, and the equipment is top-notch!", true, "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=300&q=80", "Ahmed Mohamed", 5 },
                    { 2, "The atmosphere is extremely motivating. I've lost over 10kg and gained so much strength thanks to the personalized workout plan.", true, "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=300&q=80", "Amina Ali", 5 },
                    { 3, "Best gym in town hands down! Clean facilities, friendly staff, and the supplement store has everything I need after a heavy workout.", true, "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=300&q=80", "Liban Hassan", 5 }
                });

            migrationBuilder.InsertData(
                table: "TrainerProfiles",
                columns: new[] { "TrainerID", "DisplayOrder", "Experience", "ImageUrl", "IsActive", "Role", "TrainerName" },
                values: new object[,]
                {
                    { 1, 1, "8 Years Experience", "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?auto=format&fit=crop&w=600&q=80", true, "Head Strength Coach", "Sarah Jenkins" },
                    { 2, 2, "5 Years Experience", "https://images.unsplash.com/photo-1567013127542-490d757e51fc?auto=format&fit=crop&w=600&q=80", true, "CrossFit & Conditioning", "David Miller" },
                    { 3, 3, "6 Years Experience", "https://images.unsplash.com/photo-1541534741688-6078c6bfb5c5?auto=format&fit=crop&w=600&q=80", true, "Personal Fitness Trainer", "Alex Turner" },
                    { 4, 4, "7 Years Experience", "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?auto=format&fit=crop&w=600&q=80", true, "Yoga & Mobility Specialist", "Jessica Wong" }
                });

            migrationBuilder.InsertData(
                table: "MembershipPlanFeatures",
                columns: new[] { "FeatureID", "Description", "DisplayOrder", "PlanID" },
                values: new object[,]
                {
                    { 1, "Access to gym equipment", 1, 1 },
                    { 2, "Locker room access", 2, 1 },
                    { 3, "1 Free guest pass per month", 3, 1 },
                    { 4, "Standard support", 4, 1 },
                    { 5, "24/7 Gym access", 1, 2 },
                    { 6, "Unlimited guest passes", 2, 2 },
                    { 7, "Free personal trainer (2 sessions)", 3, 2 },
                    { 8, "Nutrition guidance", 4, 2 },
                    { 9, "Priority support", 5, 2 },
                    { 10, "All Pro Plan features", 1, 3 },
                    { 11, "Dedicated 1-on-1 personal trainer", 2, 3 },
                    { 12, "Custom meal & diet plans", 3, 3 },
                    { 13, "VIP lounge & sauna access", 4, 3 },
                    { 14, "Free protein shakes", 5, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductID",
                table: "CartItems",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_SessionId_ProductID",
                table: "CartItems",
                columns: new[] { "SessionId", "ProductID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestOrderItems_OrderID",
                table: "GuestOrderItems",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_GuestOrders_OrderNumber",
                table: "GuestOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlanFeatures_PlanID",
                table: "MembershipPlanFeatures",
                column: "PlanID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropTable(
                name: "Faqs");

            migrationBuilder.DropTable(
                name: "GalleryImages");

            migrationBuilder.DropTable(
                name: "GuestOrderItems");

            migrationBuilder.DropTable(
                name: "GymClasses");

            migrationBuilder.DropTable(
                name: "MembershipPlanFeatures");

            migrationBuilder.DropTable(
                name: "Testimonials");

            migrationBuilder.DropTable(
                name: "TrainerProfiles");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "GuestOrders");

            migrationBuilder.DropTable(
                name: "MembershipPlans");
        }
    }
}
