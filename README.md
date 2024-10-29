# weekplanner-api
 The backend API for the GIRAF weekplaner app

## Get Started
1. Install Microsoft Entity Framework
    ```bash
    dotnet tool install --global dotnet-ef
    ```
2. Run the API from the GirafAPI directory
    ```bash
    dotnet run
    ```

## Update the Database
If you make changes to entities or DTOs, make sure to update the database:

1. Add a new migration
   ```bash
   dotnet ef migrations add {NewMigrationName} --output-dir Data\Migrations
   ```
2. Update the database
   ```bash
   dotnet ef database update
   ```
   
## Running in a Container

### Development Environment
```
docker compose up
```
(If you are running the api on Linux and it does not seem to be working, try changing the port in docker-compose.yml to 5171:8080 instead of 5171:5171)

### Production Environment
```
docker compose -f docker-compose.prod.yml up --build
```
