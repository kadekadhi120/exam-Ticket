using exam1_Ticket.Services;
using Ticket.Entites;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


var logDirectory = "logs";
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

var logFilePath = Path.Combine("logs", $"Log-{DateTime.UtcNow:yyyyMMdd}.txt");

// Konfigurasi Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()  // Set log level Information
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

// ✅ Gunakan Serilog di `builder.Host`
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//Configure SQL Server
builder.Services.AddEntityFrameworkSqlServer();
builder.Services.AddDbContextPool<AccelokaDbContext>(options =>
{
    var conString = configuration.GetConnectionString("SQLServerDB");
    options.UseSqlServer(conString);
});


builder.Services.AddTransient<TicketService>();


var app = builder.Build();
Log.Information("🚀 Application is starting...");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

Log.Information("🛑 Application is shutting down...");
Log.CloseAndFlush();
