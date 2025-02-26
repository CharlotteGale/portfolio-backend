using portfolio_backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
//builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddScoped<GmailOAuth>();

builder.Services.AddScoped<GmailService>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://www.charlottegale.dev",
            "https://charlottegale.dev");
            //"http://localhost:5173");
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();

        Console.WriteLine("CORS policy configured with specific origins");
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request from origin: {context.Request.Headers["Origin"]}");
    await next.Invoke();
    Console.WriteLine($"Response status code: {context.Response.StatusCode}");
    if (context.Response.StatusCode == 500)
    {
        Console.WriteLine("500 error occurred");
    }
});

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
