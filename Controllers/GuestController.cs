using System;
using System.Linq;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers
{
    public class GuestController : Controller
    {
        private const decimal ShippingFee = 5.00m;
        private const string CartSessionKey = "GuestCartId";

        private readonly ApplicationDbContext _context;

        public GuestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. STATIC / CATALOG PAGES
        // =========================================================
        public IActionResult HomePage()
        {
            var membershipPlans = _context.MembershipPlans
               .Where(p => p.IsActive)
               .ToList();
            var classes = _context.GymClasses
             .Where(c => c.Status == "Active")
             .ToList();
            var trainers = _context.TrainerProfiles
               .Where(t => t.IsActive)
               .ToList();
            var products = _context.Products
               .Where(p => p.IsActive)
               .OrderBy(p => p.ProductID)
               .ToList();
            var galleryImages = _context.GalleryImages
               .Where(g => g.IsVisible)
               .ToList();

            var testimonials = _context.Testimonials
                .Where(t => t.IsApproved)
                .OrderByDescending(t => t.Rating)
                .ToList();

            var faqs = _context.Faqs
                .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .ToList();

            return View(new
            {
                MembershipPlans = membershipPlans,
                Classes = classes,
                Trainers = trainers,
                Products=products,
                GalleryImages = galleryImages,
                Testimonials = testimonials,
                Faqs = faqs

            });
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Memberships()
        {
            return View(new GuestModel { MembershipPlans = ActivePlans().ToList() });
        }

        public IActionResult Classes()
        {
            var classes = _context.GymClasses
                .Where(c => c.Status == "Active")
                .ToList();

            return View(classes);
        }
        // Badhanka "View Full Schedule" ee bogga Classes
        public IActionResult Schedule()
        {
            return RedirectToAction(nameof(Classes));
        }

        public IActionResult Trainers()
        {
            return View(new GuestModel { Trainers = ActiveTrainers().ToList() });
        }

        public IActionResult Gallery()
        {
            return View(new GuestModel { GalleryImages = ActiveGallery().ToList() });
        }

        public IActionResult Testmonals()
        {
            return View(new GuestModel { Testimonials = ApprovedTestimonials().ToList() });
        }

        public IActionResult Faq()
        {
            return View(new GuestModel
            {
                Faqs = _context.Faqs.Where(f => f.IsActive).OrderBy(f => f.DisplayOrder).ToList()
            });
        }

        public IActionResult Store()
        {
            return View(new GuestModel { Products = ActiveProducts().ToList() });
        }

        // =========================================================
        // 2. CONTACT
        // =========================================================
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitContact(string name, string email, string subject, string message)
        {
            // HUBIN: Guest-ku fariin ma diri karo ilaa uu Login/Register sameeyo
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Fadlan marka hore Login ama Register samee si aad fariin u soo dirtid!";
                return RedirectToAction("Register", "Account");
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Fadlan buuxi dhammaan meelaha loo baahan yahay.";
                return RedirectToAction(nameof(Contact));
            }

            _context.ContactMessages.Add(new ContactMessage
            {
                Name = name.Trim(),
                Email = email.Trim(),
                Subject = string.IsNullOrWhiteSpace(subject) ? "General Inquiry" : subject.Trim(),
                Message = message.Trim(),
                SubmittedAt = DateTime.Now,
                IsRead = false
            });

            _context.Notifications.Add(new Notification
            {
                TargetRole = "Receptionist",
                Title = "New Contact Message",
                Message = $"A new message has been received from {name.Trim()} regarding '{subject ?? "General Inquiry"}'.",
                Type = "Contact",
                IsRead = false,
                Timestamp = DateTime.Now
            });

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Fariintaada waa la helay. Waan kula soo xidhiidhi doonaa dhawaan!";
            return RedirectToAction(nameof(Contact));
        }
        // =========================================================
        // 3. SHOPPING CART
        // =========================================================
        public IActionResult Cart()
        {
            return View(BuildCartModel());
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            // HUBIN: Guest-ku Cart wax uga dari karo ilaa uu Register/Login sameeyo
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Fadlan marka hore Register ama Login samee si aad alaabta gaadhiga ugu dartid!";
                return RedirectToAction("Register", "Account");
            }

            var product = _context.Products.FirstOrDefault(p => p.ProductID == id && p.IsActive);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Alaabtan lama helin.";
                return RedirectToAction(nameof(Store));
            }

            if (quantity < 1) quantity = 1;

            var cartId = GetCartId();
            var existing = _context.CartItems.FirstOrDefault(c => c.SessionId == cartId && c.ProductID == id);

            if (existing == null)
            {
                _context.CartItems.Add(new CartItem
                {
                    SessionId = cartId,
                    ProductID = id,
                    Quantity = quantity,
                    AddedAt = DateTime.Now
                });
            }
            else
            {
                existing.Quantity += quantity;
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = $"{product.ProductName} waa lagu daray gaadhiga.";
            return RedirectToAction(nameof(Cart));
        }
        public IActionResult IncreaseQuantity(int id)
        {
            var item = FindCartItem(id);
            if (item != null)
            {
                item.Quantity++;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Cart));
        }

        public IActionResult DecreaseQuantity(int id)
        {
            var item = FindCartItem(id);
            if (item != null)
            {
                if (item.Quantity <= 1)
                {
                    _context.CartItems.Remove(item);
                }
                else
                {
                    item.Quantity--;
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Cart));
        }

        public IActionResult RemoveItem(int id)
        {
            var item = FindCartItem(id);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Cart));
        }

        public IActionResult ClearCart()
        {
            var cartId = GetCartId();
            var items = _context.CartItems.Where(c => c.SessionId == cartId).ToList();
            if (items.Count > 0)
            {
                _context.CartItems.RemoveRange(items);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Cart));
        }

        // =========================================================
        // 4. CHECKOUT & ORDERS
        // =========================================================
        public IActionResult Checkout()
        {
            var model = BuildCartModel();
            if (model.IsCartEmpty)
            {
                TempData["ErrorMessage"] = "Gaadhigaagu waa madhan yahay.";
                return RedirectToAction(nameof(Cart));
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult PlaceOrder(string fullName, string phone, string address, string paymentMethod)
        {
            var cartId = GetCartId();
            var items = LoadCartItems(cartId);

            if (items.Count == 0)
            {
                TempData["ErrorMessage"] = "Gaadhigaagu waa madhan yahay.";
                return RedirectToAction(nameof(Cart));
            }

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(address))
            {
                TempData["AlertMessage"] = "Fadlan buuxi macluumaadka gaadiidka (magac, telefoon, cinwaan).";
                return RedirectToAction(nameof(Checkout));
            }

            var subtotal = items.Sum(i => i.TotalPrice);
            var order = new GuestOrder
            {
                OrderNumber = $"ORD-{DateTime.Now:yyyyMMddHHmmss}",
                SessionId = cartId,
                FullName = fullName.Trim(),
                Phone = phone.Trim(),
                Address = address.Trim(),
                PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "Cash" : paymentMethod,
                Subtotal = subtotal,
                ShippingFee = ShippingFee,
                TotalAmount = subtotal + ShippingFee,
                Status = "Pending",
                OrderDate = DateTime.Now,
                Items = items.Select(i => new GuestOrderItem
                {
                    ProductID = i.ProductID,
                    ProductName = i.ProductName,
                    UnitPrice = i.Price,
                    Quantity = i.Quantity,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };

            _context.GuestOrders.Add(order);
            _context.CartItems.RemoveRange(items);
            _context.Notifications.Add(new Notification
            {
                TargetRole = "Receptionist",
                Title = "New Order Received",
                Message = $"A new store order ({order.OrderNumber}) has been placed by {order.FullName} totaling ${order.TotalAmount:N2}.",
                Type = "Order",
                IsRead = false,
                Timestamp = DateTime.Now
            });
            _context.SaveChanges();

            return RedirectToAction(nameof(OrderConfirmation), new { orderNumber = order.OrderNumber });
        }

        public IActionResult OrderConfirmation(string orderNumber)
        {
            var order = _context.GuestOrders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.OrderNumber == orderNumber);

            if (order == null)
            {
                return RedirectToAction(nameof(Store));
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitMembershipRequest(string planName, string paymentMethod)
        {
            var loggedInUser = User.Identity?.Name;

            if (string.IsNullOrEmpty(loggedInUser))
            {
                TempData["ErrorMessage"] = "Fadlan marka hore Login ama Register samee!";
                return RedirectToAction("Register", "Account");
            }

            if (string.IsNullOrWhiteSpace(planName))
            {
                TempData["ErrorMessage"] = "Fadlan dooro qorshe sax ah.";
                return RedirectToAction(nameof(Memberships));
            }

            var userProfile = _context.UserProfiles.FirstOrDefault(u =>
                u.Email == loggedInUser ||
                u.Username == loggedInUser ||
                u.Name == loggedInUser);

            if (userProfile != null)
            {
                userProfile.MembershipType = planName.Trim();
                userProfile.Status = "Pending";
                userProfile.JoinDate = DateTime.Now;

                _context.Notifications.Add(new Notification
                {
                    TargetRole = "Receptionist",
                    Title = "Membership Request Pending",
                    Message = $"A new membership request has been submitted by {userProfile.Name} ({userProfile.Id}) for plan {planName}.",
                    Type = "Member",
                    IsRead = false,
                    Timestamp = DateTime.Now
                });
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Your application has been submitted successfully! Please wait while your request is reviewed and approved.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Koontada loogu talagalay '{loggedInUser}' lagama helin database-ka.";
                return RedirectToAction(nameof(Memberships));
            }

            return RedirectToAction(nameof(Memberships));
        }
        // =========================================================
        // HELPERS
        // =========================================================
        private IQueryable<MembershipPlan> ActivePlans() =>
            _context.MembershipPlans
                .Where(p => p.IsActive);

        private IQueryable<Product> ActiveProducts() =>
            _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductID);

        private IQueryable<GymClass> ActiveClasses() =>
            _context.GymClasses.Where(c => c.Status == "Active").OrderBy(c => c.Id);

        private IQueryable<TrainerProfile> ActiveTrainers() =>
            _context.TrainerProfiles.Where(t => t.IsActive);

        private IQueryable<GalleryImage> ActiveGallery() =>
            _context.GalleryImages.Where(g => g.IsVisible);

        private IQueryable<Testimonial> ApprovedTestimonials() =>
            _context.Testimonials.Where(t => t.IsApproved).OrderBy(t => t.TestimonialID);

        private string GetCartId()
        {
            var cartId = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString("N");
                HttpContext.Session.SetString(CartSessionKey, cartId);
            }

            return cartId;
        }

        private List<CartItem> LoadCartItems(string cartId) =>
            _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.SessionId == cartId)
                .OrderBy(c => c.CartItemID)
                .ToList();

        private CartItem? FindCartItem(int productId)
        {
            var cartId = GetCartId();
            return _context.CartItems.FirstOrDefault(c => c.SessionId == cartId && c.ProductID == productId);
        }

        private GuestModel BuildCartModel()
        {
            var items = LoadCartItems(GetCartId());
            return new GuestModel
            {
                CartItems = items,
                ShippingFee = items.Count == 0 ? 0m : ShippingFee
            };
        }
    }
}
