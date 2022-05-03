using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FPT_Book.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using FPT_Book.Email;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserContextConnection")));

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<UserContext>();



//Configure a bit on Identity Option
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true; //Password bắt buộc có số ?
    options.Password.RequireLowercase = true; //Pass bắt buộc có chữ thường
    options.Password.RequireNonAlphanumeric = true; //Tương tự, tự google
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;    //Độ dài ít nhất cũng phải >= 6 kí tự.

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._@";
    options.User.RequireUniqueEmail = false;
});

//Configure a bit on Cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);  //thời gian cookie hiệu lực
    options.SlidingExpiration = true;
});


// Add services to the container.
builder.Services.AddControllersWithViews();

var mvcBuilder = builder.Services.AddRazorPages();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

// Add EmailSender as a service 

var config = builder.Configuration;
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<EmailSenderOptions>(options =>
{
    options.Host = config["MailSettings:Host"];
    options.Port = int.Parse(config["MailSettings:Port"]);
    options.User = config["MailSettings:User"];
    options.Pass = config["MailSettings:Pass"];
    options.Name = config["MailSettings:Name"];
    options.Sender = config["MailSettings:User"];
});


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = { "Customer", "Seller","Admin" };
    IdentityResult roleResult;
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();;

app.UseAuthorization();

app.UseEndpoints(endpoints =>
 {
     endpoints.MapControllerRoute(
       name: "areas",
       pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
     );

     app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

 });
app.MapRazorPages();
app.Run();
