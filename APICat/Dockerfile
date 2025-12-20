# ETAPA 1: Build (Compilación)
# Usamos la imagen del SDK para compilar el código
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Archivo de proyecto (csproj) y restauramos dependencias
COPY ["APICat/APICat.csproj", "APICat/"]
RUN dotnet restore "APICat/APICat.csproj"

# Copiamos el resto del código fuente
COPY . .
WORKDIR "/src/APICat"

# Compilamos y publicamos
RUN dotnet build "APICat.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "APICat.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ETAPA 2: Runtime (Ejecución)
# Se utiliza imagen de ASP.NET Core (más ligera) para correr la app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy Archivos publicados desde la etapa de build
COPY --from=build /app/publish .

# Definimos el punto de entrada
ENTRYPOINT ["dotnet", "APICat.dll"]