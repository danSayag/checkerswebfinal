using CheckersWeb.Data;
using CheckersWeb.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<CheckersDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("users")));

// Authentication and Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/Login";
    });

// Authorization is added to ensure that the [Authorize] attributes in controllers and hubs work correctly, enforcing access control based on user authentication status.
builder.Services.AddAuthorization();

// SignalR is added to enable real-time communication between the server and clients, which is essential for the interactive gameplay experience in the checkers game.
builder.Services.AddSignalR();


// ✅ Build the app after all services are configured
var app = builder.Build();

// ✅ Run migrations (OK here)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CheckersDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ✅ Middleware order matters! Authentication and Authorization must be before endpoint mapping.
app.UseHttpsRedirection();
// Static files should be served before routing to ensure that requests
// for static assets are handled correctly without unnecessary processing through the routing middleware.  
app.UseStaticFiles();

// Routing should be configured after static file handling to ensure that requests for static assets are not processed through the routing middleware, which can lead to improved performance and correct handling of static content.
app.UseRouting();

// Authentication and Authorization middleware should be placed after routing but before endpoint mapping to ensure that authentication and authorization checks are performed for the correct endpoints. This allows the application to properly enforce access control based on the defined routes and controllers.
app.UseAuthentication();
app.UseAuthorization();

// ✅ Map endpoints AFTER middleware
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<GameHub>("/gamehub");



app.Run();