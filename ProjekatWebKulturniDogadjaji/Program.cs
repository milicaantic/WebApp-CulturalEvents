using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjekatWebKulturniDogadjaji.Data;
using ProjekatWebKulturniDogadjaji.Models;
using ProjekatWebKulturniDogadjaji.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
          ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false  ;
})
.AddRoles<IdentityRole>() 
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddHttpClient<WeatherService>();



builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

 
    string[] roles = new[] { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@example.com";
    var adminPassword = "Admin123!";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Administrator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword); 
        if (!result.Succeeded)
        {
            throw new Exception("Ne mogu da kreiram admin korisnika: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
    else
    {
       
        var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
        await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
        adminUser.EmailConfirmed = true;
        await userManager.UpdateAsync(adminUser);
    }

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();
