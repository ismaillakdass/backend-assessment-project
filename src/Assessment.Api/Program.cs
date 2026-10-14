using System.Reflection;
using Asp.Versioning;
using Assessment.Api.Middleware;
using Assessment.Api.Swagger;
using Assessment.Application;
using Assessment.Infrastructure;
using Assessment.Infrastructure.Persistence.Seed;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger & OpenAPI
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Register Application & Infrastructure Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Global Exception Handling (TASK 9)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Token Revocation check (TASK 11)
app.UseMiddleware<TokenRevocationMiddleware>();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment() || true) // Enabled for assessment testing
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
        options.RoutePrefix = string.Empty; // Swagger UI served at application root (http://localhost:5000/)
    });
}

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Automatic DB Creation & Seeding on Startup
try
{
    Log.Information("Applying migrations and seeding database...");
    await DatabaseSeeder.SeedAsync(app.Services);
    Log.Information("Database setup completed successfully.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred while seeding the database.");
}

app.Run();
