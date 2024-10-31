# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Set the default environment to Development
ARG ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}

# Copy the solution and project files
COPY weekplanner-api.sln ./
COPY GirafAPI/GirafAPI.csproj ./GirafAPI/
COPY Giraf.UnitTests/Giraf.UnitTests.csproj ./Giraf.UnitTests/
COPY Giraf.IntegrationTests/Giraf.IntegrationTests.csproj ./Giraf.IntegrationTests/
RUN dotnet restore weekplanner-api.sln

# Copy the entire source code for the projects
COPY . .

# Expose the port for the app
EXPOSE 5171

# Set the entry point for development
ENTRYPOINT ["sh", "-c", "if [ \"$ASPNETCORE_ENVIRONMENT\" = 'Development' ]; then dotnet watch run --project GirafAPI/GirafAPI.csproj --urls http://+:5171; else dotnet GirafAPI.dll; fi"]

