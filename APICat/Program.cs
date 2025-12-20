using APICat.Extensions;
using Serilog;
using Microsoft.EntityFrameworkCore;
using APICat.Application.Extensions;
using Microsoft.OpenApi;
using ApiCNV.Extensions;
using APICat.Infraestructure.Contexts;

const string nombreProyecto = "APICat";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build())
    .Enrich.WithProperty("Application", nombreProyecto)
    .CreateLogger();

try
{
    Log.Information(">>>>> {nombreProyecto}: Iniciando la Aplicación", nombreProyecto);

    var builder = WebApplication.CreateBuilder(args);

    // Serilog Config.

    builder.Host.UseSerilog(Log.Logger, dispose: true);

    // Appsettings Envirorment.
    builder.Configuration
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", reloadOnChange: true, optional: true);

    builder.Services.AddDbContext<CatContext>(config => config.UseSqlServer(builder.Configuration.GetConnectionString("CatDBConnection")));


    // Add services to the container.
    builder.Services.AddRepositoriesAndServices();
    builder.RegisterExternalServices();


    // Add Controllers
    builder.Services.AddControllers();

    // NOTA: Si bien la nueva implementación se hace con la siguiente instrucción AddOpenApi(),
    //       prefiero usar la implementación tradicional con swashbuckle, par no perder la configuración personalizada.
    //              -- Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi -- 
    
    //builder.Services.AddOpenApi();

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // Información extra para la documentación de swagger.
            options.SwaggerDoc("v1", new OpenApiInfo()
            {
                Title = nombreProyecto,
                Description = "API General que centraliza las peticiones de acceso a datos del clima",
                Version = "V1"
            });
        });
    }

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        //! Configuración de los endpoints y la url de swagger.
        app.UseSwagger();
        app.UseSwaggerUI(opt =>
        {
            opt.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            //opt.RoutePrefix = "docs";
        });
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.UseSerilogRequestLogging();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, ">>>>> La aplicación generó una excepción al intentar iniciar");
}
finally
{
    Log.CloseAndFlush();
}



