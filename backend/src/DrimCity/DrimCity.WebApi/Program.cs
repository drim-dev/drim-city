using System.Reflection;
using DrimCity.WebApi.Common.Endpoints;
using DrimCity.WebApi.Common.Errors;
using DrimCity.WebApi.Common.Pipeline.Behaviors;
using DrimCity.WebApi.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AppDbContext")));

builder.Services.AddProblemDetails();

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapProblemDetails();

app.MapEndpoints();

await app.MigrateDatabase();

app.Run();

public partial class Program {}
