var builder = WebApplication.CreateBuilder(args);

// Adds MVC Controllers and Views support.
builder.Services.AddControllersWithViews();

// Required for Session to work. Session stores who is logged in (UserId, Role) in memory.
builder.Services.AddDistributedMemoryCache();

// Configures Session - 30 minute timeout
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});


builder.Services.AddHttpClient("RaceDayApi", c =>
{
    c.BaseAddress = new Uri("https://localhost:7001/");
});

var app = builder.Build();

// Shows the Error page instead of a crash screen if something goes wrong in production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.UseSession();
app.UseAuthorization();


app.MapControllerRoute(
name: "default",
pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();