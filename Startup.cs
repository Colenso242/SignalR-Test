using TestSignalR.Hubs;

namespace TestSignalR;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSignalR();
    }
    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        if(!env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseStaticFiles();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<OrderNotificationHub>("/orderNotification");
        });
    }
}