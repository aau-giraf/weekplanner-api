# Stage 1: Build the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /app

# Set the default environment to Development
ARG ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}

# Copy the solution and project files
COPY weekplanner-api.sln ./
COPY GirafAPI/*.csproj ./GirafAPI/
COPY GirafAPI/Data/Migrations/*.cs ./GirafAPI/Data/Migrations/
COPY Giraf.UnitTests/*.csproj ./Giraf.UnitTests/
COPY Giraf.IntegrationTests/*.csproj ./Giraf.IntegrationTests/

# Restore dependencies for all projects in the solution
RUN dotnet restore weekplanner-api.sln

# Copy the entire source code for the projects
COPY . .

# Build each project individually with the architecture specified
RUN dotnet build ./GirafAPI/GirafAPI.csproj -c Release -o /app/build -a $TARGETARCH
RUN dotnet build ./Giraf.UnitTests/Giraf.UnitTests.csproj -c Release -o /app/build -a $TARGETARCH
RUN dotnet build ./Giraf.IntegrationTests/Giraf.IntegrationTests.csproj -c Release -o /app/build -a $TARGETARCH

# Publish the main application with the architecture specified
RUN dotnet publish ./GirafAPI/GirafAPI.csproj -c Release -o /app/publish -a $TARGETARCH --no-restore

# Expose the port for the app
EXPOSE 5171

# Set the entry point for development or production
ENTRYPOINT ["sh", "-c", "if [ \"$ASPNETCORE_ENVIRONMENT\" = 'Development' ]; then dotnet watch run --project GirafAPI/GirafAPI.csproj --urls http://+:5171; else dotnet /app/publish/GirafAPI.dll; fi"]

