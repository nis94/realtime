FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MovementIntel.Processor/MovementIntel.Processor.csproj", "src/MovementIntel.Processor/"]
COPY ["src/MovementIntel.Common/MovementIntel.Common.csproj", "src/MovementIntel.Common/"]
COPY ["src/MovementIntel.Domain/MovementIntel.Domain.csproj", "src/MovementIntel.Domain/"]
RUN dotnet restore "src/MovementIntel.Processor/MovementIntel.Processor.csproj"
COPY . .
WORKDIR "/src/src/MovementIntel.Processor"
RUN dotnet build "MovementIntel.Processor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MovementIntel.Processor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MovementIntel.Processor.dll"]
