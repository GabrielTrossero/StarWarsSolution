using Microsoft.EntityFrameworkCore;
using MoviesApp.Application.Interfaces;
using MoviesApp.Application.Services;
using MoviesApp.Domain.Interfaces;
using MoviesApp.Infrastructure.Persistence;
using MoviesApp.Infrastructure.Persistence.Repositories;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Add Dependency Injection
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IMovieExternalService, MovieExternalService>();

// Config Serilog for logs from appsettings.json
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddDbContext<StarWarsDbContext>(options =>
    options.UseInMemoryDatabase("StarWarsDb"));

// Config HttpClient
builder.Services.AddHttpClient<MovieExternalService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();