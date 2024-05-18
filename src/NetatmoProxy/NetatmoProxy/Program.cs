using Microsoft.Extensions.Caching.Memory;
using NetatmoProxy.Configuration;
using NetatmoProxy.Middleware;
using NetatmoProxy.Services;
using Prometheus;

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
            builder.Services.AddHttpClient();
            builder.Services.AddMemoryCache();
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddTransient<ResponseLoggerMiddleware>();
            builder.Services.AddTransient<INowService, NowService>();
            builder.Services.AddTransient<IPythonDateTimeFormatService, PythonDateTimeFormatService>();

            // Healthcheck
            builder.Services.AddHealthChecks()
                .AddCheck<HealthCheck>(nameof(HealthCheck))
                .ForwardToPrometheus();
            
            // DayNightService, MetSunriseApi
            if ("MetSunriseApi".Equals(Configuration.GetValue<string>("DayNightService:Type"), StringComparison.InvariantCultureIgnoreCase))
            {
                var metSunriseApiConfig = new MetSunriseApiConfig();
                Configuration.Bind("DayNightService", metSunriseApiConfig);
                builder.Services.AddSingleton<MetSunriseApiConfig>(metSunriseApiConfig);
                builder.Services.AddSingleton<IDayNightService>((sp) =>
                {
                    return new MetSunriseApiService(
                        config: metSunriseApiConfig,
                        logger: sp.GetService<ILogger<MetSunriseApiService>>(),
                        httpClient: sp.GetService<IHttpClientFactory>().CreateClient(MetSunriseApiService.HttpClientName),
                        memCache: sp.GetService<IMemoryCache>(),
                        nowService: sp.GetService<INowService>()
                        );
                });
            }
            else
            {
                builder.Services.AddTransient<IDayNightService, DayNightSimpleService>();
            }

            // NetatmoApi
            var authConfig = new AuthConfig();
            Configuration.Bind("NetatmoApi:Auth", authConfig);
            builder.Services.AddSingleton(authConfig);
            builder.Services.AddSingleton<IAccessTokenService>((sp) =>
            {
                return new AccessTokenService(
                    config: authConfig, 
                    logger: sp.GetService<ILogger<AccessTokenService>>(),
                    httpClient: sp.GetService<IHttpClientFactory>().CreateClient(AccessTokenService.HttpClientName),
                    memCache: sp.GetService<IMemoryCache>()
                    );
            });
            var netatmoApiConfig = new NetatmoApiConfig();
            Configuration.Bind("NetatmoApi", netatmoApiConfig);
            builder.Services.AddSingleton<NetatmoApiConfig>(netatmoApiConfig);
            builder.Services.AddSingleton<INetatmoApiService>((sp) =>
            {
                return new NetatmoApiRestService(
                    config: sp.GetService<NetatmoApiConfig>(),
                    logger: sp.GetService<ILogger<NetatmoApiRestService>>(),
                    accessTokenService: sp.GetService<IAccessTokenService>(), 
                    httpClient: sp.GetService<IHttpClientFactory>().CreateClient(NetatmoApiRestService.HttpClientName),
                    memCache: sp.GetService<IMemoryCache>()
                    );
            });

            // Simulation
            var simulationConfig = new SimulationConfig();
            Configuration.Bind("Simulation", simulationConfig);
            builder.Services.AddSingleton<SimulationConfig>(simulationConfig);

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

            app.UseHttpMetrics();
            app.UseAuthorization();
            app.UseResponseLogger();

            app.MapControllers();
            app.MapMetrics();

            app.Run();
        }
    }
}