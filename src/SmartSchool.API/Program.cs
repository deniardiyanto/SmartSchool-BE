using SmartSchool.Application;
using SmartSchool.Infrastructure.DependencyInjection;
using SmartSchool.Infrastructure.Persistence.Context;
using SmartSchool.Infrastructure.Seed;
using SmartSchool.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using SmartSchool.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//testing cors
builder.Services.AddCors(options =>
{
    var allowedOrigins =
        builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

//testing cors
app.UseCors("Frontend");

app.UseSwagger();

app.UseSwaggerUI();
app.UseGlobalException();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<SmartSchoolDbContext>();

    await context.Database.MigrateAsync();

    var passwordHasher = services.GetRequiredService<IPasswordHasher>();

    await DataSeeder.SeedAsync(context, passwordHasher);
}

app.Run();
