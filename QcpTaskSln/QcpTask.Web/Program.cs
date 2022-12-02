using Microsoft.AspNetCore.Diagnostics;
using QcpTask.Core.HostedServices;
using QcpTask.Core.Hubs;
using static System.Net.Mime.MediaTypeNames;

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

            builder.Services.AddHostedService<HostedService>();


            var app = builder.Build();

            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    // using static System.Net.Mime.MediaTypeNames;
                    context.Response.ContentType = Text.Plain;

                    await context.Response.WriteAsync("An exception was thrown.");

                    var exceptionHandlerPathFeature =
                        context.Features.Get<IExceptionHandlerPathFeature>();

                    if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
                    {
                        await context.Response.WriteAsync(" The file was not found.");
                    }

                    if (exceptionHandlerPathFeature?.Path == "/")
                    {
                        await context.Response.WriteAsync(" Page: Home.");
                    }

                    await context.Response.WriteAsync(exceptionHandlerPathFeature.Error?.Message + exceptionHandlerPathFeature.Error?.StackTrace);
                });
            });


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