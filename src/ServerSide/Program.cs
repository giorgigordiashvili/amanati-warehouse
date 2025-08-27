using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using ServerSide.Hubs;
using ServerSide.Infrastructure.ParcelService;
using ServerSide.OptionsModels;
using System.Security.Claims;

namespace ServerSide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer("Bearer", o =>
                {
                    o.SecurityTokenValidators.Clear();
                    //o.SecurityTokenValidators.Add(new NameTokenValidator());
                    o.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/communicationHub")) // Ensure that this path is the same as yours!
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };

                });
            builder.Services.AddAuthorization(o =>
            {
                o.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireClaim(ClaimTypes.Name)
                    .Build();
            });

            builder.Services.AddMemoryCache();
            builder.Services.AddSignalR();

            builder.Services.AddOptions();
            builder.Services.Configure<ParcelOptions>(options => builder.Configuration.GetSection(nameof(ParcelOptions)).Bind(options));

            //builder.Host.UseSerilog((ctx, lc) =>lc
            //.WriteTo.Console()
            //.WriteTo.File(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Log\\ParcelManagement_.log", rollingInterval: RollingInterval.Day)
            //);

            builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

            builder.Services.AddHttpClient<IParcelRestClient, ParcelRestClient>()
                    .ConfigureHttpClient(configure =>
                    {
                        configure.BaseAddress = new Uri(builder.Configuration["ParcelOptions:BaseURL"]);
                    });

            builder.Services.AddHttpContextAccessor();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthorization();
            //app.UseAuthentication();

            app.MapHub<CommunicationHub>("/CommunicationHub");

            //app.MapHub<CommunicationHub>("/CommunicationHub", options =>
            //{
            //    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
            //});

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}