FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PersonalAssistant.slnx ./
COPY src/PersonalAssistant.Domain/PersonalAssistant.Domain.csproj src/PersonalAssistant.Domain/
COPY src/PersonalAssistant.Application/PersonalAssistant.Application.csproj src/PersonalAssistant.Application/
COPY src/PersonalAssistant.Infrastructure/PersonalAssistant.Infrastructure.csproj src/PersonalAssistant.Infrastructure/
COPY src/PersonalAssistant.Api/PersonalAssistant.Api.csproj src/PersonalAssistant.Api/
COPY tests/PersonalAssistant.UnitTests/PersonalAssistant.UnitTests.csproj tests/PersonalAssistant.UnitTests/

RUN dotnet restore src/PersonalAssistant.Api/PersonalAssistant.Api.csproj

COPY src/ ./src/
COPY tests/ ./tests/

RUN dotnet publish src/PersonalAssistant.Api/PersonalAssistant.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PersonalAssistant.Api.dll"]
