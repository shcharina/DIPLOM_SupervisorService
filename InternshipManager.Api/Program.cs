using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System.Text.Json.Serialization;

using InternshipManager.Api.Data;
using InternshipManager.Api.Services;
using InternshipManager.Api.Services.Interfaces;
using InternshipManager.Api.Repositories;
using InternshipManager.Api.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Подключение к PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Репозитории
builder.Services.AddScoped<ISupervisorApplicationRepository,
    SupervisorApplicationRepository>();
builder.Services.AddScoped<IInterviewSlotRepository,
    InterviewSlotRepository>();
builder.Services.AddScoped<IStudentSupervisorApplicationRepository,
    StudentSupervisorApplicationRepository>();
builder.Services.AddScoped<IInterviewRepository,
    InterviewRepository>();
builder.Services.AddScoped<ISupervisorReviewRepository,
    SupervisorReviewRepository>();


// Сервисы
builder.Services.AddScoped<ISupervisorApplicationService,
    SupervisorApplicationService>();
builder.Services.AddScoped<IInterviewSlotService,
    InterviewSlotService>();
builder.Services.AddScoped<IStudentSupervisorApplicationService,
    StudentSupervisorApplicationService>();
builder.Services.AddScoped<IInterviewService,
    InterviewService>();
builder.Services.AddScoped<ISupervisorReviewService,
    SupervisorReviewService>();
builder.Services.AddScoped<ISupervisorApplicationStatusService,
    SupervisorApplicationStatusService>();

// Фоновая задача проверки дат
builder.Services.AddHostedService<SupervisorApplicationDeadlineCheckerService>();


builder.Services.AddHttpClient<ManagerApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:ManagerApi"]
        ?? "http://localhost:5001");
        
    client.Timeout = TimeSpan.FromSeconds(10); });

// HTTP клиент для сервиса студента
builder.Services.AddHttpClient<StudentApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:StudentApi"]
        ?? "http://localhost:5003");

    client.Timeout = TimeSpan.FromSeconds(10);
});

//Версионирование API
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });
    
builder.Services.AddEndpointsApiExplorer();

// Swagger с версионированием
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "InternshipManager API",
        Version = "v1"
    });
});

builder.Services.AddMemoryCache();

// CORS для Vue фронта
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVue", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseCors("AllowVue");

app.UseAuthorization();

app.MapControllers();

app.Run();