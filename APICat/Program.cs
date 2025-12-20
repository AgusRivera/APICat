using ApiCat.Extensions;
using APICat.Application.Extensions;
using APICat.Extensions;
using APICat.Infraestructure.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

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

    builder.Services.AddScoped<DbContext, CatContext>();

    // Add services to the container.
    builder.Services.AddRepositoriesAndServices();
    builder.RegisterExternalServices();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

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

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingrese su token JWT."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
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



