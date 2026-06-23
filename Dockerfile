# syntax=docker/dockerfile:1
# Multi-stage build for the ASP.NET Core API.
#
# Why two stages? C# is compiled. The SDK image (with the compiler) is large
# (~800MB); the runtime image is small. We compile in the SDK stage, then copy
# ONLY the compiled output into a slim runtime image. Final image stays lean.
# (Laravel/PHP isn't compiled, so its Dockerfile is usually one stage — this
#  two-stage pattern is a .NET norm.)

# ---- Stage 1: build + publish ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the csproj first and restore. This caches the (slow) NuGet restore
# layer so it's reused unless dependencies change — like copying composer.json
# and running `composer install` before copying the rest of the app.
COPY src/DotnetApp/DotnetApp.csproj src/DotnetApp/
RUN dotnet restore src/DotnetApp/DotnetApp.csproj

# Now copy the rest of the source and publish a release build.
COPY . .
RUN dotnet publish src/DotnetApp/DotnetApp.csproj -c Release -o /app/publish --no-restore

# ---- Stage 2: runtime ----
# aspnet (not sdk) = runtime only, no compiler. Much smaller + smaller attack surface.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Run as a non-root user (the base image provides 'app'). Security best practice.
USER app

# Kestrel listens on 8080 inside the container (8080 needs no root, unlike 80).
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DotnetApp.dll"]
