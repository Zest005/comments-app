using CommentsApp.Application;
using CommentsApp.Infrastructure;
using CommentsApp.Persistence;
using CommentsApp.Web.Hubs;
using CommentsApp.Web.Middleware;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var retries = 10;
    while (retries > 0)
    {
        try
        {
            dbContext.Database.Migrate();
            break;
        }
        catch (Exception)
        {
            retries--;
            if (retries == 0) throw;
            Thread.Sleep(3000);
        }
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAngular");

app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.MapHub<CommentHub>("/hubs/comments");

// SPA fallback — Angular маршруты
app.MapFallbackToFile("index.html");

app.Run();
