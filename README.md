# GAC WMS Integration Solution

A .NET 9-based solution for integrating GAC's Warehouse Management System (WMS) with external customer ERP systems.

## Overview

This solution provides two primary integration paths:

1. **Real-time API Integration**: RESTful APIs for modern ERP systems to directly communicate with GAC's WMS
2. **File-based Legacy Integration**: Scheduled polling of legacy data files from SFTP or shared folders

## Architecture

The solution follows a layered architecture with the following components:

- **API Layer**: Handles real-time data ingestion via RESTful endpoints
- **Data Persistence Layer**: Manages database operations using Entity Framework Core
- **File Integration Layer**: Processes legacy file-based integrations
- **Transformation Engine**: Converts data between formats
- **WMS Communication Layer**: Interfaces with GAC's WMS
- **Cross-cutting Concerns**: Handles logging, monitoring, etc.

## Project Structure

```
GAC.WMS.Integrations.API/
├── Controllers/              # API endpoints
├── Models/                   # Data models
│   ├── Domain/               # Domain entities
│   ├── DTOs/                 # Data transfer objects
│   └── Validation/           # Validation rules
├── Services/                 # Business logic
│   ├── Integration/          # Integration services
│   ├── Transformation/       # Data transformation
│   └── Communication/        # WMS API communication
├── Infrastructure/           # Infrastructure components
│   ├── Data/                 # Data access
│   ├── Scheduling/           # CRON-based scheduling
│   └── FileProcessing/       # File processing
├── Configuration/            # Application configuration
└── Utilities/                # Helper classes
```

## Features

- **RESTful API Endpoints**:
  - Customer Master Data
  - Product Master Data
  - Purchase Orders (POs)
  - Sales Orders (SOs)

- **File Integration**:
  - CRON-based scheduled polling
  - Support for SFTP and shared folders
  - XML file parsing and transformation
  - Configurable for multiple customers and file formats

- **Resilience**:
  - Circuit breaker pattern
  - Retry mechanisms with exponential backoff
  - Timeout handling
  - Error logging and monitoring

## Prerequisites

- .NET 9 SDK
- SQL Server 2022
- Visual Studio 2022 or later / VS Code

## Setup Instructions

### Database Setup

1. Update the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=GAC_WMS_Integration;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

2. Run Entity Framework migrations:
   ```bash
   dotnet ef database update
   ```

### Configuration

1. **API Settings**: Update the WMS API configuration in `appsettings.json`:
   ```json
   "WmsApi": {
     "BaseUrl": "https://your-wms-api-url.com/api",
     "ApiKey": "YOUR_API_KEY",
     "Timeout": 30
   }
   ```

2. **File Integration**: Configure file polling settings in `appsettings.json`:
   ```json
   "FileIntegration": {
     "BaseDirectory": "C:\\WMS\\FileIntegration",
     "SourceDirectory": "Incoming",
     "ProcessingDirectory": "Processing",
     "ArchiveDirectory": "Archive",
     "ErrorDirectory": "Error",
     "FilePatterns": ["*.xml"],
     "PollingInterval": "0 */5 * * * ?"
   }
   ```

3. **Resilience Settings**: Configure resilience policies in `appsettings.json`:
   ```json
   "Resilience": {
     "RetryCount": 3,
     "RetryDelayMilliseconds": 1000,
     "CircuitBreakerFailureThreshold": 0.5,
     "CircuitBreakerSamplingDurationSeconds": 60,
     "CircuitBreakerDurationOfBreakSeconds": 30,
     "TimeoutSeconds": 30
   }
   ```

### Running the Application

1. Clone the repository
2. Restore packages:
   ```bash
   dotnet restore
   ```
3. Build the solution:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run --project GAC.WMS.Integrations.API
   ```

## Testing

Run the unit tests:
```bash
dotnet test
```

## API Documentation

When running in development mode, Swagger UI is available at `/swagger` to explore and test the API endpoints.

## License

Proprietary - GAC WMS Integration Solution
