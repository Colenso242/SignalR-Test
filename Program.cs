using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using TestSignalR.Hubs;
using TestSignalR.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// enum come stringhe ("Shipped") sia per le API che per SignalR, così il JS può usare i nomi
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddRazorPages();
builder.Services.AddSignalR()
    .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<IOrderService,OrderService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
        o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// niente default per controller: "/" va alla Razor Page Pages/Index, la vista MVC resta su /Home
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();
app.UseStaticFiles();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<OrderNotificationHub>("/orderNotification");
});


app.Run();
