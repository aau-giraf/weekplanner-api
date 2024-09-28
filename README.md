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
If you make changes to entities or DTOs, make sure to update migrations:
   ```bash
   dotnet ef migrations remove
   dotnet ef migrations add Initial --output-dir Data\Migrations
   ```

