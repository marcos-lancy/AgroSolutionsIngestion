# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/AgroSolutions.Ingestion.Service.Api/AgroSolutions.Ingestion.Service.Api.csproj", "src/AgroSolutions.Ingestion.Service.Api/"]
COPY ["src/AgroSolutions.Ingestion.Service.Application/AgroSolutions.Ingestion.Service.Application.csproj", "src/AgroSolutions.Ingestion.Service.Application/"]
COPY ["src/AgroSolutions.Ingestion.Service.Domain/AgroSolutions.Ingestion.Service.Domain.csproj", "src/AgroSolutions.Ingestion.Service.Domain/"]
COPY ["src/AgroSolutions.Ingestion.Service.Infra/AgroSolutions.Ingestion.Service.Infra.csproj", "src/AgroSolutions.Ingestion.Service.Infra/"]

# Restaurar dependências
RUN dotnet restore "src/AgroSolutions.Ingestion.Service.Api/AgroSolutions.Ingestion.Service.Api.csproj"

# Copiar tudo
COPY . .

# Build
WORKDIR "/src/src/AgroSolutions.Ingestion.Service.Api"
RUN dotnet build "AgroSolutions.Ingestion.Service.Api.csproj" -c Release -o /app/build

# Estágio de publicação
FROM build AS publish
RUN dotnet publish "AgroSolutions.Ingestion.Service.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgroSolutions.Ingestion.Service.Api.dll"]
