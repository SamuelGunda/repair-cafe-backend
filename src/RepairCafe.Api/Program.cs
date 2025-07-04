using RepairCafe.Shared.Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSharedInfrastructure();

var assemblies = Directory.GetFiles(AppContext.BaseDirectory, "RepairCafe.*.Application.dll")
    .Select(Assembly.LoadFrom)
    .ToArray();

builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssemblies(assemblies);
});

var app = builder.Build();

app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();