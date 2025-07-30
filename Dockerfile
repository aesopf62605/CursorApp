# Multi-stage build for .NET 8 Invoice Calculation API on Linux
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["InvoiceApi/InvoiceApi.csproj", "InvoiceApi/"]
RUN dotnet restore "InvoiceApi/InvoiceApi.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/InvoiceApi"

# Build the application
RUN dotnet build "InvoiceApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "InvoiceApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - Using Alpine Linux for smaller image size
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Install curl for health checks
RUN apk add --no-cache curl

# Copy the published application
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN addgroup -g 1000 appuser && \
    adduser -D -s /bin/sh -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app
USER appuser

# Expose port 8080 (standard for containerized apps)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "InvoiceApi.dll"] 