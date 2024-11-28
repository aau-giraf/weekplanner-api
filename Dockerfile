# Stage 1: Build the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /app
ARG ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}

COPY weekplanner-api.sln ./
COPY GirafAPI/*.csproj ./GirafAPI/
COPY GirafAPI/Data/Migrations/*.cs ./GirafAPI/Data/Migrations/
COPY Giraf.UnitTests/*.csproj ./Giraf.UnitTests/
COPY Giraf.IntegrationTests/*.csproj ./Giraf.IntegrationTests/
RUN dotnet restore weekplanner-api.sln

COPY . .
RUN dotnet build ./GirafAPI/GirafAPI.csproj -c Release -o /app/build -a $TARGETARCH
RUN dotnet build ./Giraf.UnitTests/Giraf.UnitTests.csproj -c Release -o /app/build -a $TARGETARCH
RUN dotnet build ./Giraf.IntegrationTests/Giraf.IntegrationTests.csproj -c Release -o /app/build -a $TARGETARCH
RUN dotnet publish ./GirafAPI/GirafAPI.csproj -c Release -o /app/publish -a $TARGETARCH --no-restore

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build /app/publish .

EXPOSE 5171

# Set the entry point for development or production
ENTRYPOINT ["sh", "-c", "if [ \"$ASPNETCORE_ENVIRONMENT\" = 'Development' ]; then dotnet watch run --project GirafAPI/GirafAPI.csproj --urls http://+:5171; else dotnet /app/GirafAPI.dll; fi"]
