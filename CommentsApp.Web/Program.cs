using CommentsApp.Persistence;
using CommentsApp.Application;
using CommentsApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddPersistence(connectionString);
builder.Services.AddApplication();

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
builder.Services.AddInfrastructure(uploadsPath);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();
