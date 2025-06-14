using HIV_System_API_BOs;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add DB Context
builder.Services.AddDbContext<HivSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HIVSystemDatabase")));

// Add Services
builder.Services.AddScoped<IArvMedicationDetailService, ArvMedicationDetailService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IDoctorWorkScheduleService, DoctorWorkScheduleService>();
builder.Services.AddScoped<IPatientMedicalRecordService, PatientMedicalRecordService>();
builder.Services.AddScoped<IPatientArvRegimenService, PatientArvRegimenService>();
builder.Services.AddScoped<IPatientArvMedicationService, PatientArvMedicationService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500") // No trailing slash
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowReactApp");
app.MapControllers();

app.Run();
