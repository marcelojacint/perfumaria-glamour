FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Glamour.Web/Glamour.Web.csproj", "src/Glamour.Web/"]
COPY ["src/Glamour.Application/Glamour.Application.csproj", "src/Glamour.Application/"]
COPY ["src/Glamour.Domain/Glamour.Domain.csproj", "src/Glamour.Domain/"]
COPY ["src/Glamour.Infrastructure/Glamour.Infrastructure.csproj", "src/Glamour.Infrastructure/"]
COPY ["src/Glamour.Shared/Glamour.Shared.csproj", "src/Glamour.Shared/"]
RUN dotnet restore "src/Glamour.Web/Glamour.Web.csproj"

COPY . .
WORKDIR "/src/src/Glamour.Web"
RUN dotnet build "Glamour.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Glamour.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Glamour.Web.dll"]
