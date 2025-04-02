using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
             option =>
             {
                 option.LoginPath = "/Comptes/Login";
                 option.ExpireTimeSpan = TimeSpan.FromMinutes(200);
                 option.AccessDeniedPath = "/Home/ErrorHandler/403";
             }
         );

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential 
    // cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.ConsentCookieValue = "true";
});

builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePagesWithReExecute("/Home/ErrorHandler/{0}");
    app.UseExceptionHandler("/Home/Error");
}

   

app.UseStaticFiles();

app.UseRouting();

app.UseCookiePolicy();

app.UseAuthentication();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
