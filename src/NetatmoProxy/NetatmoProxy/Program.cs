using Microsoft.Extensions.Caching.Memory;
using NetatmoProxy.Configuration;
using NetatmoProxy.Middleware;
using NetatmoProxy.Services;

namespace NetatmoProxy
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Configuration = builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .Build();

            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddMemoryCache();
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddTransient<ResponseLoggerMiddleware>();
            var authConfig = new AuthConfig();
            Configuration.Bind("NetatmoApi:Auth", authConfig);
            builder.Services.AddSingleton(authConfig);
            builder.Services.AddSingleton<IAccessTokenService>((sp) =>
            {
                return new AccessTokenService(authConfig, sp.GetService<ILogger<AccessTokenService>>(), new HttpClientHandler(), sp.GetService<IMemoryCache>());
            });
            var netatmoApiConfig = new NetatmoApiConfig();
            Configuration.Bind("NetatmoApi", netatmoApiConfig);
            builder.Services.AddSingleton<INetatmoApiService>((sp) =>
            {
                return new NetatmoApiRestService(netatmoApiConfig, sp.GetService<ILogger<NetatmoApiRestService>>(), sp.GetService<IAccessTokenService>(), new HttpClientHandler(), sp.GetService<IMemoryCache>());
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.UseResponseLogger();

            app.MapControllers();

            app.Run();
        }
    }
}