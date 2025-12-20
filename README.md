# APICat

APICat es una API RESTful desarrollada en **.NET 9**, diseñada bajo los principios de **Clean Architecture**. El proyecto implementa patrones avanzados para el manejo dinámico de bases de datos, intercepción de servicios y seguridad centralizada.

## 🚀 Tecnologías y Características

* **.NET 9**
* **Entity Framework Core 9** (SQL Server)
* **DispatchProxy** (AOP - Programación Orientada a Aspectos)
* **Multi-Tenancy / Multi-DB Support**
* **Serilog** (Logging estructurado)
* **JWT** (Autenticación Segura)
* **Docker** (Contenerización con Dockerfile)

---

## 📂 Estructura del Proyecto y Dependencias

A continuación se detalla la responsabilidad de cada proyecto y los paquetes NuGet utilizados:

### 1. APICat (Web API)
Es el punto de entrada de la aplicación. Aquí se configura la inyección de dependencias, los middlewares y los controladores.
* **Paquetes:**
    * `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.11)
    * `Microsoft.OpenApi` (1.6.25)
    * `Swashbuckle.AspNetCore` (9.0.6)
    * `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` (1.22.1)

### 2. APICat.Application
Contiene la lógica de negocio pura, DTOs y validaciones. No depende de detalles de infraestructura.
* **Paquetes:**
    * `FluentValidation` (12.1.1)
    * `Newtonsoft.Json` (13.0.1)

### 3. APICat.Infrastructure
Capa encargada del acceso a datos, implementaciones de repositorios y comunicación externa.
* **Características Clave:**
    * **DispatchProxy:** Se utiliza para crear proxies dinámicos que interceptan las llamadas a los servicios. Esto permite inyectar lógica transversal (como logging o manejo de errores) sin ensuciar la lógica de negocio.
    * **Resolvers y Multi-DB:** Gracias a la carpeta `Resolvers`, la aplicación es capaz de gestionar **conexiones a múltiples bases de datos**. Los resolvers determinan en tiempo de ejecución qué `DbContext` o cadena de conexión utilizar según el contexto de la petición.
* **Paquetes:**
    * `Microsoft.EntityFrameworkCore` (9.0.11)
    * `Microsoft.EntityFrameworkCore.SqlServer` (9.0.11)
    * `Microsoft.EntityFrameworkCore.Design` (9.0.11)
    * `Microsoft.EntityFrameworkCore.Tools` (9.0.11)
    * `Microsoft.IdentityModel.JsonWebTokens` (8.15.0)

### 4. APICat.Logging
Proyecto transversal dedicado a la configuración y gestión de logs estructurados.
* **Paquetes:**
    * `Serilog` (4.3.0)
    * `Serilog.AspNetCore` (10.0.0)
    * `Serilog.Extensions.Logging` (10.0.0)
    * `Serilog.Settings.Configuration` (10.0.0)
    * `Serilog.Sinks.Debug` (3.0.0)
    * `Serilog.Sinks.File` (7.0.0)

### 5. APICat.Test
Proyecto de pruebas unitarias para asegurar la calidad del código.
* **Paquetes:**
    * `xunit` (2.9.2)
    * `xunit.runner.visualstudio` (2.8.2)
    * `Moq` (4.20.72)
    * `Microsoft.NET.Test.Sdk` (17.12.0)
    * `coverlet.collector` (6.0.2)

---

## 🗄️ Configuración de Base de Datos

El sistema no utiliza migraciones automáticas al inicio, se basa en scripts SQL existentes.

1.  **Restauración de Objetos SQL:**
    * Dirígete a la carpeta que contiene los scripts SQL (ubicada usualmente en la raíz o dentro de `Infrastructure`).
    * Ejecuta los scripts en tu servidor de SQL Server para crear las tablas y procedimientos almacenados necesarios.

2.  **Configuración de Conexión:**
    * Abre el archivo `appsettings.json` (o `appsettings.Development.json`).
    * Localiza la sección de `ConnectionStrings`.
    * Actualiza los valores para que apunten a tu instancia local de SQL Server.

---

## ⚡ Pruebas con Postman y JWT

Para probar la API se incluye una colección de Postman.

1.  **Importar Colección:**
    * Importa el archivo `.json` de la colección provisto en el repositorio.
    * **Importante:** Verifica en las variables de entorno de la colección que la URL apunte al puerto correcto de tu `localhost` (ej. `https://localhost:7155` o el que indique tu consola al iniciar).

2.  **Autenticación:**
    * Las credenciales por defecto son:
        * **Usuario:** `admin`
        * **Password:** `1234`
    * **Automatización:** La colección de Postman ya está configurada para obtener el Token JWT automáticamente. No necesitas loguearte manualmente antes de cada petición; el script de pre-request se encarga de renovar el token.

---

## 🐳 Docker

El proyecto incluye un `Dockerfile` para generar la imagen de la API de forma aislada.

**1. Construir la imagen:**
Ejecuta desde la raíz de la solución:
```bash
docker build -t apicat-image -f APICat/Dockerfile .
```

Desarrollado por: Agustín Gonzalo Rivera - Prueba Técnica Diciembre 2025


