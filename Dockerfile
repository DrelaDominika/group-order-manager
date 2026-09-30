# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first so Docker can cache the restore layer
# (only re-runs `restore` when a .csproj actually changes, not on every code edit)
COPY GroupOrderManager.Api/*.csproj GroupOrderManager.Api/
COPY GroupOrderManager.Application/*.csproj GroupOrderManager.Application/
COPY GroupOrderManager.Domain/*.csproj GroupOrderManager.Domain/
COPY GroupOrderManager.Infrastructure/*.csproj GroupOrderManager.Infrastructure/
RUN dotnet restore GroupOrderManager.Api/GroupOrderManager.Api.csproj

# Now copy everything else and build
COPY GroupOrderManager.Api/ GroupOrderManager.Api/
COPY GroupOrderManager.Application/ GroupOrderManager.Application/
COPY GroupOrderManager.Domain/ GroupOrderManager.Domain/
COPY GroupOrderManager.Infrastructure/ GroupOrderManager.Infrastructure/

RUN dotnet publish GroupOrderManager.Api/GroupOrderManager.Api.csproj -c Release -o /app --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "GroupOrderManager.Api.dll"]
