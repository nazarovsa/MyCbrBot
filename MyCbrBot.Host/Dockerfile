FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MyCbrBot.Host/MyCbrBot.Host.csproj", "MyCbrBot.Host/"]
COPY ["MyCbrBot.Domain/MyCbrBot.Domain.csproj", "MyCbrBot.Domain/"]
COPY ["MyCbrBot.Core/MyCbrBot.Core.csproj", "MyCbrBot.Core/"]
RUN dotnet restore "MyCbrBot.Host/MyCbrBot.Host.csproj"
COPY . .
WORKDIR "/src/MyCbrBot.Host"
RUN dotnet build "MyCbrBot.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyCbrBot.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyCbrBot.Host.dll"]
