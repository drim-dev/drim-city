using System.Reflection;
using Common.Web.Endpoints;
using Common.Web.Errors;
using Common.Web.Validation.Behaviors;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Features.Accounts.Extensions;
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

builder.AddAccounts();

var app = builder.Build();

app.MapProblemDetails();

app.MapEndpoints();

await app.MigrateDatabase();

app.Run();

namespace DrimCity.WebApi
{
    public partial class Program {}
}
