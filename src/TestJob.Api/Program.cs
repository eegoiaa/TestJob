using System.Text.Json;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Npgsql;
using TestJob.Api.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<IProcessingService, ProcessingService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestJob API v1");
    c.RoutePrefix = "api/swagger";
});

app.UseAuthorization();

app.MapControllers();

await InitializeDatabaseAsync(app.Services);

app.Run();

static async Task InitializeDatabaseAsync(IServiceProvider services)
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Postgres");

    if (string.IsNullOrEmpty(connectionString))
    {
        return;
    }

    const string createTableSql = """
        CREATE TABLE IF NOT EXISTS elements (
            id BIGSERIAL PRIMARY KEY,
            attribute_value TEXT NOT NULL,
            html_content TEXT NOT NULL,
            created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
        )
        """;

    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    await connection.ExecuteAsync(createTableSql);
}
