using QcpTask.Core.HostedServices;
using QcpTask.Core.Hubs;

namespace QcpTask.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            var testkey = builder.Configuration.GetValue<string>("testkey");

            Console.WriteLine($"TESTKEY: =========== {testkey}");

            var signalrConnString = builder.Configuration.GetValue<string>("signalrConnString");

            builder.Services.AddSignalR().AddAzureSignalR(signalrConnString);

            builder.Services.AddHostedService<TwitterChatService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chat");
            });

            app.Run();
        }
    }
}