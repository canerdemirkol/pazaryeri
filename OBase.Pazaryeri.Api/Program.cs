using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OBase.Pazaryeri.Api.Helpers;
using OBase.Pazaryeri.Business.BackgroundJobs;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Constants;
using OBase.Pazaryeri.Business.Middleware;
using OBase.Pazaryeri.Business.Utility;
using OBase.Pazaryeri.Core.Utility;
using OBase.Pazaryeri.DataAccess.Context;
using Serilog;
using Serilog.Events;
using Obase.Log.Extensions;
using System.Reflection;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Api.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<AppSettings>(builder.Configuration.Bind);
builder.Services.Configure<SwaggerSettings>(builder.Configuration.GetSection("SwaggerSettings"));
#region Logger

LogDefinitions logSettings = new();
logSettings = builder.Configuration.GetSection("LogDefinitions").Get<LogDefinitions>()!;
var LogLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), logSettings?.MinLogLevel ?? "Warning");
var loggerConfig = new LoggerConfiguration();

foreach (var item in Constants.CommonConstants.LogFiles)
{
    loggerConfig
   .WriteTo.
   Conditional(
       x =>
       {
           if (!x.Properties.Keys.Contains("LogFolderName"))
               return false;
           var value = x.Properties["LogFolderName"].ToString().Replace('\"', ' ').Trim();
           return value.Equals(item);
       },
       y => y.File(Path.Combine(AppContext.BaseDirectory, $"Logs/{item}/{item}.txt"),
               restrictedToMinimumLevel: LogLevel, outputTemplate: logSettings?.LogOutputTemplate ?? "[{Timestamp:dd.MM.yyyy HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 50 * 1024 * 1024, rollOnFileSizeLimit: true));
}
loggerConfig
   .WriteTo.File(Path.Combine(AppContext.BaseDirectory, $"Logs/GeneralLogs/AllLogs.txt"), restrictedToMinimumLevel: LogEventLevel.Error, outputTemplate: logSettings?.LogOutputTemplate ?? "[{Timestamp:dd.MM.yyyy HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day,
   fileSizeLimitBytes: 50 * 1024 * 1024, rollOnFileSizeLimit: true)

   .WriteTo.
   Conditional(
       x =>
       {
           return x.Properties.Keys.Contains("HttpRequest");
       },
       y => y.File(Path.Combine(AppContext.BaseDirectory, $"Logs/HttpLogs/HttpLog.txt"),
               restrictedToMinimumLevel: LogEventLevel.Information, outputTemplate: logSettings?.LogOutputTemplate ?? "[{Timestamp:dd.MM.yyyy HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 50 * 1024 * 1024, rollOnFileSizeLimit: true))
   .ReadFrom.Configuration(builder.Configuration);
Log.Logger = loggerConfig.CreateLogger();

#endregion

#region Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName.Replace("+", "."));
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "OBase Pazaryeri API", Version = "v1" });
    options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Basic Authentication. Enter username and password"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }

            },
            Array.Empty<string>()
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Endpoint'leri koşullu olarak göstermek için filter ekle
    options.DocumentFilter<SwaggerEndpointFilter>();
});
#endregion

var scheduledTaskConfig = builder.Configuration.GetSection("ScheduledTasks").Get<List<OBase.Pazaryeri.Core.Concrete.BackgroundJob.ScheduledTask>>();
string connectionString = Cipher.DecryptString(builder.Configuration.GetConnectionString("DefaultConnection")!);

builder.Services.AddDbContext<PyDbContext>(options =>
{
options.UseOracle(connectionString, options =>
    options.UseOracleSQLCompatibility("11"));
    // SensitiveDataLogging yalnızca geliştirme ortamında etkinleştirilir
#if DEBUG
    options.EnableSensitiveDataLogging();
#endif
});

var useElasticsearchLogging = builder.Configuration.GetValue<bool>("UseElasticsearchLogging");
if (useElasticsearchLogging)
{
    builder.UseSerilogWithElasticSearch();
}

builder.Services.InjectBusinessServices(builder.Configuration);
builder.Services.ConfigureRecuringJobs();
builder.Services.ConfigureHangfire();

#region Authentication
builder.Services.AddAuthentication("BasicAuthentication")
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options => { }).AddCookie("Identity.Application");
#endregion

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OBase Pazaryeri API v1");
});


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHangFire();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<BearerTokenValidationMiddleware>();
app.UseMiddleware<ExceptionAndLogMiddleware>();
using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    services.Start(scheduledTaskConfig!);
}

ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
Logger.Information($"min worker threads: {workerThreads}, min I/O threads: {completionPortThreads}");
Logger.Information($"application starting...");

app.Run();