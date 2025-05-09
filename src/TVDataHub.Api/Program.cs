using Microsoft.EntityFrameworkCore;
using TVDataHub.Api.Jobs;
using TVDataHub.Application;
using TVDataHub.DataAccess;
using TVDataHub.Scraper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddUseCases();

builder.Services.AddScraper(builder.Configuration);

builder.Services.AddHostedService<SyncNewTVShowsJob>();
builder.Services.AddHostedService<SyncTVShowsCastJob>();
builder.Services.AddHostedService<SyncUpdatedTVShowsJob>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "TVDataHub API");
     c.RoutePrefix = string.Empty;
});

using (var scope = app.Services.CreateScope())
{
     var db = scope.ServiceProvider.GetRequiredService<TVDataHubContext>();
     db.Database.Migrate();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();