using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ScalableTeaching.Data;
using ScalableTeaching.Helpers;
using ScalableTeaching.OpenNebula;
using ScalableTeaching.Services;
using Serilog;
using ScalableTeaching.Services.HostedServices;
using Serilog.Events;
using static ScalableTeaching.Models.User.UserType;

namespace ScalableTeaching
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "backend", Version = "v1"}); });
            services.AddDbContext<VmDeploymentContext>(options =>
                //Build database connection string from environment
                options.UseLazyLoadingProxies().UseNpgsql(
                    "Host=" + Environment.GetEnvironmentVariable("dbhost") +
                    ";Database=" + Environment.GetEnvironmentVariable("db") +
                    ";Username=" + Environment.GetEnvironmentVariable("dbuser") +
                    ";Password=" + Environment.GetEnvironmentVariable("dbpass")
                )
            );
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false; //TODO: Set to true for production
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(CryptoHelper.Instance.Rsa),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdministratorLevel", policy => policy.RequireClaim("account_type",
                    nameof(Administrator)));
                options.AddPolicy("EducatorLevel", policy => policy.RequireClaim("account_type",
                    nameof(Educator), nameof(Administrator)));
                options.AddPolicy("UserLevel", policy => policy.RequireClaim("account_type",
                    nameof(User), nameof(Educator), nameof(Administrator)));
            });


            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IOpenNebulaAccessor>(
                new OpenNebulaAccessor(Environment.GetEnvironmentVariable("OpenNebulaLocation"),
                    Environment.GetEnvironmentVariable("OpenNebulaSession")));
            services.AddSingleton<MachineConfigurator>();
            services.AddScoped<SshConfigBuilder>();
            services.AddHostedService<MachineControllerService>();
            services.AddSingleton<IDbContextFactory, VmDeploymentContextFactory>();
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.Console()
                .WriteTo.File($"{Environment.GetEnvironmentVariable("ScalableTeachingBaseLocation")}/logs/log-.txt",
                    rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromMinutes(1));
            var ENVSerilogLoggingLevel = Environment.GetEnvironmentVariable("SERILOG_LOGGING_LEVEL");
            if (ENVSerilogLoggingLevel is not null)
            {
                try
                {
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Is(ToLogLevel(ENVSerilogLoggingLevel));
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(
                        $"There was an exception setting the log level in serilog - Message {e.Message}, \nStacktrace: {e.StackTrace}");
                }
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "backend v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();


            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            lifetime.ApplicationStopped.Register(OnShutdown);
        }

        private void OnShutdown()
        {
            Log.CloseAndFlush();
        }

        private static LogEventLevel ToLogLevel(string level) => level switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => throw new ArgumentOutOfRangeException($"Level {level} is not a valid serilog level")
        };
    }
}
