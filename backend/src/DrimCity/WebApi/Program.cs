using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WebApi.Common.Endpoints;
using WebApi.Common.Errors;
using WebApi.Common.Pipeline.Behaviors;
using WebApi.Database;

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

app.Run();

public partial class Program {}
