# Stage 1: Build and Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files first (this helps Docker cache dependencies)
COPY ["BlazorUI/BlazorUI.csproj", "BlazorUI/"]
COPY ["scripting/scripting.csproj", "scripting/"]

# Restore dependencies
RUN dotnet restore "BlazorUI/BlazorUI.csproj"

# Copy all the actual source code
COPY . .

# Build and publish the BlazorUI app
WORKDIR "/src/BlazorUI"
RUN dotnet publish "BlazorUI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8080 (the default for .NET 9 apps)
EXPOSE 8080

# Start the Blazor app
ENTRYPOINT ["dotnet", "BlazorUI.dll"]
