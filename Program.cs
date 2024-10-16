using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ServerAPI.DAL;
using ServerAPI.Filters;
using ServerAPI.Interfaces;
using ServerAPI.Repositories;
using ServerAPI.Services;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Generate a new key at startup
string GenerateKey(int length = 32)
{
    var key = new byte[length];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(key);
    }

    return Convert.ToBase64String(key);
}

var key = GenerateKey(); // Generate a new key each time the application starts

// Configure JWT authentication with the new key
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(key))
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("OnAuthenticationFailed: " + context.Exception + context.Exception.InnerException);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine("OnMessageReceived: " + context.Token);
            return Task.CompletedTask;
        }
    };
});

// Add configuration support
var configuration = builder.Configuration;
builder.Logging.ClearProviders(); // Optionally clear default providers
builder.Logging.AddConsole(); // Add console logging
builder.Logging.AddDebug();   // Optionally add debug logging
builder.Logging.SetMinimumLevel(LogLevel.Warning); // Set the default log level to Warning


// If in development, add User Secrets
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
else
{
    builder.Configuration.AddEnvironmentVariables();
}

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IKeyService>(new KeyService(key));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000",
                                "http://localhost:4200",
                                "http://localhost:8081",
                                "https://localhost:44305")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

builder.Services.AddControllersWithViews();

var path = Path.Combine(Environment.CurrentDirectory, "App_Data");
var dbPath = Path.Combine(path, "Personal.db");

builder.Services.AddDbContext<PersonalContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<IAlbumValidator, AlbumValidator>();
builder.Services.AddScoped<IAlbumHelperService, AlbumHelperService>();
builder.Services.AddScoped<IAlbumsRepository, AlbumsRepository>();
builder.Services.AddScoped<IPhotosRepository, PhotosRepository>();
builder.Services.AddScoped<IAlbumsService, AlbumsService>();
builder.Services.AddScoped<IPhotosService, PhotosService>();
builder.Services.AddScoped<IPhotoDetailsService, PhotoDetailsService>();
builder.Services.AddScoped<IRandomHandlerService, RandomHandlerService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServerAPI", Version = "v1" });
    c.DocumentFilter<CustomDocumentFilter>();  // Register the custom document filter

    // Add the JWT bearer token authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                new string[] { }
            }
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
var envIsDevelopment = app.Environment.IsDevelopment();
if (envIsDevelopment)
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCors("AllowClientApp");
app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();

app.Use(async (context, next) =>
{
    // Check if the request path does not start with "/api"
    if (context.Request.Path.Equals("/"))
    {
        await context.Response.WriteAsync("API ready to receive requests!");
        return; // Stop further processing
    }

    // For API routes or other routes, proceed as usual
    var sessionId = context.Session.Id;
    Console.WriteLine($"Session ID: {sessionId}");
    await next();
});


app.MapControllerRoute(
    name: "randomhandler",
    pattern: "RandomHandler/{action}/{arg1}/{arg2}",
    defaults: new { controller = "RandomHandler" });


app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action}/{id?}");



app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServerAPI V1");
    c.RoutePrefix = "swagger";
});

app.Run();
