using System.Text;
using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Schulmessenger.Api.ExceptionHandlers;
using Schulmessenger.Api.Hubs;
using Schulmessenger.Application.Interfaces.Db;
using Schulmessenger.Application.Interfaces.Services;
using Schulmessenger.Application.Interfaces.Services.Auth;
using Schulmessenger.Application.Services;
using Schulmessenger.Application.Services.Auth;
using Schulmessenger.Persistence.MSSQL.Db;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? throw new InvalidOperationException("JWT_SECRET ist nicht gesetzt!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = (context) =>
            {
                if (context.Request.Cookies.TryGetValue("jwt", out var cookieToken))
                {
                    context.Token = cookieToken;
                } // Aktuell noch im localStorage und nicht per Cookie

                // SignalR hängt das Token als Query-Parameter an die Hub-URL an
                var path = context.HttpContext.Request.Path;
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policyBuilder => policyBuilder.WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // Browser darf Cookies mitgeben
});

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

builder.Services.AddSignalR()
    .AddJsonProtocol(options => { options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
builder.Services.AddDbContext<SchulmessengerDbContext>(opt => opt.UseSqlServer(connectionString));
builder.Services.AddScoped<ISchulmessengerDbContext>(sp => sp.GetRequiredService<SchulmessengerDbContext>());

builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IClassService, ClassService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILdapService, LdapService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<SchulmessengerHub>("/hubs/schulmessenger");

app.Run();