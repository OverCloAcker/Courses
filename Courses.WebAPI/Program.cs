using Courses.WebAPI.ApiVersioning;
using Courses.WebAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddDefaultApiVersioning();

var app = builder.Build();

app.UseDefaultApiVersioning();
app.UseHttpsRedirection();

var apiVersionManager = app.Services.GetRequiredService<ApiVersionManager>();
app.MapWeatherEndpoints(apiVersionManager);

app.Run();