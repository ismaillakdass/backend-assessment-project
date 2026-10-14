# TASK 14: Dockerfile (2 Points)
# NOTE: This Dockerfile has intentional issues:
# - Check the COPY paths for the multi-project solution.
# - SQLite database file is written inside the container without a persistent volume mount.
# - Verify exposed ports and ASPNETCORE_URLS.

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY BackendAssessment.sln .
COPY src/Assessment.Domain/Assessment.Domain.csproj src/Assessment.Domain/
COPY src/Assessment.Application/Assessment.Application.csproj src/Assessment.Application/
COPY src/Assessment.Infrastructure/Assessment.Infrastructure.csproj src/Assessment.Infrastructure/
COPY src/Assessment.Api/Assessment.Api.csproj src/Assessment.Api/
COPY tests/Assessment.UnitTests/Assessment.UnitTests.csproj tests/Assessment.UnitTests/

RUN dotnet restore BackendAssessment.sln

# Copy remaining source code
COPY . .

# Build and publish API
# FLAW [Task 14]: Candidate should ensure output path and configuration are properly set
WORKDIR /app/src/Assessment.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# FLAW [Task 14]: SQLite database file needs appropriate directory permissions and volume mount in docker-compose
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "Assessment.Api.dll"]
