using Microsoft.Extensions.DependencyInjection.Extensions;
using SpeechAPI.Interfaces;
using SpeechAPI.Services;
using SpeechAPI.Utils.ConfigOptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200") // Replace with your Angular app's origin
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.Configure<GCSConfigOptions>(builder.Configuration);
builder.Services.TryAddScoped<ISpeechService, SpeechService>();


//builder.Services.TryAddScoped<IGCSUploaderService, GCSUploaderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowAngular");

app.MapControllers();

app.Run();
