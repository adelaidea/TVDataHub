using Microsoft.EntityFrameworkCore;
using TVDataHub.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccess(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
     var db = scope.ServiceProvider.GetRequiredService<TVDataHubContext>();
     db.Database.Migrate();
}

app.Run();