# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Set default environment as Development
ARG ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}


# Copy the .csproj file and restore any dependencies
COPY weekplanner-api.sln ./
COPY GirafAPI/*.csproj ./GirafAPI/
COPY Giraf.UnitTests/*.csproj ./Giraf.UnitTests/
COPY Giraf.IntegrationTests/*.csproj ./Giraf.IntegrationTests/
RUN dotnet restore weekplanner-api.sln

# Copy the rest of the application code
COPY . ./
WORKDIR /src/GirafAPI

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Set up the runtime environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

RUN ls -la /app

# Specify the entrypoint command to run the app
ENTRYPOINT ["dotnet", "GirafAPI.dll"]
