using Newtonsoft.Json;
using RepairCafe.Shared.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using RepairCafe.Api.Extensions;
using RepairCafe.Api.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddOptions<MvcNewtonsoftJsonOptions>()
    .Configure<IServiceProvider>((options, sp) =>
    {
        var registeredSettings = sp.GetRequiredService<JsonSerializerSettings>();
        options.SerializerSettings.SerializationBinder = registeredSettings.SerializationBinder;
        options.SerializerSettings.TypeNameHandling = registeredSettings.TypeNameHandling;
    });

//    Example:
//    builder.Services.AddRepairRequestsModule(builder.Configuration);

builder.Services.AddSharedInfrastructure(builder.Configuration, ModuleRegistry.DomainAssemblies);

builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssemblies(ModuleRegistry.ApplicationAssemblies.ToArray());
});

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();