using Microsoft.EntityFrameworkCore;
using NoteApp.Business.DTOs;
using NoteApp.Business.DTOs.Tag;
using NoteApp.Business.DTOs.NoteShare;
using NoteApp.Business.Services;
using NoteApp.Business.Validators;
using NoteApp.Business.Validators.NoteShare;
using NoteApp.DataAccess.Contexts;
using NoteApp.DataAccess.Repositories;
using NoteApp.DataAccess.UnitOfWork;
using FluentValidation;
using FluentValidation.AspNetCore;
using AutoMapper;
using NoteApp.Business.Mappings;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using NoteApp.WebApi.Hubs;
using NoteApp.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:8081")

              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Dosya yükleme limitini artır
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NoteApp API",
        Version = "v1",
        Description = "NoteApp API Documentation"
    });

    // JWT Authentication için Swagger yapılandırması
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//  DbContext eklemesi
builder.Services.AddDbContext<NoteAppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//  Repositories eklemesi
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

//  UnitOfWork eklemesi
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services eklemesi
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<FolderService>();
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<NoteImageService>();
builder.Services.AddScoped<ReminderService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<IAudioService, AudioService>();
builder.Services.AddScoped<NoteShareService>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Validators eklenmesi
builder.Services.AddScoped<IValidator<UserCreateDTO>, UserCreateDTOValidator>();
builder.Services.AddScoped<IValidator<UserUpdateDTO>, UserUpdateDTOValidator>();
builder.Services.AddScoped<IValidator<FolderCreateDTO>, FolderCreateDTOValidator>();
builder.Services.AddScoped<IValidator<FolderUpdateDTO>, FolderUpdateDTOValidator>();
builder.Services.AddScoped<IValidator<NoteCreateDTO>, NoteCreateDTOValidator>();
builder.Services.AddScoped<IValidator<NoteUpdateDTO>, NoteUpdateDTOValidator>();
builder.Services.AddScoped<IValidator<NoteImageCreateDTO>, NoteImageCreateDTOValidator>();
builder.Services.AddScoped<IValidator<NoteImageUpdateDTO>, NoteImageUpdateDTOValidator>();
builder.Services.AddScoped<IValidator<ReminderCreateDTO>, ReminderCreateDTOValidator>();
builder.Services.AddScoped<IValidator<ReminderUpdateDTO>, ReminderUpdateDTOValidator>();
builder.Services.AddScoped<IValidator<AudioCreateDTO>, AudioCreateDTOValidator>();
builder.Services.AddScoped<IValidator<AudioUpdateDTO>, AudioUpdateDTOValidator>();
builder.Services.AddScoped<IValidator<NoteShareCreateDTO>, NoteShareCreateDTOValidator>();
builder.Services.AddScoped<IValidator<NoteShareUpdateDTO>, NoteShareUpdateDTOValidator>();
builder.Services.AddScoped<IValidator<UserCreateDTO>, UserCreateDTOValidator>();

//FluentValidation eklenmesi
builder.Services.AddFluentValidationAutoValidation();

// AutoMapper eklenmesi
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// TokenHelper servisini ekle
builder.Services.AddScoped<TokenHelper>();

// JWT Authentication ekle
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Önce SecretKey'i bir değişkene alalım
        var secretKey = builder.Configuration["JwtSettings:SecretKey"];

        // Değişkenin null veya boş olup olmadığını kontrol edelim
        if (string.IsNullOrEmpty(secretKey))
        {
            // Eğer null ise, programın başlamasını engelleyecek bir hata fırlatalım.
            throw new InvalidOperationException("JWT SecretKey ayarı appsettings.json dosyasında bulunamadı veya boş.");
        }
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        // SignalR için JWT token doğrulama
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/noteHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

// Veritabanını oluştur
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NoteAppDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NoteApp API V1");
        c.RoutePrefix = string.Empty; // Swagger'ı kök URL'de göster
        c.ConfigObject.DisplayRequestDuration = true;
        c.ConfigObject.DocExpansion = Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None;
    });
    app.UseDeveloperExceptionPage();
}

// Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


// Use CORS
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NoteHub>("/noteHub");

// Global hata yakalama middleware'i
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        var errorMessage = $"An error occurred: {ex.Message}";
        if (ex.InnerException != null)
        {
            errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
        }
        await context.Response.WriteAsync(errorMessage);
    }
});

app.UseStaticFiles();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
