using System.Security.Claims;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        AppDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager)
    {
        // Automatically apply any pending migrations
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }

        // If database is already initialized, verify and seed any missing categories and products
        if (await context.Categories.IgnoreQueryFilters().AnyAsync())
        {
            await EnsureComprehensiveCatalogSeededAsync(context);
            await EnsureUniqueProductImagesAsync(context);
            return;
        }

        // 1. ROLES & PERMISSIONS
        var superAdminRole = new AppRole("SuperAdmin", "Full unrestricted administrative access");
        var managerRole = new AppRole("Manager", "Store operations management");
        var customerRole = new AppRole("Customer", "Standard registered shopper");

        await roleManager.CreateAsync(superAdminRole);
        await roleManager.CreateAsync(managerRole);
        await roleManager.CreateAsync(customerRole);

        // Assign Permissions as Role Claims & RolePermission entities
        foreach (var permission in Permissions.All)
        {
            await roleManager.AddClaimAsync(superAdminRole, new Claim("permission", permission));
            await context.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = superAdminRole.Id,
                Permission = permission
            });
        }

        foreach (var permission in Permissions.ManagerPermissions)
        {
            await roleManager.AddClaimAsync(managerRole, new Claim("permission", permission));
            await context.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = managerRole.Id,
                Permission = permission
            });
        }

        // 2. USERS
        var adminUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin@test.com",
            Email = "admin@test.com",
            FirstName = "Super",
            LastName = "Admin",
            EmailConfirmed = true,
            IsActive = true
        };
        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, "SuperAdmin");

        var managerUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "manager@test.com",
            Email = "manager@test.com",
            FirstName = "Store",
            LastName = "Manager",
            EmailConfirmed = true,
            IsActive = true
        };
        await userManager.CreateAsync(managerUser, "Manager123!");
        await userManager.AddToRoleAsync(managerUser, "Manager");

        var customers = new List<AppUser>();
        for (int i = 1; i <= 5; i++)
        {
            var customer = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = $"customer{i}@test.com",
                Email = $"customer{i}@test.com",
                FirstName = $"Customer",
                LastName = $"{i}",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(customer, "Customer123!");
            await userManager.AddToRoleAsync(customer, "Customer");
            customers.Add(customer);
        }

        // 3. BRANDS
        var brands = new List<Brand>
        {
            new() { Id = Guid.NewGuid(), Name = "Apple", Slug = "apple", Description = "Innovative consumer tech and hardware.", LogoUrl = "/images/brands/apple.png" },
            new() { Id = Guid.NewGuid(), Name = "Samsung", Slug = "samsung", Description = "Global leader in digital electronics.", LogoUrl = "/images/brands/samsung.png" },
            new() { Id = Guid.NewGuid(), Name = "Nike", Slug = "nike", Description = "Premier athletic footwear and apparel.", LogoUrl = "/images/brands/nike.png" },
            new() { Id = Guid.NewGuid(), Name = "Adidas", Slug = "adidas", Description = "Iconic sportswear, shoes and streetwear.", LogoUrl = "/images/brands/adidas.png" },
            new() { Id = Guid.NewGuid(), Name = "Sony", Slug = "sony", Description = "Cutting-edge audio and gaming entertainment.", LogoUrl = "/images/brands/sony.png" }
        };
        await context.Brands.AddRangeAsync(brands);

        // 4. TAGS
        var tags = new List<Tag>
        {
            new() { Id = Guid.NewGuid(), Name = "Trending", Slug = "trending" },
            new() { Id = Guid.NewGuid(), Name = "Sale", Slug = "sale" },
            new() { Id = Guid.NewGuid(), Name = "Eco Friendly", Slug = "eco-friendly" },
            new() { Id = Guid.NewGuid(), Name = "Summer 2026", Slug = "summer-2026" },
            new() { Id = Guid.NewGuid(), Name = "New Arrival", Slug = "new-arrival" },
            new() { Id = Guid.NewGuid(), Name = "Premium", Slug = "premium" },
            new() { Id = Guid.NewGuid(), Name = "Casual", Slug = "casual" },
            new() { Id = Guid.NewGuid(), Name = "Sport", Slug = "sport" }
        };
        await context.Tags.AddRangeAsync(tags);

        // 5. CATEGORIES (Parent-Child Hierarchy + 1 Soft-deleted category)
        var catElectronics = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Slug = "electronics",
            Description = "Gadgets, smartphones, and computers",
            DisplayOrder = 1
        };
        var catSmartphones = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Smartphones",
            Slug = "smartphones",
            Description = "Flagship and budget smartphones",
            ParentCategoryId = catElectronics.Id,
            DisplayOrder = 1
        };
        var catLaptops = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Laptops & Computers",
            Slug = "laptops-computers",
            Description = "High performance laptops and accessories",
            ParentCategoryId = catElectronics.Id,
            DisplayOrder = 2
        };
        var catAudio = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Audio & Headphones",
            Slug = "audio-headphones",
            Description = "Wireless headphones and audio speakers",
            ParentCategoryId = catElectronics.Id,
            DisplayOrder = 3
        };

        var catFashion = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Fashion & Apparel",
            Slug = "fashion-apparel",
            Description = "Trending streetwear and sports clothing",
            DisplayOrder = 2
        };
        var catMensClothing = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Men's Clothing",
            Slug = "mens-clothing",
            Description = "Shirts, jackets, and sportswear for men",
            ParentCategoryId = catFashion.Id,
            DisplayOrder = 1
        };
        var catWomensClothing = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Women's Clothing",
            Slug = "womens-clothing",
            Description = "Dresses, activewear, and shoes for women",
            ParentCategoryId = catFashion.Id,
            DisplayOrder = 2
        };

        var catHome = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Home & Living",
            Slug = "home-living",
            Description = "Modern home accessories and furniture",
            DisplayOrder = 3
        };

        // Soft-deleted Category (for testing Recycle Bin / Trash endpoints)
        var catArchived = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Seasonal Clearance 2025",
            Slug = "seasonal-clearance-2025",
            Description = "Old seasonal collection archive",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-10),
            DeletedBy = "admin@test.com",
            DisplayOrder = 99
        };

        await context.Categories.AddRangeAsync(
            catElectronics, catSmartphones, catLaptops, catAudio,
            catFashion, catMensClothing, catWomensClothing,
            catHome, catArchived);

        // 6. SHIPPING METHODS
        var shippingMethods = new List<ShippingMethod>
        {
            new() { Id = Guid.NewGuid(), Name = "Standard Shipping", Description = "Delivered via postal freight", Price = 5.00m, EstimatedDeliveryDays = "3-5 business days" },
            new() { Id = Guid.NewGuid(), Name = "Express Delivery", Description = "Fast courier door-to-door delivery", Price = 15.00m, EstimatedDeliveryDays = "1-2 business days" },
            new() { Id = Guid.NewGuid(), Name = "Next Day Priority", Description = "Guaranteed delivery by next morning", Price = 25.00m, EstimatedDeliveryDays = "Next business day" }
        };
        await context.ShippingMethods.AddRangeAsync(shippingMethods);

        // 7. COUPONS (Active, Expired, Limit Reached, Inactive)
        var coupons = new List<Coupon>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Code = "SUMMER20",
                Description = "20% off summer promo",
                CouponType = CouponType.Percentage,
                DiscountValue = 20.00m,
                MinOrderAmount = 50.00m,
                MaxDiscountAmount = 100.00m,
                StartDate = DateTime.UtcNow.AddDays(-30),
                ExpiryDate = DateTime.UtcNow.AddDays(60),
                UsageLimit = 500,
                TimesUsed = 25,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "WELCOME10",
                Description = "Fixed $10 discount on first purchase",
                CouponType = CouponType.FixedAmount,
                DiscountValue = 10.00m,
                MinOrderAmount = 30.00m,
                StartDate = DateTime.UtcNow.AddDays(-10),
                ExpiryDate = DateTime.UtcNow.AddDays(90),
                UsageLimit = 1000,
                TimesUsed = 15,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "EXPIRED50",
                Description = "Expired flash discount",
                CouponType = CouponType.Percentage,
                DiscountValue = 50.00m,
                StartDate = DateTime.UtcNow.AddDays(-60),
                ExpiryDate = DateTime.UtcNow.AddDays(-5),
                UsageLimit = 100,
                TimesUsed = 40,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "MAXEDOUT",
                Description = "Promotional code reached its max usage cap",
                CouponType = CouponType.FixedAmount,
                DiscountValue = 15.00m,
                StartDate = DateTime.UtcNow.AddDays(-20),
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                UsageLimit = 10,
                TimesUsed = 10,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "BLACKFRIDAY",
                Description = "Inactive upcoming promo code",
                CouponType = CouponType.Percentage,
                DiscountValue = 30.00m,
                StartDate = DateTime.UtcNow.AddDays(40),
                ExpiryDate = DateTime.UtcNow.AddDays(45),
                UsageLimit = 200,
                TimesUsed = 0,
                IsActive = false
            }
        };
        await context.Coupons.AddRangeAsync(coupons);

        // 8. BANNERS
        var banners = new List<Banner>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Flagship Tech Collection 2026",
                Subtitle = "Upgrade to the latest smartphones and laptops today with free shipping.",
                ImageUrl = "/images/banners/hero-tech.jpg",
                TargetUrl = "/category/electronics",
                Position = BannerPosition.Hero,
                DisplayOrder = 1,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Summer Streetwear Drop",
                Subtitle = "Fresh looks, breathable fabrics and trending casual sneakers.",
                ImageUrl = "/images/banners/hero-fashion.jpg",
                TargetUrl = "/category/fashion-apparel",
                Position = BannerPosition.Hero,
                DisplayOrder = 2,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Audio Excellence Experience",
                Subtitle = "High fidelity noise-cancelling headphones up to 25% off.",
                ImageUrl = "/images/banners/subhero-audio.jpg",
                TargetUrl = "/category/audio-headphones",
                Position = BannerPosition.SubHero,
                DisplayOrder = 1,
                IsActive = true
            }
        };
        await context.Banners.AddRangeAsync(banners);

        // 9. 30+ PRODUCTS WITH VARIANTS, IMAGES, TAGS & STOCK
        var products = new List<Product>();
        var productVariants = new List<ProductVariant>();
        var productImages = new List<ProductImage>();
        var productTagEntities = new List<ProductTag>();
        var stockMovements = new List<StockMovement>();

        var rawProductCatalog = new[]
        {
            // Smartphones (Apple / Samsung)
            ("iPhone 15 Pro", "iphone-15-pro", "IPH15P", "Titanium design with A17 Pro chip and advanced camera system.", 999.00m, 1099.00m, catSmartphones.Id, (Guid?)brands[0].Id, true, 4.8, 42,
                new[] { ("Natural Titanium / 128GB", "Natural", "128GB", 999.00m, 25, 5), ("Black Titanium / 256GB", "Black", "256GB", 1099.00m, 18, 5) }),
            ("iPhone 15", "iphone-15", "IPH15", "Dynamic Island, 48MP main camera, and USB-C connectivity.", 799.00m, 899.00m, catSmartphones.Id, brands[0].Id, false, 4.6, 28,
                new[] { ("Blue / 128GB", "Blue", "128GB", 799.00m, 30, 5), ("Pink / 256GB", "Pink", "256GB", 899.00m, 12, 5) }),
            ("Galaxy S24 Ultra", "galaxy-s24-ultra", "S24U", "AI-powered smartphone with 200MP camera and built-in S Pen.", 1199.00m, 1299.00m, catSmartphones.Id, brands[1].Id, true, 4.9, 65,
                new[] { ("Titanium Gray / 256GB", "Gray", "256GB", 1199.00m, 22, 5), ("Titanium Violet / 512GB", "Violet", "512GB", 1319.00m, 4, 5) }), // Low stock!
            ("Galaxy S24+", "galaxy-s24-plus", "S24P", "QHD+ display, versatile cameras, and all-day intelligent battery.", 999.00m, 1049.00m, catSmartphones.Id, brands[1].Id, false, 4.5, 19,
                new[] { ("Onyx Black / 256GB", "Black", "256GB", 999.00m, 15, 5), ("Cobalt Violet / 512GB", "Violet", "512GB", 1119.00m, 8, 5) }),
            ("Galaxy Z Fold 5", "galaxy-z-fold-5", "ZFL5", "Transformative 7.6-inch foldable display for multitasking.", 1799.00m, 1899.00m, catSmartphones.Id, brands[1].Id, true, 4.7, 34,
                new[] { ("Phantom Black / 512GB", "Black", "512GB", 1799.00m, 6, 5), ("Icy Blue / 1TB", "Blue", "1TB", 2099.00m, 3, 5) }), // Low stock!

            // Laptops & Computers
            ("MacBook Pro 16", "macbook-pro-16", "MBP16", "M3 Max beast with Liquid Retina XDR screen and supreme battery life.", 2499.00m, 2699.00m, catLaptops.Id, brands[0].Id, true, 4.9, 82,
                new[] { ("Space Black / 36GB / 512GB", "Black", "512GB", 2499.00m, 10, 5), ("Silver / 36GB / 1TB", "Silver", "1TB", 2699.00m, 7, 5) }),
            ("MacBook Air 15", "macbook-air-15", "MBA15", "Incredibly thin, lightweight M3 powerhouse for work and study.", 1299.00m, 1399.00m, catLaptops.Id, brands[0].Id, false, 4.8, 47,
                new[] { ("Midnight / 16GB / 256GB", "Midnight", "256GB", 1299.00m, 20, 5), ("Starlight / 16GB / 512GB", "Starlight", "512GB", 1499.00m, 14, 5) }),
            ("Galaxy Book4 Pro", "galaxy-book4-pro", "GBP4", "Dynamic AMOLED 2X touchscreen and Intel Core Ultra 7 processor.", 1449.00m, 1599.00m, catLaptops.Id, brands[1].Id, false, 4.4, 15,
                new[] { ("Moonstone Gray / 16GB / 512GB", "Gray", "512GB", 1449.00m, 11, 5), ("Platinum Silver / 32GB / 1TB", "Silver", "1TB", 1749.00m, 5, 5) }), // Low stock!
            ("Galaxy Book4 360", "galaxy-book4-360", "GB4-360", "2-in-1 convertible touchscreen with bundled S Pen support.", 1199.00m, 1299.00m, catLaptops.Id, brands[1].Id, false, 4.3, 11,
                new[] { ("Gray / 16GB / 512GB", "Gray", "512GB", 1199.00m, 9, 5) }),

            // Audio & Headphones
            ("Sony WH-1000XM5", "sony-wh-1000xm5", "SNXM5", "Industry-leading active noise cancellation with 30-hour battery.", 399.00m, 429.00m, catAudio.Id, brands[4].Id, true, 4.9, 120,
                new[] { ("Silver / One Size", "Silver", "Standard", 399.00m, 35, 5), ("Black / One Size", "Black", "Standard", 399.00m, 40, 5) }),
            ("Sony WF-1000XM5", "sony-wf-1000xm5", "SNWFXM5", "Compact noise canceling true wireless in-ear earbuds.", 299.00m, 319.00m, catAudio.Id, brands[4].Id, false, 4.7, 54,
                new[] { ("Black / Standard", "Black", "Standard", 299.00m, 28, 5), ("Silver / Standard", "Silver", "Standard", 299.00m, 20, 5) }),
            ("AirPods Max", "airpods-max", "APMAX", "Computational audio with high-fidelity sound and memory foam ear cushions.", 549.00m, 599.00m, catAudio.Id, brands[0].Id, true, 4.6, 76,
                new[] { ("Space Gray / Standard", "Gray", "Standard", 549.00m, 15, 5), ("Sky Blue / Standard", "Blue", "Standard", 549.00m, 8, 5) }),
            ("AirPods Pro 2", "airpods-pro-2", "APP2", "Active Noise Cancellation, Adaptive Audio, and USB-C MagSafe case.", 249.00m, 269.00m, catAudio.Id, brands[0].Id, true, 4.8, 140,
                new[] { ("White / USB-C", "White", "Standard", 249.00m, 50, 5) }),
            ("Galaxy Buds2 Pro", "galaxy-buds2-pro", "GBP2", "24-bit Hi-Fi sound and intelligent ANC in ergonomic design.", 199.00m, 229.00m, catAudio.Id, brands[1].Id, false, 4.5, 38,
                new[] { ("Graphite / Standard", "Graphite", "Standard", 199.00m, 25, 5), ("Bora Purple / Standard", "Purple", "Standard", 199.00m, 14, 5) }),

            // Men's Clothing (Nike / Adidas)
            ("Nike Tech Fleece Windrunner", "nike-tech-fleece-windrunner", "NTF-WR", "Lightweight warmth with full-zip hoodie and signature taped pockets.", 140.00m, 155.00m, catMensClothing.Id, brands[2].Id, true, 4.8, 95,
                new[] { ("Black / M", "Black", "M", 140.00m, 25, 5), ("Black / L", "Black", "L", 140.00m, 20, 5), ("Heather Gray / L", "Gray", "L", 140.00m, 5, 5) }), // Low stock!
            ("Nike Tech Fleece Joggers", "nike-tech-fleece-joggers", "NTF-JG", "Tailored fit fleece track pants with zip utility pocket.", 120.00m, 130.00m, catMensClothing.Id, brands[2].Id, false, 4.7, 83,
                new[] { ("Black / M", "Black", "M", 120.00m, 30, 5), ("Dark Gray / L", "Gray", "L", 120.00m, 18, 5) }),
            ("Nike Air Force 1 07", "nike-air-force-1-07", "NAF1", "Legendary basketball sneakers with crisp leather overlays.", 115.00m, 125.00m, catMensClothing.Id, brands[2].Id, true, 4.9, 210,
                new[] { ("Triple White / US 10", "White", "US 10", 115.00m, 45, 5), ("Triple White / US 11", "White", "US 11", 115.00m, 35, 5), ("Black / US 10", "Black", "US 10", 115.00m, 14, 5) }),
            ("Nike Pegasus 40", "nike-pegasus-40", "NPEG40", "Springy running shoes with dual Zoom Air units and engineered mesh.", 130.00m, 140.00m, catMensClothing.Id, brands[2].Id, false, 4.6, 52,
                new[] { ("Blue / US 10", "Blue", "US 10", 130.00m, 22, 5), ("Black / US 11", "Black", "US 11", 130.00m, 16, 5) }),
            ("Adidas Originals Trefoil Hoodie", "adidas-originals-trefoil-hoodie", "AD-TRF", "Classic heavyweight cotton fleece with iconic trefoil chest logo.", 75.00m, 85.00m, catMensClothing.Id, brands[3].Id, false, 4.6, 67,
                new[] { ("Black / M", "Black", "M", 75.00m, 24, 5), ("Black / L", "Black", "L", 75.00m, 18, 5), ("Night Indigo / XL", "Indigo", "XL", 75.00m, 9, 5) }),
            ("Adidas Ultraboost Light", "adidas-ultraboost-light", "AD-UBL", "Next-generation energy return cushioning for marathon runners.", 190.00m, 210.00m, catMensClothing.Id, brands[3].Id, true, 4.8, 88,
                new[] { ("Core Black / US 10", "Black", "US 10", 190.00m, 20, 5), ("Cloud White / US 10.5", "White", "US 10.5", 190.00m, 15, 5) }),
            ("Adidas Samba Classic", "adidas-samba-classic", "AD-SMB", "Heritage indoor soccer low-top leather shoes.", 100.00m, 110.00m, catMensClothing.Id, brands[3].Id, true, 4.9, 175,
                new[] { ("Core Black-White / US 9.5", "Black/White", "US 9.5", 100.00m, 12, 5), ("Core Black-White / US 10", "Black/White", "US 10", 100.00m, 3, 5) }), // Low stock!

            // Women's Clothing
            ("Nike Dri-FIT One High-Rise Leggings", "nike-dri-fit-one-leggings", "NDF-LG", "Buttery-soft squat-proof leggings with moisture wicking fabric.", 65.00m, 75.00m, catWomensClothing.Id, brands[2].Id, true, 4.8, 110,
                new[] { ("Black / S", "Black", "S", 65.00m, 30, 5), ("Black / M", "Black", "M", 65.00m, 25, 5), ("Burgundy / S", "Burgundy", "S", 65.00m, 12, 5) }),
            ("Nike Phoenix Fleece Oversized Hoodie", "nike-phoenix-fleece-hoodie", "NPH-HD", "Extra roomy dropped-shoulder fleece pullover.", 80.00m, 90.00m, catWomensClothing.Id, brands[2].Id, false, 4.7, 59,
                new[] { ("Sail / S", "Sail", "S", 80.00m, 18, 5), ("Black / M", "Black", "M", 80.00m, 15, 5) }),
            ("Nike Invincible 3 Running Shoes", "nike-invincible-3-shoes", "NINV3", "Maximum cushioning with ultra-responsive ZoomX foam soles.", 180.00m, 195.00m, catWomensClothing.Id, brands[2].Id, false, 4.7, 41,
                new[] { ("White-Pink / US 7.5", "White/Pink", "US 7.5", 180.00m, 14, 5), ("Black-Gold / US 8", "Black/Gold", "US 8", 180.00m, 8, 5) }),
            ("Adidas Adicolor Classics Firebird Track Jacket", "adidas-firebird-track-jacket", "AD-FBD", "Shiny tricot fabric with ribbed cuffs and 3-stripes branding.", 80.00m, 90.00m, catWomensClothing.Id, brands[3].Id, false, 4.6, 44,
                new[] { ("Black / S", "Black", "S", 80.00m, 15, 5), ("Red / M", "Red", "M", 80.00m, 10, 5) }),
            ("Adidas Gazelle Bold Shoes", "adidas-gazelle-bold-shoes", "AD-GZB", "Stacked triple-platform gum rubber sole version of the retro icon.", 120.00m, 130.00m, catWomensClothing.Id, brands[3].Id, true, 4.8, 86,
                new[] { ("Core Black / US 7", "Black", "US 7", 120.00m, 11, 5), ("Pink-White / US 8", "Pink/White", "US 8", 120.00m, 4, 5) }), // Low stock!

            // Home & Living
            ("Smart LED Ambient Light Bar", "smart-led-ambient-light-bar", "SLED-LB", "Wi-Fi sync music reactive color lighting for desk and television.", 89.00m, 99.00m, catHome.Id, null, false, 4.5, 32,
                new[] { ("RGB Dual Pack / Standard", "Black", "Standard", 89.00m, 28, 5) }),
            ("Ergonomic Mesh Office Chair", "ergonomic-mesh-office-chair", "EM-CHR", "Dynamic lumbar support and 3D adjustable armrests for long sitting hours.", 299.00m, 349.00m, catHome.Id, null, true, 4.7, 49,
                new[] { ("Charcoal Gray / Standard", "Gray", "Standard", 299.00m, 12, 5), ("All Black / Standard", "Black", "Standard", 299.00m, 16, 5) }),
            ("Aroma Ultrasonic Diffuser", "aroma-ultrasonic-diffuser", "AROM-DIF", "Whisper-quiet ceramic essential oil diffuser with soothing night light.", 49.00m, 59.00m, catHome.Id, null, false, 4.4, 23,
                new[] { ("Matte White / 300ml", "White", "300ml", 49.00m, 35, 5), ("Matte Black / 300ml", "Black", "300ml", 49.00m, 20, 5) }),
            ("Minimalist Ceramic Coffee Mug Set", "ceramic-coffee-mug-set", "MUG-SET4", "Handmade stoneware 12oz ceramic mugs set of 4.", 39.00m, 45.00m, catHome.Id, null, false, 4.8, 58,
                new[] { ("Earthy Clay / Set of 4", "Clay", "Set of 4", 39.00m, 40, 5) }),
            ("Stainless Steel Thermal Flask 1L", "stainless-thermal-flask-1l", "TH-FLK1", "Double wall vacuum insulation maintains hot drinks 12h and cold 24h.", 29.00m, 35.00m, catHome.Id, null, false, 4.6, 31,
                new[] { ("Matte Black / 1 Liter", "Black", "1L", 29.00m, 30, 5), ("Sage Green / 1 Liter", "Green", "1L", 29.00m, 18, 5) }),
            ("Precision Pour-Over Coffee Kettle", "precision-pour-over-kettle", "CF-KTL", "Gooseneck spout with built-in analog thermometer for barista brews.", 55.00m, 65.00m, catHome.Id, null, false, 4.7, 27,
                new[] { ("Brushed Steel / 800ml", "Silver", "800ml", 55.00m, 22, 5) })
        };

        foreach (var item in rawProductCatalog)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = item.Item1,
                Slug = item.Item2,
                SKU = item.Item3,
                ShortDescription = item.Item4,
                FullDescription = $"{item.Item4} Built with premium craftsmanship, rigorous durability testing, and backed by a comprehensive manufacturer warranty.",
                BasePrice = item.Item5,
                CompareAtPrice = item.Item6,
                CostPrice = item.Item5 * 0.65m,
                CategoryId = item.Item7,
                BrandId = item.Item8,
                IsFeatured = item.Item9,
                AverageRating = item.Item10,
                ReviewCount = item.Item11,
                IsActive = true
            };
            products.Add(product);

            // Images
            productImages.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ImageUrl = $"/images/products/{product.Slug}-main.jpg",
                AltText = $"{product.Name} Front View",
                IsPrimary = true,
                DisplayOrder = 1
            });
            productImages.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ImageUrl = $"/images/products/{product.Slug}-angle.jpg",
                AltText = $"{product.Name} Angled View",
                IsPrimary = false,
                DisplayOrder = 2
            });

            // Tags
            productTagEntities.Add(new ProductTag { Id = Guid.NewGuid(), ProductId = product.Id, TagId = tags[0].Id }); // Trending
            if (product.CompareAtPrice.HasValue)
            {
                productTagEntities.Add(new ProductTag { Id = Guid.NewGuid(), ProductId = product.Id, TagId = tags[1].Id }); // Sale
            }

            // Variants & Inventory Movements
            int variantIndex = 1;
            foreach (var v in item.Item12)
            {
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Name = v.Item1,
                    Color = v.Item2,
                    Size = v.Item3,
                    Price = v.Item4,
                    StockQuantity = v.Item5,
                    LowStockThreshold = v.Item6,
                    SKU = $"{product.SKU}-{variantIndex++}",
                    ImageUrl = $"/images/products/{product.Slug}-main.jpg"
                };
                productVariants.Add(variant);

                stockMovements.Add(new StockMovement
                {
                    Id = Guid.NewGuid(),
                    ProductVariantId = variant.Id,
                    MovementType = StockMovementType.In,
                    Quantity = variant.StockQuantity,
                    RemainingStock = variant.StockQuantity,
                    ReferenceNumber = "INITIAL-RESTOCK",
                    Reason = "Warehouse catalog initialization",
                    CreatedByUserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                });
            }
        }

        // Soft-deleted Products (For testing Trash/Recycle bin endpoints)
        var deletedProduct1 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Discontinued iPhone 12 Case",
            Slug = "discontinued-iphone-12-case",
            SKU = "OLD-IPH12-CS",
            ShortDescription = "Clear case for older generation iPhone.",
            BasePrice = 19.99m,
            CategoryId = catSmartphones.Id,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-7),
            DeletedBy = "manager@test.com"
        };
        var deletedProduct2 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Legacy Running Shoes 2023",
            Slug = "legacy-running-shoes-2023",
            SKU = "OLD-RUN-2023",
            ShortDescription = "Out-of-season sneakers.",
            BasePrice = 49.99m,
            CategoryId = catMensClothing.Id,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-3),
            DeletedBy = "admin@test.com"
        };
        products.Add(deletedProduct1);
        products.Add(deletedProduct2);

        await context.Products.AddRangeAsync(products);
        await context.ProductImages.AddRangeAsync(productImages);
        await context.ProductVariants.AddRangeAsync(productVariants);
        await context.ProductTags.AddRangeAsync(productTagEntities);
        await context.StockMovements.AddRangeAsync(stockMovements);

        // 10. ADDRESSES FOR CUSTOMERS
        var address1 = new Address
        {
            Id = Guid.NewGuid(),
            UserId = customers[0].Id,
            FullName = "John Doe",
            PhoneNumber = "+1 555 123 4567",
            AddressLine1 = "742 Evergreen Terrace",
            City = "Springfield",
            State = "IL",
            PostalCode = "62704",
            Country = "United States",
            IsDefaultShipping = true,
            IsDefaultBilling = true
        };
        var address2 = new Address
        {
            Id = Guid.NewGuid(),
            UserId = customers[1].Id,
            FullName = "Jane Smith",
            PhoneNumber = "+1 555 987 6543",
            AddressLine1 = "221B Baker Street",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "United States",
            IsDefaultShipping = true,
            IsDefaultBilling = true
        };
        await context.Addresses.AddRangeAsync(address1, address2);

        // 11. ORDERS IN DIFFERENT STATUSES
        var seededOrders = new List<Order>();
        var seededOrderItems = new List<OrderItem>();
        var seededStatusHistories = new List<OrderStatusHistory>();
        var seededPayments = new List<Payment>();

        // Order 1: Delivered (Delivered order with payment, status history, verified review, and return request)
        var sampleVariant1 = productVariants[0]; // iPhone 15 Pro 128GB
        var sampleVariant2 = productVariants[18]; // Sony WH-1000XM5
        var order1 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2026-1001",
            UserId = customers[0].Id,
            Status = OrderStatus.Delivered,
            SubTotal = sampleVariant1.Price + sampleVariant2.Price,
            ShippingCost = shippingMethods[0].Price,
            DiscountAmount = 20.00m,
            TotalAmount = (sampleVariant1.Price + sampleVariant2.Price) + shippingMethods[0].Price - 20.00m,
            ShippingMethodId = shippingMethods[0].Id,
            CouponCodeSnapshot = "SUMMER20",
            ShippingFullName = address1.FullName,
            ShippingPhoneNumber = address1.PhoneNumber,
            ShippingAddressLine1 = address1.AddressLine1,
            ShippingCity = address1.City,
            ShippingState = address1.State,
            ShippingPostalCode = address1.PostalCode,
            ShippingCountry = address1.Country,
            CreatedAt = DateTime.UtcNow.AddDays(-14)
        };
        seededOrders.Add(order1);

        var orderItem1 = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order1.Id,
            ProductVariantId = sampleVariant1.Id,
            ProductNameSnapshot = sampleVariant1.Product.Name,
            VariantNameSnapshot = sampleVariant1.Name,
            SKUSnapshot = sampleVariant1.SKU,
            ProductImageUrlSnapshot = sampleVariant1.ImageUrl,
            UnitPriceSnapshot = sampleVariant1.Price,
            Quantity = 1,
            TotalPrice = sampleVariant1.Price
        };
        var orderItem2 = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order1.Id,
            ProductVariantId = sampleVariant2.Id,
            ProductNameSnapshot = sampleVariant2.Product.Name,
            VariantNameSnapshot = sampleVariant2.Name,
            SKUSnapshot = sampleVariant2.SKU,
            ProductImageUrlSnapshot = sampleVariant2.ImageUrl,
            UnitPriceSnapshot = sampleVariant2.Price,
            Quantity = 1,
            TotalPrice = sampleVariant2.Price
        };
        seededOrderItems.Add(orderItem1);
        seededOrderItems.Add(orderItem2);

        seededStatusHistories.AddRange(new[]
        {
            new OrderStatusHistory { Id = Guid.NewGuid(), OrderId = order1.Id, OldStatus = null, NewStatus = OrderStatus.Pending, ChangedByName = "customer1@test.com", CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new OrderStatusHistory { Id = Guid.NewGuid(), OrderId = order1.Id, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Processing, ChangedByName = "manager@test.com", CreatedAt = DateTime.UtcNow.AddDays(-13) },
            new OrderStatusHistory { Id = Guid.NewGuid(), OrderId = order1.Id, OldStatus = OrderStatus.Processing, NewStatus = OrderStatus.Shipped, ChangedByName = "manager@test.com", Comment = "Tracking: FEDEX-98127391", CreatedAt = DateTime.UtcNow.AddDays(-11) },
            new OrderStatusHistory { Id = Guid.NewGuid(), OrderId = order1.Id, OldStatus = OrderStatus.Shipped, NewStatus = OrderStatus.Delivered, ChangedByName = "System Carrier", Comment = "Delivered at doorstep", CreatedAt = DateTime.UtcNow.AddDays(-9) }
        });

        seededPayments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order1.Id,
            Provider = PaymentProvider.Mock,
            TransactionId = "TXN-MOCK-918231",
            Amount = order1.TotalAmount,
            Currency = "USD",
            Status = PaymentStatus.Paid,
            PaidAt = DateTime.UtcNow.AddDays(-14)
        });

        // Order 2: Shipped Order
        var sampleVariant3 = productVariants[27]; // Nike AF1
        var order2 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2026-1002",
            UserId = customers[1].Id,
            Status = OrderStatus.Shipped,
            SubTotal = sampleVariant3.Price,
            ShippingCost = shippingMethods[1].Price,
            DiscountAmount = 0m,
            TotalAmount = sampleVariant3.Price + shippingMethods[1].Price,
            ShippingMethodId = shippingMethods[1].Id,
            ShippingFullName = address2.FullName,
            ShippingPhoneNumber = address2.PhoneNumber,
            ShippingAddressLine1 = address2.AddressLine1,
            ShippingCity = address2.City,
            ShippingState = address2.State,
            ShippingPostalCode = address2.PostalCode,
            ShippingCountry = address2.Country,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };
        seededOrders.Add(order2);
        seededOrderItems.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order2.Id,
            ProductVariantId = sampleVariant3.Id,
            ProductNameSnapshot = sampleVariant3.Product.Name,
            VariantNameSnapshot = sampleVariant3.Name,
            SKUSnapshot = sampleVariant3.SKU,
            ProductImageUrlSnapshot = sampleVariant3.ImageUrl,
            UnitPriceSnapshot = sampleVariant3.Price,
            Quantity = 1,
            TotalPrice = sampleVariant3.Price
        });
        seededStatusHistories.AddRange(new[]
        {
            new OrderStatusHistory { Id = Guid.NewGuid(), OrderId = order2.Id, OldStatus = null, NewStatus = OrderStatus.Pending, ChangedByName = "customer2@test.com", CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new OrderStatusHistory { Id = Guid.NewGuid(), OrderId = order2.Id, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Processing, ChangedByName = "manager@test.com", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new OrderStatusHistory { Id = Guid.NewGuid(), OrderId = order2.Id, OldStatus = OrderStatus.Processing, NewStatus = OrderStatus.Shipped, ChangedByName = "manager@test.com", Comment = "Tracking: UPS-4481023", CreatedAt = DateTime.UtcNow.AddDays(-1) }
        });
        seededPayments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order2.Id,
            Provider = PaymentProvider.Mock,
            TransactionId = "TXN-MOCK-918232",
            Amount = order2.TotalAmount,
            Currency = "USD",
            Status = PaymentStatus.Paid,
            PaidAt = DateTime.UtcNow.AddDays(-3)
        });

        // Order 3: Pending Order
        var sampleVariant4 = productVariants[35]; // Adidas Samba Classic
        var order3 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2026-1003",
            UserId = customers[2].Id,
            Status = OrderStatus.Pending,
            SubTotal = sampleVariant4.Price * 2,
            ShippingCost = shippingMethods[0].Price,
            DiscountAmount = 10.00m,
            TotalAmount = (sampleVariant4.Price * 2) + shippingMethods[0].Price - 10.00m,
            ShippingMethodId = shippingMethods[0].Id,
            CouponCodeSnapshot = "WELCOME10",
            ShippingFullName = "Robert Johnson",
            ShippingPhoneNumber = "+1 555 456 7890",
            ShippingAddressLine1 = "100 Pine Street",
            ShippingCity = "Seattle",
            ShippingState = "WA",
            ShippingPostalCode = "98101",
            ShippingCountry = "United States",
            CreatedAt = DateTime.UtcNow.AddHours(-4)
        };
        seededOrders.Add(order3);
        seededOrderItems.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order3.Id,
            ProductVariantId = sampleVariant4.Id,
            ProductNameSnapshot = sampleVariant4.Product.Name,
            VariantNameSnapshot = sampleVariant4.Name,
            SKUSnapshot = sampleVariant4.SKU,
            ProductImageUrlSnapshot = sampleVariant4.ImageUrl,
            UnitPriceSnapshot = sampleVariant4.Price,
            Quantity = 2,
            TotalPrice = sampleVariant4.Price * 2
        });
        seededStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order3.Id,
            OldStatus = null,
            NewStatus = OrderStatus.Pending,
            ChangedByName = "customer3@test.com",
            CreatedAt = DateTime.UtcNow.AddHours(-4)
        });
        seededPayments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order3.Id,
            Provider = PaymentProvider.Mock,
            Amount = order3.TotalAmount,
            Currency = "USD",
            Status = PaymentStatus.Pending
        });

        // Order 4: Cancelled Order (Cancelled while pending, stock restored)
        var order4 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2026-1004",
            UserId = customers[3].Id,
            Status = OrderStatus.Cancelled,
            SubTotal = sampleVariant3.Price,
            ShippingCost = shippingMethods[0].Price,
            DiscountAmount = 0m,
            TotalAmount = sampleVariant3.Price + shippingMethods[0].Price,
            ShippingFullName = "Alice Davis",
            ShippingPhoneNumber = "+1 555 321 0987",
            ShippingAddressLine1 = "500 Market Street",
            ShippingCity = "Austin",
            ShippingState = "TX",
            ShippingPostalCode = "78701",
            ShippingCountry = "United States",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        seededOrders.Add(order4);
        seededOrderItems.Add(new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order4.Id,
            ProductVariantId = sampleVariant3.Id,
            ProductNameSnapshot = sampleVariant3.Product.Name,
            VariantNameSnapshot = sampleVariant3.Name,
            SKUSnapshot = sampleVariant3.SKU,
            ProductImageUrlSnapshot = sampleVariant3.ImageUrl,
            UnitPriceSnapshot = sampleVariant3.Price,
            Quantity = 1,
            TotalPrice = sampleVariant3.Price
        });
        seededStatusHistories.AddRange(new[]
        {
            new OrderStatusHistory { Id = Guid.NewGuid(), OrderId = order4.Id, OldStatus = null, NewStatus = OrderStatus.Pending, ChangedByName = "customer4@test.com", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new OrderStatusHistory { Id = Guid.NewGuid(), OrderId = order4.Id, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Cancelled, ChangedByName = "customer4@test.com", Comment = "Cancelled by customer before processing. Inventory restored.", CreatedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(30) }
        });

        await context.Orders.AddRangeAsync(seededOrders);
        await context.OrderItems.AddRangeAsync(seededOrderItems);
        await context.OrderStatusHistories.AddRangeAsync(seededStatusHistories);
        await context.Payments.AddRangeAsync(seededPayments);

        // 12. VERIFIED PURCHASE REVIEWS (Approved and Pending)
        var review1 = new Review
        {
            Id = Guid.NewGuid(),
            ProductId = sampleVariant1.ProductId, // iPhone 15 Pro
            UserId = customers[0].Id,
            OrderId = order1.Id,
            Rating = 5,
            Title = "Incredible titanium feel and cameras",
            Comment = "The battery and camera upgrade over my previous model is mind blowing. Extremely lightweight in hand.",
            IsApproved = true,
            ModeratedAt = DateTime.UtcNow.AddDays(-8),
            ModeratedBy = "manager@test.com"
        };
        var review2 = new Review
        {
            Id = Guid.NewGuid(),
            ProductId = sampleVariant2.ProductId, // Sony WH-1000XM5
            UserId = customers[0].Id,
            OrderId = order1.Id,
            Rating = 4,
            Title = "Top tier noise cancellation, slight comfort adjustment",
            Comment = "Blocks out airplane engine noise effortlessly. Headband takes a couple of days to break in though.",
            IsApproved = false // Pending admin moderation
        };
        await context.Reviews.AddRangeAsync(review1, review2);

        // 13. RETURN REQUEST (For Delivered Order Item)
        var returnRequest = new ReturnRequest
        {
            Id = Guid.NewGuid(),
            OrderId = order1.Id,
            OrderItemId = orderItem2.Id,
            UserId = customers[0].Id,
            Reason = "Fit issue or preference change",
            CustomerNote = "Headphone ear cushions are slightly warm for my taste during long sessions.",
            Status = ReturnStatus.Pending,
            RequestedAt = DateTime.UtcNow.AddDays(-1)
        };
        await context.ReturnRequests.AddAsync(returnRequest);

        // 14. WISHLIST ITEMS
        var wishlistItems = new List<WishlistItem>
        {
            new() { Id = Guid.NewGuid(), UserId = customers[0].Id, ProductId = products[4].Id }, // Galaxy Z Fold 5
            new() { Id = Guid.NewGuid(), UserId = customers[0].Id, ProductId = products[5].Id }, // MacBook Pro 16
            new() { Id = Guid.NewGuid(), UserId = customers[1].Id, ProductId = products[16].Id } // Nike Air Force 1
        };
        await context.WishlistItems.AddRangeAsync(wishlistItems);

        // 15. SAMPLE AUDIT LOGS
        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                UserName = "admin@test.com",
                Action = "RoleChange",
                EntityName = "AppRole",
                EntityId = managerRole.Id.ToString(),
                NewValues = "{\"Role\":\"Manager\",\"PermissionsAssigned\":11}",
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = managerUser.Id,
                UserName = "manager@test.com",
                Action = "SoftDelete",
                EntityName = "Category",
                EntityId = catArchived.Id.ToString(),
                OldValues = "{\"IsDeleted\":false,\"Name\":\"Seasonal Clearance 2025\"}",
                NewValues = "{\"IsDeleted\":true,\"DeletedBy\":\"admin@test.com\"}",
                IpAddress = "192.168.1.10",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = managerUser.Id,
                UserName = "manager@test.com",
                Action = "OrderStatusChange",
                EntityName = "Order",
                EntityId = order1.Id.ToString(),
                OldValues = "{\"Status\":\"Processing\"}",
                NewValues = "{\"Status\":\"Shipped\",\"TrackingNumber\":\"FEDEX-98127391\"}",
                IpAddress = "192.168.1.10",
                CreatedAt = DateTime.UtcNow.AddDays(-11)
            }
        };
        await context.AuditLogs.AddRangeAsync(auditLogs);

        // Save everything to database
        await context.SaveChangesAsync();
    }

    public static async Task EnsureComprehensiveCatalogSeededAsync(AppDbContext context)
    {
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
        var adminUserId = adminUser?.Id ?? Guid.NewGuid();

        // 1. ENSURE BRANDS
        var brandDefinitions = new[]
        {
            ("Apple", "apple", "Innovative consumer tech and hardware.", "/images/brands/apple.png"),
            ("Samsung", "samsung", "Global leader in digital electronics.", "/images/brands/samsung.png"),
            ("Nike", "nike", "Premier athletic footwear and apparel.", "/images/brands/nike.png"),
            ("Adidas", "adidas", "Iconic sportswear, shoes and streetwear.", "/images/brands/adidas.png"),
            ("Sony", "sony", "Cutting-edge audio and gaming entertainment.", "/images/brands/sony.png"),
            ("Microsoft", "microsoft", "Gaming consoles, software and hardware.", "/images/brands/microsoft.png"),
            ("Nintendo", "nintendo", "Gaming consoles and family entertainment.", "/images/brands/nintendo.png"),
            ("Logitech", "logitech", "High performance PC peripherals and accessories.", "/images/brands/logitech.png"),
            ("Razer", "razer", "For gamers, by gamers hardware.", "/images/brands/razer.png"),
            ("Dyson", "dyson", "Engineering marvel home and personal care tech.", "/images/brands/dyson.png"),
            ("Dior", "dior", "French luxury fashion and fragrances.", "/images/brands/dior.png"),
            ("Garmin", "garmin", "GPS sports watches and outdoor navigation.", "/images/brands/garmin.png"),
            ("Ray-Ban", "ray-ban", "Timeless sunglasses and eyewear.", "/images/brands/rayban.png"),
            ("Bellroy", "bellroy", "Slim leather wallets and carry accessories.", "/images/brands/bellroy.png"),
            ("Moleskine", "moleskine", "Iconic notebooks and stationery.", "/images/brands/moleskine.png")
        };

        var brandsDict = new Dictionary<string, Brand>();
        foreach (var bDef in brandDefinitions)
        {
            var existingBrand = await context.Brands.FirstOrDefaultAsync(b => b.Slug == bDef.Item2);
            if (existingBrand == null)
            {
                existingBrand = new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = bDef.Item1,
                    Slug = bDef.Item2,
                    Description = bDef.Item3,
                    LogoUrl = bDef.Item4
                };
                await context.Brands.AddAsync(existingBrand);
            }
            brandsDict[bDef.Item2] = existingBrand;
        }

        // 2. ENSURE TAGS
        var tagDefinitions = new[]
        {
            ("Trending", "trending"),
            ("Sale", "sale"),
            ("Eco Friendly", "eco-friendly"),
            ("New Arrival", "new-arrival"),
            ("Premium", "premium"),
            ("Casual", "casual"),
            ("Sport", "sport")
        };

        var tagsDict = new Dictionary<string, Tag>();
        foreach (var tDef in tagDefinitions)
        {
            var existingTag = await context.Tags.FirstOrDefaultAsync(t => t.Slug == tDef.Item2);
            if (existingTag == null)
            {
                existingTag = new Tag { Id = Guid.NewGuid(), Name = tDef.Item1, Slug = tDef.Item2 };
                await context.Tags.AddAsync(existingTag);
            }
            tagsDict[tDef.Item2] = existingTag;
        }

        await context.SaveChangesAsync();

        // 3. ENSURE ALL CATEGORIES
        // Migrate old 'fashion-apparel' to 'clothing' if present
        var oldFashion = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "fashion-apparel");
        if (oldFashion != null)
        {
            oldFashion.Name = "Clothing";
            oldFashion.Slug = "clothing";
            oldFashion.Description = "Trending fashion and apparel for everyone";
            await context.SaveChangesAsync();
        }

        var categoryDefinitions = new[]
        {
            ("Electronics", "electronics", "Gadgets, smartphones, and computers", 1, (string?)null),
            ("Smartphones", "smartphones", "Flagship and budget smartphones", 1, "electronics"),
            ("Laptops & Computers", "laptops-computers", "High performance laptops and accessories", 2, "electronics"),
            ("Audio & Headphones", "audio-headphones", "Wireless headphones and audio speakers", 3, "electronics"),
            ("Gaming & Consoles", "gaming-consoles", "Consoles, controllers, and premium gaming gear", 4, "electronics"),
            ("Clothing", "clothing", "Trending fashion and apparel for everyone", 2, null),
            ("Men's Clothing", "mens-clothing", "Shirts, jackets, and sportswear for men", 1, "clothing"),
            ("Women's Clothing", "womens-clothing", "Dresses, activewear, and shoes for women", 2, "clothing"),
            ("Kids' Clothing", "kids-clothing", "Fun, comfy everyday apparel and shoes for boys & girls", 3, "clothing"),
            ("Home & Living", "home-living", "Modern home accessories and furniture", 3, null),
            ("Sports & Fitness", "sports-fitness", "Gym equipment, running accessories, and workout gear", 4, null),
            ("Beauty & Personal Care", "beauty-personal-care", "Luxury fragrances, skincare, and grooming essentials", 5, null),
            ("Watches & Accessories", "watches-accessories", "Smartwatches, sunglasses, leather wallets, and timepieces", 6, null),
            ("Books & Stationery", "books-stationery", "Bestselling books, luxury notebooks, and writing instruments", 7, null)
        };

        var categoriesDict = new Dictionary<string, Category>();
        foreach (var cDef in categoryDefinitions)
        {
            var existingCat = await context.Categories.FirstOrDefaultAsync(c => c.Slug == cDef.Item2);
            if (existingCat == null)
            {
                Guid? parentId = null;
                if (cDef.Item5 != null && categoriesDict.TryGetValue(cDef.Item5, out var parentCat))
                {
                    parentId = parentCat.Id;
                }

                existingCat = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = cDef.Item1,
                    Slug = cDef.Item2,
                    Description = cDef.Item3,
                    DisplayOrder = cDef.Item4,
                    ParentCategoryId = parentId
                };
                await context.Categories.AddAsync(existingCat);
            }
            categoriesDict[cDef.Item2] = existingCat;
        }

        await context.SaveChangesAsync();

        // Re-link parent categories to enforce exact hierarchy
        var catElectronics = categoriesDict.GetValueOrDefault("electronics") ?? await context.Categories.FirstOrDefaultAsync(c => c.Slug == "electronics");
        var catClothing = categoriesDict.GetValueOrDefault("clothing") ?? await context.Categories.FirstOrDefaultAsync(c => c.Slug == "clothing");

        if (catElectronics != null)
        {
            if (categoriesDict.TryGetValue("gaming-consoles", out var gc)) gc.ParentCategoryId = catElectronics.Id;
            if (categoriesDict.TryGetValue("smartphones", out var sp)) sp.ParentCategoryId = catElectronics.Id;
            if (categoriesDict.TryGetValue("laptops-computers", out var lp)) lp.ParentCategoryId = catElectronics.Id;
            if (categoriesDict.TryGetValue("audio-headphones", out var ah)) ah.ParentCategoryId = catElectronics.Id;
        }

        if (catClothing != null)
        {
            if (categoriesDict.TryGetValue("mens-clothing", out var mc)) mc.ParentCategoryId = catClothing.Id;
            if (categoriesDict.TryGetValue("womens-clothing", out var wc)) wc.ParentCategoryId = catClothing.Id;
            if (categoriesDict.TryGetValue("kids-clothing", out var kc)) kc.ParentCategoryId = catClothing.Id;
        }
        await context.SaveChangesAsync();

        // 4. COMPREHENSIVE PRODUCT DEFINITIONS PER CATEGORY
        var newCatalog = new List<(
            string Name, string Slug, string SKU, string ShortDesc, decimal Price, decimal? ComparePrice,
            string CategorySlug, string? BrandSlug, bool IsFeatured, double Rating, int Reviews,
            (string VariantName, string Color, string Size, decimal Price, int Stock, int LowStock)[] Variants
        )>
        {
            // === GAMING & CONSOLES ===
            ("PlayStation 5 Slim Console", "playstation-5-slim", "PS5-SLM",
             "Ultra-high speed SSD, ray tracing, 4K gaming and immersive 3D audio.", 499.99m, 549.99m,
             "gaming-consoles", "sony", true, 4.9, 148,
             new[] {
                 ("Disc Edition / White 1TB", "White", "1TB", 499.99m, 25, 5),
                 ("Digital Edition / White 1TB", "White", "1TB Digital", 449.99m, 18, 5)
             }),
            ("Xbox Series X 1TB Console", "xbox-series-x", "XBX-1TB",
             "Fastest, most powerful Xbox ever with 12 teraflops of raw graphic processing power.", 499.99m, 529.99m,
             "gaming-consoles", "microsoft", true, 4.8, 92,
             new[] {
                 ("Carbon Black / 1TB", "Black", "1TB", 499.99m, 20, 5)
             }),
            ("Nintendo Switch OLED Model", "nintendo-switch-oled", "NSW-OLED",
             "Vibrant 7-inch OLED screen, wide adjustable stand, and wired LAN port dock.", 349.99m, 369.99m,
             "gaming-consoles", "nintendo", true, 4.8, 115,
             new[] {
                 ("White Joy-Con / 64GB", "White", "Standard", 349.99m, 30, 5),
                 ("Neon Red/Blue Joy-Con / 64GB", "Neon", "Standard", 349.99m, 15, 5)
             }),
            ("Logitech G Pro X Superlight 2", "logitech-g-pro-x-superlight-2", "LOG-SL2",
             "60g ultra-lightweight wireless esports gaming mouse with HERO 2 sensor.", 159.99m, 169.99m,
             "gaming-consoles", "logitech", false, 4.9, 87,
             new[] {
                 ("Matte Black / Wireless", "Black", "Standard", 159.99m, 35, 5),
                 ("Crisp White / Wireless", "White", "Standard", 159.99m, 28, 5),
                 ("Magenta / Wireless", "Magenta", "Standard", 159.99m, 12, 5)
             }),
            ("Razer BlackWidow V4 Pro Mechanical Keyboard", "razer-blackwidow-v4-pro", "RZ-BW4P",
             "Immersive battlestation mechanical keyboard with dedicated macro keys and Razer Command Dial.", 229.99m, 249.99m,
             "gaming-consoles", "razer", false, 4.7, 54,
             new[] {
                 ("Green Clicky Switches / Full Size", "Black", "Green Switch", 229.99m, 22, 5),
                 ("Yellow Linear Switches / Full Size", "Black", "Yellow Switch", 229.99m, 16, 5)
             }),
            ("DualSense Edge Wireless Controller", "dualsense-edge-controller", "DS-EDGE",
             "High-performance customizable pro controller with remappable buttons and replaceable stick modules.", 199.99m, 219.99m,
             "gaming-consoles", "sony", false, 4.6, 43,
             new[] {
                 ("White-Black / Standard", "White", "Standard", 199.99m, 20, 5)
             }),

            // === SPORTS & FITNESS ===
            ("Bowflex SelectTech 552 Adjustable Dumbbells", "bowflex-selecttech-552", "BW-552",
             "Adjustable weights replace 15 sets of dumbbells from 5 to 52.5 lbs in seconds.", 429.00m, 499.00m,
             "sports-fitness", null, true, 4.9, 134,
             new[] {
                 ("Black-Red / Pair (5-52.5 lbs)", "Black/Red", "Pair", 429.00m, 14, 5)
             }),
            ("Manduka PRO Premium Yoga Mat", "manduka-pro-yoga-mat", "MAN-PRO",
             "Ultra-dense 6mm cushioned yoga mat engineered for joint protection and lifetime durability.", 138.00m, 150.00m,
             "sports-fitness", null, false, 4.8, 89,
             new[] {
                 ("Black Sage / 71 inch", "Sage", "71 inch", 138.00m, 25, 5),
                 ("Midnight Blue / 71 inch", "Blue", "71 inch", 138.00m, 18, 5)
             }),
            ("Garmin Forerunner 265 GPS Running Watch", "garmin-forerunner-265", "GAR-FR265",
             "Brilliant AMOLED touchscreen display with advanced training metrics and recovery insights.", 449.99m, 479.99m,
             "sports-fitness", "garmin", true, 4.9, 78,
             new[] {
                 ("Black / Slate Gray 46mm", "Black", "46mm", 449.99m, 16, 5),
                 ("Whitestone / Tidal Blue 46mm", "White", "46mm", 449.99m, 10, 5)
             }),
            ("Under Armour Undeniable 5.0 Duffle Bag", "under-armour-undeniable-duffle", "UA-UND5",
             "Water-repellent UA Storm technology duffle with large vented laundry shoe pocket.", 45.00m, 55.00m,
             "sports-fitness", null, false, 4.7, 66,
             new[] {
                 ("Pitch Black / Medium 58L", "Black", "M", 45.00m, 40, 5),
                 ("Heather Gray / Medium 58L", "Gray", "M", 45.00m, 25, 5)
             }),
            ("Hydro Flask 32 oz Wide Mouth Straw Lid", "hydro-flask-32oz-wide-mouth", "HF-32OZ",
             "TempShield double-wall vacuum insulation keeps beverages cold up to 24 hours.", 44.95m, 49.95m,
             "sports-fitness", null, false, 4.8, 112,
             new[] {
                 ("Pacific Blue / 32 oz", "Blue", "32 oz", 44.95m, 35, 5),
                 ("Goji Berry Red / 32 oz", "Red", "32 oz", 44.95m, 22, 5),
                 ("Black / 32 oz", "Black", "32 oz", 44.95m, 45, 5)
             }),

            // === BEAUTY & PERSONAL CARE ===
            ("Dior Sauvage Eau de Parfum", "dior-sauvage-edp", "DIOR-SVG",
             "Iconic noble fragrance blending fresh Calabrian bergamot with smoky vanilla absolute.", 165.00m, 185.00m,
             "beauty-personal-care", "dior", true, 4.9, 230,
             new[] {
                 ("60ml / Spray", "Navy", "60ml", 120.00m, 30, 5),
                 ("100ml / Spray", "Navy", "100ml", 165.00m, 45, 5),
                 ("200ml / Spray", "Navy", "200ml", 220.00m, 15, 5)
             }),
            ("Dyson Supersonic Hair Dryer", "dyson-supersonic-hair-dryer", "DYS-SUP",
             "Engineered for fast drying with intelligent heat control to protect natural hair shine.", 429.99m, 449.99m,
             "beauty-personal-care", "dyson", true, 4.8, 142,
             new[] {
                 ("Nickel / Copper", "Copper", "Standard", 429.99m, 18, 5),
                 ("Iron / Fuchsia", "Fuchsia", "Standard", 429.99m, 14, 5)
             }),
            ("The Ordinary Niacinamide 10% + Zinc 1%", "the-ordinary-niacinamide", "ORD-NIA",
             "High-strength vitamin and mineral blemish formula that balances sebum activity.", 12.00m, 15.00m,
             "beauty-personal-care", null, false, 4.7, 189,
             new[] {
                 ("30ml / Dropper Bottle", "Clear", "30ml", 12.00m, 80, 10),
                 ("60ml / Dropper Bottle", "Clear", "60ml", 19.00m, 50, 10)
             }),
            ("Philips Norelco Shaver 9000 Prestige", "philips-norelco-shaver-9000", "PH-S9000",
             "Precision NanoTech dual blades with Qi wireless charging pad and premium travel pouch.", 329.99m, 379.99m,
             "beauty-personal-care", null, false, 4.7, 61,
             new[] {
                 ("Matte Silver / Deluxe Kit", "Silver", "Standard", 329.99m, 15, 5)
             }),
            ("CeraVe Hydrating Facial Cleanser", "cerave-hydrating-facial-cleanser", "CRV-HFC",
             "Non-comedogenic daily face wash with 3 essential ceramides and hyaluronic acid.", 16.99m, 19.99m,
             "beauty-personal-care", null, false, 4.8, 175,
             new[] {
                 ("236ml / Pump", "White", "236ml", 16.99m, 60, 10),
                 ("473ml / Pump", "White", "473ml", 24.99m, 40, 10)
             }),

            // === WATCHES & ACCESSORIES ===
            ("Apple Watch Ultra 2 GPS + Cellular", "apple-watch-ultra-2", "AW-U2",
             "Rugged 49mm aerospace titanium case, brightest Apple display, and precision dual-frequency GPS.", 799.00m, 849.00m,
             "watches-accessories", "apple", true, 4.9, 160,
             new[] {
                 ("Titanium Case / Orange Ocean Band", "Orange", "49mm", 799.00m, 20, 5),
                 ("Titanium Case / Blue Alpine Loop", "Blue", "49mm", 799.00m, 18, 5)
             }),
            ("Samsung Galaxy Watch 6 Classic", "galaxy-watch-6-classic", "GW6-CLS",
             "Rotating physical bezel, sapphire crystal glass, and advanced personalized sleep coaching.", 399.99m, 429.99m,
             "watches-accessories", "samsung", true, 4.7, 95,
             new[] {
                 ("Black Stainless / 47mm", "Black", "47mm", 399.99m, 24, 5),
                 ("Silver Stainless / 47mm", "Silver", "47mm", 399.99m, 18, 5)
             }),
            ("Ray-Ban Aviator Classic Polarized", "ray-ban-aviator-classic", "RB-3025",
             "Legendary teardrop gold frame sunglasses with crystal green polarized UV lenses.", 215.00m, 235.00m,
             "watches-accessories", "ray-ban", true, 4.8, 140,
             new[] {
                 ("Gold Frame / Polarized Green G-15", "Gold", "58mm Standard", 215.00m, 35, 5),
                 ("Black Frame / Polarized Grey Gradient", "Black", "58mm Standard", 215.00m, 25, 5)
             }),
            ("Bellroy Hide & Seek RFID Leather Wallet", "bellroy-hide-and-seek-wallet", "BEL-HS",
             "Slim bi-fold wallet made from environmentally certified leather with hidden bill section.", 89.00m, 99.00m,
             "watches-accessories", "bellroy", false, 4.8, 77,
             new[] {
                 ("Premium Black Leather", "Black", "Slim", 89.00m, 40, 5),
                 ("Caramel Brown Leather", "Brown", "Slim", 89.00m, 30, 5)
             }),
            ("Fossil Grant Chronograph Leather Watch", "fossil-grant-chronograph", "FOS-FS4835",
             "Classic roman numeral dial with genuine blue leather strap and quartz stopwatch.", 140.00m, 160.00m,
             "watches-accessories", null, false, 4.6, 52,
             new[] {
                 ("Rose Gold-Blue / 44mm", "Blue", "44mm", 140.00m, 22, 5),
                 ("Smoke Black / 44mm", "Black", "44mm", 140.00m, 18, 5)
             }),

            // === BOOKS & STATIONERY ===
            ("Atomic Habits by James Clear", "atomic-habits-james-clear", "BK-ATHB",
             "An easy and proven way to build good habits and break bad ones. Over 15 million copies sold.", 27.00m, 32.00m,
             "books-stationery", null, true, 4.9, 310,
             new[] {
                 ("Hardcover Edition", "White", "Hardcover", 27.00m, 60, 10),
                 ("Paperback Edition", "White", "Paperback", 18.00m, 45, 10)
             }),
            ("Clean Code by Robert C. Martin", "clean-code-robert-martin", "BK-CLCD",
             "A handbook of agile software craftsmanship. Master the fundamentals of robust software architecture.", 45.00m, 50.00m,
             "books-stationery", null, true, 4.9, 185,
             new[] {
                 ("Paperback 1st Edition", "Black", "Paperback", 45.00m, 50, 10)
             }),
            ("The Psychology of Money", "psychology-of-money-housel", "BK-POM",
             "Timeless lessons on wealth, greed, and happiness by award-winning author Morgan Housel.", 24.99m, 29.99m,
             "books-stationery", null, false, 4.8, 140,
             new[] {
                 ("Hardcover Edition", "Blue", "Hardcover", 24.99m, 40, 10),
                 ("Paperback Edition", "Blue", "Paperback", 16.99m, 35, 10)
             }),
            ("Moleskine Classic Ruled Hardcover Notebook", "moleskine-classic-notebook", "MOL-NBK",
             "Ivory acid-free 70gsm pages, elastic closure band, and expandable inner rear pocket.", 24.50m, 28.00m,
             "books-stationery", "moleskine", false, 4.8, 98,
             new[] {
                 ("Large Black / 240 Pages", "Black", "Large (5x8.25 in)", 24.50m, 50, 10),
                 ("Sapphire Blue / 240 Pages", "Blue", "Large (5x8.25 in)", 24.50m, 35, 10)
             }),
            ("Parker Sonnet Black Lacquer Fountain Pen", "parker-sonnet-fountain-pen", "PRK-SNT",
             "Hand-crafted 18k solid gold finish nib with glossy black lacquer and gold-plated trim.", 145.00m, 175.00m,
             "books-stationery", null, false, 4.7, 46,
             new[] {
                 ("Black Lacquer / Gold Trim / Medium Nib", "Black/Gold", "Medium Nib", 145.00m, 15, 5),
                 ("Stainless Steel / Chrome Trim / Fine Nib", "Silver", "Fine Nib", 115.00m, 18, 5)
             }),

            // === HOME & LIVING (ADDITIONAL PREMIUM PRODUCTS) ===
            ("Dyson V15 Detect Cordless Vacuum", "dyson-v15-detect-vacuum", "DYS-V15",
             "Laser reveals microscopic dust with piezo sensor acoustic counting particle measurement.", 749.99m, 799.99m,
             "home-living", "dyson", true, 4.8, 88,
             new[] {
                 ("Yellow / Nickel Pro", "Yellow", "Standard", 749.99m, 15, 5)
             }),
            ("Philips Hue Smart Ambiance Starter Kit", "philips-hue-starter-kit", "PH-HUE-KIT",
             "White & Color Ambiance smart LED bulbs with Hue Bridge. 16 million colors and sync with music.", 199.99m, 229.99m,
             "home-living", null, false, 4.7, 72,
             new[] {
                 ("A19 4-Pack Bulbs + Smart Bridge", "White", "4-Pack", 199.99m, 25, 5)
             }),
            ("Nespresso Vertuo Pop Coffee Machine", "nespresso-vertuo-pop", "NES-VPOP",
             "Centrifusion technology brews 4 cup sizes from espresso to mug with velvety crema.", 129.00m, 149.00m,
             "home-living", null, false, 4.6, 64,
             new[] {
                 ("Liquorice Black / Compact", "Black", "Compact", 129.00m, 30, 5),
                 ("Mango Yellow / Compact", "Yellow", "Compact", 129.00m, 18, 5)
             }),

            // === ELECTRONICS DIRECT ACCESSORIES ===
            ("Anker 737 Power Bank 24000mAh 140W", "anker-737-power-bank", "ANK-737",
             "Ultra-powerful two-way fast charging with smart digital smart display.", 149.99m, 169.99m,
             "electronics", null, true, 4.9, 125,
             new[] {
                 ("Charcoal Black / 24000mAh", "Black", "24000mAh", 149.99m, 40, 5)
             }),
            ("Samsung T7 Shield 2TB Portable SSD", "samsung-t7-shield-2tb", "SAM-T7S",
             "Rugged USB 3.2 Gen 2 external SSD with IP65 dust and water resistance.", 179.99m, 209.99m,
             "electronics", "samsung", false, 4.8, 93,
             new[] {
                 ("Shield Black / 2TB", "Black", "2TB", 179.99m, 35, 5),
                 ("Shield Blue / 2TB", "Blue", "2TB", 179.99m, 22, 5)
             }),

            // === KIDS' CLOTHING ===
            ("Kids Organic Cotton Graphic Tee", "kids-organic-cotton-graphic-tee", "KID-TEE-01",
             "Soft breathable organic cotton casual t-shirt with playful graphic print.", 24.99m, 29.99m,
             "kids-clothing", "nike", false, 4.8, 38,
             new[] {
                 ("Ocean Blue / 6-7Y", "Blue", "6-7Y", 24.99m, 25, 5),
                 ("Sunny Yellow / 8-9Y", "Yellow", "8-9Y", 24.99m, 20, 5)
             }),
            ("Kids Cozy Fleece Pullover Hoodie", "kids-cozy-fleece-pullover-hoodie", "KID-HOD-02",
             "Plush brushed fleece hoodie with front kangaroo pocket for warm daily play.", 39.99m, 44.99m,
             "kids-clothing", "adidas", true, 4.9, 52,
             new[] {
                 ("Heather Gray / 6-7Y", "Gray", "6-7Y", 39.99m, 20, 5),
                 ("Pastel Pink / 8-9Y", "Pink", "8-9Y", 39.99m, 18, 5)
             }),
            ("Kids Waterproof Lightweight Parka", "kids-waterproof-lightweight-parka", "KID-PRK-03",
             "All-weather windproof and rainproof jacket with reflective safety strips.", 64.99m, 74.99m,
             "kids-clothing", "nike", true, 4.7, 29,
             new[] {
                 ("Forest Green / 7-8Y", "Green", "7-8Y", 64.99m, 16, 5),
                 ("Bright Orange / 9-10Y", "Orange", "9-10Y", 64.99m, 12, 5)
             }),
            ("Kids Stretch Denim Everyday Jeans", "kids-stretch-denim-everyday-jeans", "KID-JNS-04",
             "Durable flexible stretch denim jeans with adjustable comfort waistband.", 34.99m, 39.99m,
             "kids-clothing", "nike", false, 4.6, 21,
             new[] {
                 ("Classic Blue / 6-7Y", "Blue", "6-7Y", 34.99m, 25, 5),
                 ("Washed Black / 8-9Y", "Black", "8-9Y", 34.99m, 20, 5)
             })
        };

        var productsToAdd = new List<Product>();
        var variantsToAdd = new List<ProductVariant>();
        var imagesToAdd = new List<ProductImage>();
        var tagsToAdd = new List<ProductTag>();
        var movementsToAdd = new List<StockMovement>();

        foreach (var item in newCatalog)
        {
            var existingProduct = await context.Products.FirstOrDefaultAsync(p => p.Slug == item.Slug);
            if (existingProduct != null)
                continue;

            if (!categoriesDict.TryGetValue(item.CategorySlug, out var targetCat))
                continue;

            Guid? brandId = null;
            if (item.BrandSlug != null && brandsDict.TryGetValue(item.BrandSlug, out var targetBrand))
            {
                brandId = targetBrand.Id;
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = item.Name,
                Slug = item.Slug,
                SKU = item.SKU,
                ShortDescription = item.ShortDesc,
                FullDescription = $"{item.ShortDesc} Crafted with top tier materials, tested to the highest quality benchmarks, and covered by a comprehensive warranty.",
                BasePrice = item.Price,
                CompareAtPrice = item.ComparePrice,
                CostPrice = item.Price * 0.65m,
                CategoryId = targetCat.Id,
                BrandId = brandId,
                IsFeatured = item.IsFeatured,
                AverageRating = item.Rating,
                ReviewCount = item.Reviews,
                IsActive = true
            };
            productsToAdd.Add(product);

            // Images
            imagesToAdd.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ImageUrl = $"/images/products/{product.Slug}-main.jpg",
                AltText = $"{product.Name} Front View",
                IsPrimary = true,
                DisplayOrder = 1
            });
            imagesToAdd.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ImageUrl = $"/images/products/{product.Slug}-angle.jpg",
                AltText = $"{product.Name} Angled View",
                IsPrimary = false,
                DisplayOrder = 2
            });

            // Tags
            if (tagsDict.TryGetValue("trending", out var tTrending))
                tagsToAdd.Add(new ProductTag { Id = Guid.NewGuid(), ProductId = product.Id, TagId = tTrending.Id });

            if (product.CompareAtPrice.HasValue && tagsDict.TryGetValue("sale", out var tSale))
                tagsToAdd.Add(new ProductTag { Id = Guid.NewGuid(), ProductId = product.Id, TagId = tSale.Id });

            if (tagsDict.TryGetValue("new-arrival", out var tNew))
                tagsToAdd.Add(new ProductTag { Id = Guid.NewGuid(), ProductId = product.Id, TagId = tNew.Id });

            // Variants & Inventory Movements
            int vIndex = 1;
            foreach (var v in item.Variants)
            {
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Name = v.VariantName,
                    Color = v.Color,
                    Size = v.Size,
                    Price = v.Price,
                    StockQuantity = v.Stock,
                    LowStockThreshold = v.LowStock,
                    SKU = $"{product.SKU}-{vIndex++}",
                    ImageUrl = $"/images/products/{product.Slug}-main.jpg"
                };
                variantsToAdd.Add(variant);

                movementsToAdd.Add(new StockMovement
                {
                    Id = Guid.NewGuid(),
                    ProductVariantId = variant.Id,
                    MovementType = StockMovementType.In,
                    Quantity = variant.StockQuantity,
                    RemainingStock = variant.StockQuantity,
                    ReferenceNumber = "SEED-CATALOG-RESTOCK",
                    Reason = "Comprehensive catalog seeding",
                    CreatedByUserId = adminUserId,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                });
            }
        }

        if (productsToAdd.Any())
        {
            await context.Products.AddRangeAsync(productsToAdd);
            await context.ProductImages.AddRangeAsync(imagesToAdd);
            await context.ProductVariants.AddRangeAsync(variantsToAdd);
            await context.ProductTags.AddRangeAsync(tagsToAdd);
            await context.StockMovements.AddRangeAsync(movementsToAdd);
            await context.SaveChangesAsync();
        }

        await EnsureUniqueProductImagesAsync(context);
    }

    private static readonly Dictionary<string, string> UniqueProductImages = new()
    {
        // === SMARTPHONES ===
        ["iphone-15-pro"] = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=800&auto=format&fit=crop&q=80",
        ["iphone-15"] = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=800&auto=format&fit=crop&q=80",
        ["galaxy-s24-ultra"] = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=800&auto=format&fit=crop&q=80",
        ["galaxy-s24-plus"] = "https://images.unsplash.com/photo-1580910051074-3eb694886505?w=800&auto=format&fit=crop&q=80",
        ["galaxy-z-fold-5"] = "https://images.unsplash.com/photo-1565849904461-04a58ad377e0?w=800&auto=format&fit=crop&q=80",

        // === LAPTOPS & COMPUTERS ===
        ["macbook-pro-16"] = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=800&auto=format&fit=crop&q=80",
        ["macbook-air-15"] = "https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?w=800&auto=format&fit=crop&q=80",
        ["galaxy-book4-pro"] = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=800&auto=format&fit=crop&q=80",
        ["galaxy-book4-360"] = "https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2?w=800&auto=format&fit=crop&q=80",

        // === AUDIO & HEADPHONES ===
        ["sony-wh-1000xm5"] = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800&auto=format&fit=crop&q=80",
        ["sony-wf-1000xm5"] = "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=800&auto=format&fit=crop&q=80",
        ["airpods-max"] = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=800&auto=format&fit=crop&q=80",
        ["airpods-pro-2"] = "https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?w=800&auto=format&fit=crop&q=80",
        ["galaxy-buds2-pro"] = "https://images.unsplash.com/photo-1572536147248-ac59a8abfa4b?w=800&auto=format&fit=crop&q=80",

        // === MEN'S CLOTHING & SHOES ===
        ["nike-tech-fleece-windrunner"] = "https://images.unsplash.com/photo-1556905055-8f358a7a47b2?w=800&auto=format&fit=crop&q=80",
        ["nike-tech-fleece-joggers"] = "https://images.unsplash.com/photo-1552902865-b72c031ac5ea?w=800&auto=format&fit=crop&q=80",
        ["nike-air-force-1-07"] = "https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=800&auto=format&fit=crop&q=80",
        ["nike-pegasus-40"] = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=800&auto=format&fit=crop&q=80",
        ["adidas-originals-trefoil-hoodie"] = "https://images.unsplash.com/photo-1509967419530-da38b4704bc6?w=800&auto=format&fit=crop&q=80",
        ["adidas-ultraboost-light"] = "https://images.unsplash.com/photo-1587563871167-1ee9c731aefb?w=800&auto=format&fit=crop&q=80",
        ["adidas-samba-classic"] = "https://images.unsplash.com/photo-1608231387042-66d1773070a5?w=800&auto=format&fit=crop&q=80",

        // === WOMEN'S CLOTHING & SHOES ===
        ["nike-dri-fit-one-leggings"] = "https://images.unsplash.com/photo-1506152983158-b4a74a01c721?w=800&auto=format&fit=crop&q=80",
        ["nike-phoenix-fleece-hoodie"] = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=800&auto=format&fit=crop&q=80",
        ["nike-invincible-3-shoes"] = "https://images.unsplash.com/photo-1600185365483-26d7a4cc7519?w=800&auto=format&fit=crop&q=80",
        ["adidas-firebird-track-jacket"] = "https://images.unsplash.com/photo-1495105787522-5334e3ffa0ef?w=800&auto=format&fit=crop&q=80",
        ["adidas-gazelle-bold-shoes"] = "https://images.unsplash.com/photo-1560769629-975ec94e6a86?w=800&auto=format&fit=crop&q=80",

        // === HOME & LIVING ===
        ["smart-led-ambient-light-bar"] = "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80",
        ["ergonomic-mesh-office-chair"] = "https://images.unsplash.com/photo-1580481077111-92f7bb7e2d9a?w=800&auto=format&fit=crop&q=80",
        ["aroma-ultrasonic-diffuser"] = "https://images.unsplash.com/photo-1608571423902-eed4a5ad8108?w=800&auto=format&fit=crop&q=80",
        ["ceramic-coffee-mug-set"] = "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?w=800&auto=format&fit=crop&q=80",
        ["stainless-thermal-flask-1l"] = "https://images.unsplash.com/photo-1544816155-12df9643f363?w=800&auto=format&fit=crop&q=80",
        ["precision-pour-over-kettle"] = "https://images.unsplash.com/photo-1541167760496-1628856ab772?w=800&auto=format&fit=crop&q=80",
        ["dyson-v15-detect-vacuum"] = "https://images.unsplash.com/photo-1558317374-067fb5f30001?w=800&auto=format&fit=crop&q=80",
        ["philips-hue-starter-kit"] = "https://images.unsplash.com/photo-1565814636199-ae8133055c1c?w=800&auto=format&fit=crop&q=80",
        ["nespresso-vertuo-pop"] = "https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=800&auto=format&fit=crop&q=80",

        // === GAMING & CONSOLES ===
        ["playstation-5-slim"] = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=800&auto=format&fit=crop&q=80",
        ["xbox-series-x"] = "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=800&auto=format&fit=crop&q=80",
        ["nintendo-switch-oled"] = "https://images.unsplash.com/photo-1578303512597-81e6cc155b3e?w=800&auto=format&fit=crop&q=80",
        ["logitech-g-pro-x-superlight-2"] = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=800&auto=format&fit=crop&q=80",
        ["razer-blackwidow-v4-pro"] = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=800&auto=format&fit=crop&q=80",
        ["dualsense-edge-controller"] = "https://images.unsplash.com/photo-1592840496694-26d035b52b48?w=800&auto=format&fit=crop&q=80",

        // === SPORTS & FITNESS ===
        ["bowflex-selecttech-552"] = "https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?w=800&auto=format&fit=crop&q=80",
        ["manduka-pro-yoga-mat"] = "https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=800&auto=format&fit=crop&q=80",
        ["garmin-forerunner-265"] = "https://images.unsplash.com/photo-1510017803434-a899398421b3?w=800&auto=format&fit=crop&q=80",
        ["under-armour-undeniable-duffle"] = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=800&auto=format&fit=crop&q=80",
        ["hydro-flask-32oz-wide-mouth"] = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=800&auto=format&fit=crop&q=80",

        // === BEAUTY & PERSONAL CARE ===
        ["dior-sauvage-edp"] = "https://images.unsplash.com/photo-1523293182086-7651a899d37f?w=800&auto=format&fit=crop&q=80",
        ["dyson-supersonic-hair-dryer"] = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=800&auto=format&fit=crop&q=80",
        ["the-ordinary-niacinamide"] = "https://images.unsplash.com/photo-1620916566398-39f1143ab7be?w=800&auto=format&fit=crop&q=80",
        ["philips-norelco-shaver-9000"] = "https://images.unsplash.com/photo-1621607512214-68297480165e?w=800&auto=format&fit=crop&q=80",
        ["cerave-hydrating-facial-cleanser"] = "https://images.unsplash.com/photo-1556228720-195a672e8a03?w=800&auto=format&fit=crop&q=80",

        // === WATCHES & ACCESSORIES ===
        ["apple-watch-ultra-2"] = "https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?w=800&auto=format&fit=crop&q=80",
        ["galaxy-watch-6-classic"] = "https://images.unsplash.com/photo-1579586337278-3befd40fd17a?w=800&auto=format&fit=crop&q=80",
        ["ray-ban-aviator-classic"] = "https://images.unsplash.com/photo-1511499767150-a48a237f0083?w=800&auto=format&fit=crop&q=80",
        ["bellroy-hide-and-seek-wallet"] = "https://images.unsplash.com/photo-1627123424574-724758594e93?w=800&auto=format&fit=crop&q=80",
        ["fossil-grant-chronograph"] = "https://images.unsplash.com/photo-1524805444758-089113d48a6d?w=800&auto=format&fit=crop&q=80",

        // === BOOKS & STATIONERY ===
        ["atomic-habits-james-clear"] = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=800&auto=format&fit=crop&q=80",
        ["clean-code-robert-martin"] = "https://images.unsplash.com/photo-1532012164546-f432f2e3edd4?w=800&auto=format&fit=crop&q=80",
        ["psychology-of-money-housel"] = "https://images.unsplash.com/photo-1592496431122-2349e0fbc666?w=800&auto=format&fit=crop&q=80",
        ["moleskine-classic-notebook"] = "https://images.unsplash.com/photo-1589829085413-56de8ae18c73?w=800&auto=format&fit=crop&q=80",
        ["parker-sonnet-fountain-pen"] = "https://images.unsplash.com/photo-1583485088034-697b5bc54ccd?w=800&auto=format&fit=crop&q=80",

        // === TECH ACCESSORIES ===
        ["anker-737-power-bank"] = "https://images.unsplash.com/photo-1609592424109-dd9892f1b177?w=800&auto=format&fit=crop&q=80",
        ["samsung-t7-shield-2tb"] = "https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?w=800&auto=format&fit=crop&q=80",

        // === KIDS' CLOTHING ===
        ["kids-organic-cotton-graphic-tee"] = "https://images.unsplash.com/photo-1519238263530-99bdd11df2ea?w=800&auto=format&fit=crop&q=80",
        ["kids-cozy-fleece-pullover-hoodie"] = "https://images.unsplash.com/photo-1503919545889-aef636e10ad4?w=800&auto=format&fit=crop&q=80",
        ["kids-waterproof-lightweight-parka"] = "https://images.unsplash.com/photo-1514090458221-65bb69cf63e6?w=800&auto=format&fit=crop&q=80",
        ["kids-stretch-denim-everyday-jeans"] = "https://images.unsplash.com/photo-1522771930-78848d9293e8?w=800&auto=format&fit=crop&q=80",

        // === ARCHIVED / SOFT DELETED ===
        ["discontinued-iphone-12-case"] = "https://images.unsplash.com/photo-1601593346740-925612772716?w=800&auto=format&fit=crop&q=80",
        ["legacy-running-shoes-2023"] = "https://images.unsplash.com/photo-1515955656352-a1fa3ffcd111?w=800&auto=format&fit=crop&q=80"
    };

    public static async Task EnsureUniqueProductImagesAsync(AppDbContext context)
    {
        var products = await context.Products
            .IgnoreQueryFilters()
            .Select(p => new { p.Id, p.Slug, p.Name })
            .ToListAsync();

        foreach (var product in products)
        {
            if (UniqueProductImages.TryGetValue(product.Slug, out var uniqueUrl))
            {
                // Update existing primary or local images
                await context.ProductImages
                    .Where(pi => pi.ProductId == product.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(pi => pi.ImageUrl, uniqueUrl));

                // Update variant images
                await context.ProductVariants
                    .Where(pv => pv.ProductId == product.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(pv => pv.ImageUrl, uniqueUrl));

                // Check if any product has no image at all, and insert one
                var hasImage = await context.ProductImages.AnyAsync(pi => pi.ProductId == product.Id);
                if (!hasImage)
                {
                    context.ProductImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ImageUrl = uniqueUrl,
                        AltText = product.Name,
                        IsPrimary = true,
                        DisplayOrder = 1
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
