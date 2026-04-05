using Microsoft.EntityFrameworkCore;
using InternshipManager.Api.Data;
using InternshipManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация SharedDbContext (общая БД)
builder.Services.AddDbContext<SharedDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SharedConnection")));

// Регистрация SupervisorDbContext (БД руководителя)
builder.Services.AddDbContext<SupervisorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupervisorConnection")));

// Регистрация сервисов
builder.Services.AddScoped<ISupervisorApplicationService, SupervisorApplicationService>();

// CORS для Vue.js
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowVueFrontend");
app.UseAuthorization();
app.MapControllers();

// Автоматическое применение миграций при запуске
using (var scope = app.Services.CreateScope())
{
    var sharedDb = scope.ServiceProvider.GetRequiredService<SharedDbContext>();
    var supervisorDb = scope.ServiceProvider.GetRequiredService<SupervisorDbContext>();
    
    sharedDb.Database.Migrate();
    supervisorDb.Database.Migrate();
}

app.Run();