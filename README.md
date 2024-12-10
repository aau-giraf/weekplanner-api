# weekplanner-api
This is the backend REST API for the weekplanner branch of the GIRAF project.
The weekplanner API uses Microsoft's .NET 8 architecture with a modern MinimalAPI
setup. It also includes a containerized PostgreSql database.

## Get Started
1. Download and install .NET 8 from:
   
   https://dotnet.microsoft.com/en-us/download/dotnet

2. Install Microsoft Entity Framework:
    ```bash
    dotnet tool install --global dotnet-ef
    ```
3. Download and install Docker Desktop from:
   
   https://www.docker.com/products/docker-desktop/

4. Launch the API from the weekplanner-api directory:
    ```bash
    docker compose up
    ```

## Production Environment
The production environment uses a different build process from 
the development environment. To run the production environment, call:
```bash
docker compose -f docker-compose.prod.yml up --build
```

## Update the Database
If you make changes to entities or DTOs, make sure to update the database:

1. Add a new migration
   ```
   dotnet ef migrations add Your_Migration_Name_Here --output-dir Data\Migrations
   ```
2. Update the database
   ```bash
   dotnet ef database update
   ```
