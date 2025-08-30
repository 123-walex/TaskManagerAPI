using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;
using Serilog.Sinks.File;
using TaskManagerAPI.Data;
using TaskManagerAPI.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("EnvironmentName", builder.Environment.EnvironmentName)
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss}{StatusCode}{Level:u3}] {EnvironmentName} {ThreadId} {TraceId} {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: "C:\\Users\\Lenovo\\Documents\\c# developement\\TaskManagerAPI\\Logs.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:HH:mm:ss}{StatusCode}{Level:u3}] {EnvironmentName} {ThreadId} {TraceId} {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TaskManagerDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
                   Encoding.UTF8.GetBytes(builder.Configuration.GetValue<String>("JwtSettings:AccessToken")!)
               ),
               ValidateIssuerSigningKey = true
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
builder.Services.AddScoped<ItokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService_Manual>();
builder.Services.AddScoped<IGoogleService, AuthService_Google>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi("/openapi/v1.json");
}

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
