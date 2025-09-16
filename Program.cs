using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using Scalar.AspNetCore;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.File;
using System.Security.Claims;
using System.Text;
using TaskManagerAPI.Data;
using TaskManagerAPI.Entities;
using TaskManagerAPI.Services;
using TaskManagerAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("EnvironmentName", builder.Environment.EnvironmentName)
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss}{StatusCode}{Level:u3}] {EnvironmentName} {ThreadId} {TraceId} {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
         new JsonFormatter(renderMessage: true, closingDelimiter: null),
        path: "C:\\Users\\Lenovo\\Documents\\c# developement\\TaskManagerAPI\\Logs\\Logs.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<TaskManagerDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
       //the nextline is to split long queries so efcore can run with sql faster 
       b => b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    )
);
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            SchemaName = "Hangfire" // optional schema
        })
);
builder.Services.AddHangfireServer();


builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Automatically register all validators in the assembly
builder.Services.AddValidatorsFromAssemblyContaining<ManualLoginValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
               ValidateAudience = true,
               ValidAudience = builder.Configuration["JwtSettings:Audience"],
               ValidateLifetime = true,
               IssuerSigningKey = new SymmetricSecurityKey
               (
                   Encoding.UTF8.GetBytes(builder.Configuration.GetValue<String>("JwtSettings:Key")!)
               ),
               ValidateIssuerSigningKey = true,
               RoleClaimType = ClaimTypes.Role,
               NameClaimType = ClaimTypes.NameIdentifier 
           };

       }).AddCookie()
       .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
       {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/signin-google";
       });

builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddScoped<ItokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.MapScalarApiReference();
app.MapOpenApi("/openapi/v1.json");

app.UseStaticFiles();

app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
    {
        diagCtx.Set("TraceId", httpCtx.TraceIdentifier);
        diagCtx.Set("RequestHost", httpCtx.Request.Host.Value);
        diagCtx.Set("RequestPath", httpCtx.Request.Path);
        diagCtx.Set("StatusCode", httpCtx.Response.StatusCode);
    };
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
