using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DAOs.Implements.DashboardDAO;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Implements.DashboardRepo;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();


// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key) || Encoding.UTF8.GetBytes(key).Length < 16)
        {
            throw new InvalidOperationException("JWT Key must be at least 128 bits (16 bytes) long");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key))
        };

        // Add this block to customize responses
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                // Skip the default logic.
                context.HandleResponse();
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize("This action is not authorized.");
                    return context.Response.WriteAsync(result);
                }
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize("You don't have permission to perform this action.");
                return context.Response.WriteAsync(result);
            },
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize("Authentication failed. Invalid or expired token.");
                return context.Response.WriteAsync(result);
            }
        };
    });

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HIV System API", Version = "v1" });

    // Add JWT Authentication
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
            Array.Empty<string>()
        }
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add DB Context
var connectionString = builder.Configuration.GetConnectionString("HIVSystemDatabase");
builder.Services.AddDbContext<HivSystemApiContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
    });
}, ServiceLifetime.Scoped);

// Add memory caching
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limit cache size
});

// Add response caching
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// Register DAOs
builder.Services.AddScoped<BaseDashboardDAO>();
builder.Services.AddScoped<AdminDashboardDAO>();
builder.Services.AddScoped<DoctorDashboardDAO>();
builder.Services.AddScoped<PatientDashboardDAO>();
builder.Services.AddScoped<StaffDashboardDAO>();
builder.Services.AddScoped<ManagerDashboardDAO>();

// Register Repositories
builder.Services.AddScoped<BaseDashboardRepo>();
builder.Services.AddScoped<AdminDashboardRepo>();
builder.Services.AddScoped<DoctorDashboardRepo>();
builder.Services.AddScoped<PatientDashboardRepo>();
builder.Services.AddScoped<StaffDashboardRepo>();
builder.Services.AddScoped<ManagerDashboardRepo>();

// Add Services
builder.Services.AddScoped<IArvMedicationDetailService, ArvMedicationDetailService>();
builder.Services.AddScoped<HIV_System_API_Services.Interfaces.IAccountService, HIV_System_API_Services.Implements.AccountService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IDoctorWorkScheduleService, DoctorWorkScheduleService>();
builder.Services.AddScoped<IPatientMedicalRecordService, PatientMedicalRecordService>();
builder.Services.AddScoped<IPatientArvRegimenService, PatientArvRegimenService>();
builder.Services.AddScoped<IPatientArvMedicationService, PatientArvMedicationService>();
builder.Services.AddScoped<IMedicalServiceService, MedicalServiceService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IVerificationCodeService, VerificationCodeService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpContextAccessor(); // Add this for user claims access
builder.Services.AddScoped<IRegimenTemplateService, RegimenTemplateService>();
builder.Services.AddScoped<IPatientArvRegimenService, PatientArvRegimenService>();
builder.Services.AddScoped<ISocialBlogService, SocialBlogService>();
builder.Services.AddScoped<IBlogReactionService, BlogReactionService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IBlogImageDAO, BlogImageDAO>();
builder.Services.AddScoped<IBlogImageRepo, BlogImageRepo>();
builder.Services.AddScoped<IBlogImageService, BlogImageService>();
// Add CORS policy
// In Program.cs or Startup.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// And in your middleware configuration:

var app = builder.Build();
var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
StripeConfiguration.ApiKey = stripeSecretKey;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add Authentication & Authorization middleware in the pipeline
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAll");
app.MapControllers();
app.UseStaticFiles();
app.Run();
