using Lemax.HotelSearch.Application.Abstractions;
using Lemax.HotelSearch.Application.Services;
using Lemax.HotelSearch.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IHotelRepository, InMemoryHotelRepository>();
builder.Services.AddScoped<IHotelService, HotelService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();