# Use .NET 8 SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy and restore dependencies
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o /out

# Use .NET 8 runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Expose the port your app runs on (default: 80)
EXPOSE 80

# Start the app
ENTRYPOINT ["dotnet", "TodoListApp.dll"]