using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:3000", "http://localhost:5174", "http://localhost:4200") // Allow React frontend
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Required for WebSockets
});

builder.Logging.ClearProviders(); // Remove default providers
builder.Logging.AddConsole(); // Add console logging
builder.Logging.AddDebug();   // Add debug logging
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<StockService>();
builder.Services.AddHostedService<StockUpdateBackgroundService>();

// Add Swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Apply CORS Policy
app.UseCors("AllowReactApp");

//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock API v1");
});
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<StockHub>("/stockHub");
});

app.Run();