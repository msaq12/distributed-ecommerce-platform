# ===========================
# Stage 1: Build
# ===========================
FROM dockerregistrydevenv.azurecr.io/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["OrderService.Api/OrderService.Api.csproj", "OrderService.Api/"]
COPY ["OrderService.Data/OrderService.Data.csproj", "OrderService.Data/"]

# Restore dependencies
RUN dotnet restore "OrderService.Api/OrderService.Api.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/OrderService.Api"
RUN dotnet build "OrderService.Api.csproj" -c Release -o /app/build

# ===========================
# Stage 2: Publish
# ===========================
FROM build AS publish
RUN dotnet publish "OrderService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===========================
# Stage 3: Runtime
# ===========================
FROM dockerregistrydevenv.azurecr.io/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published files from publish stage
COPY --from=publish /app/publish .

# Expose port 8080 (ASP.NET Core default in container)
EXPOSE 8080

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

# Entry point
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]