using Application.Interface; // For ICategoryInterface, IProductInterface
using Application.Services; // For CategoryService, ProductService
using Domain; // For AppDbContext
using Domain.Entities;
using Microsoft.AspNetCore.Identity; // For IdentityUser, IdentityRole, etc.
using Microsoft.EntityFrameworkCore; // For DbContext and migrations

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<ICategoryInterface, CategoryService>(); // Register category service
builder.Services.AddScoped<IProductInterface, ProductService>(); // Register product service
builder.Services.AddScoped<IUserInterface, UserService>(); // Register users
builder.Services.AddHttpContextAccessor(); // Add this after AddIdentity and before building the app
builder.Services.AddScoped<ICartInterface, CartService>(); // for the cart
builder.Services.AddScoped<IPaymentInterface, PaymentService>(); // for the payment
// Register cart service
builder.Services.AddSession();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();


// Add MVC services
builder.Services.AddControllersWithViews(); // Handles MVC controllers and views

// Configure DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<UserEntity, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// For access denied
builder.Services.ConfigureApplicationCookie(options =>
{
    // Path for non-authenticated users (unauthorized)
    options.LoginPath = "/Auth/Login";

    // Path for authenticated users denied access to a resource
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// Customize Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true; // Require at least one number
    options.Password.RequiredLength = 6; // Minimum password length
    options.Password.RequireNonAlphanumeric = false; // No special characters required
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Allowed characters
    options.Password.RequireUppercase = false; // No uppercase required
    options.Password.RequireLowercase = false; // No lowercase required
    options.User.RequireUniqueEmail = true; // Enforce unique email
    options.SignIn.RequireConfirmedAccount = false; // Allow sign-in without account confirmation
    options.SignIn.RequireConfirmedEmail = false; // Require email confirmation
    options.SignIn.RequireConfirmedPhoneNumber = false; // No phone confirmation required
});

builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Apply pending migrations automatically (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Apply any pending migrations
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Use detailed error page in development for debugging
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Use custom error page in production
    app.UseHsts(); // Use HSTS in production
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // photos etc
app.UseSession();
app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication(); // Authenticate users
app.UseAuthorization(); // Authorize access

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();

    // Create roles 
    string[] roleNames = { "Admin", "Agent", "NormalUser" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
    // Create Admin user
    var adminEmail = "admin@ims.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new UserEntity
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin",
            Gender = "Female",
            Address = "Admin Office",
            PhoneNumber = "09030198020",
            DateRegistered = DateTime.Now
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            // Confirm email for admin (since RequireConfirmedEmail is true)
            var token = await userManager.GenerateEmailConfirmationTokenAsync(adminUser);
            await userManager.ConfirmEmailAsync(adminUser, token);
        }
    }
}

app.Run();
