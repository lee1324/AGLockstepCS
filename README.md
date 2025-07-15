# Simple HTTP Server

A basic HTTP server built with C# using .NET 2.0 grammar without any third-party libraries.

## Features

- Handles GET and POST requests
- Simple routing system
- JSON API endpoints
- HTML web interface
- Multi-threaded client handling
- Basic HTTP request/response parsing
- Comprehensive logging service with multiple levels
- Thread-safe logging with file and console output
- Exception logging with stack traces

## Available Endpoints

- `GET /` - Main web page with API documentation
- `GET /api/status` - Returns server status as JSON
- `POST /api/echo` - Echoes back posted data as JSON

## Building and Running

### Prerequisites
- .NET 2.0 SDK or later

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

The server will start on port 8080. You can access it at `http://localhost:8080`

## Project Structure

- `Program.cs` - Main entry point
- `HttpServer.cs` - Core HTTP server implementation
- `HttpRequest.cs` - HTTP request model
- `HttpResponse.cs` - HTTP response model
- `LogService.cs` - Comprehensive logging service
- `LogServiceTest.cs` - LogService demonstration and testing
- `ServerConfig.cs` - Centralized configuration for all servers
- `Servers.csproj` - Project configuration

## Usage

1. Start the server using `dotnet run`
2. Open your browser and navigate to `http://localhost:8080`
3. Use the web interface to test the API endpoints
4. Or use curl to test the API directly:

```bash
# Get server status
curl http://localhost:8080/api/status

# Echo a message
curl -X POST -d "Hello World" http://localhost:8080/api/echo
```

## Implementation Details

This server uses:
- `TcpListener` for accepting connections
- `NetworkStream` for reading/writing data
- Manual HTTP parsing (no third-party libraries)
- Thread-per-connection model
- Basic routing based on HTTP method and path
- Singleton LogService with background thread processing
- Thread-safe logging queue with Monitor synchronization

The server follows .NET 2.0 grammar and uses only built-in .NET Framework classes.

## Configuration

All server settings are centralized in `ServerConfig.cs`:

### Server Ports
- **HTTP Server**: Port 9005
- **UDP Server**: Port 9006  
- **TCP Server**: Port 9007

### Network Settings
- **Buffer Size**: 4096 bytes
- **Socket Timeout**: 30 seconds
- **TCP Max Connections**: 10
- **TCP Connection Timeout**: 30 seconds

### Logging Settings
- **Log File**: `server.log`
- **Default Log Level**: Info
- **Console Output**: Enabled
- **File Output**: Enabled

### Test Settings
- **Test Timeout**: 5 seconds
- **Test Delay**: 500ms

To change any configuration, simply modify the constants in `ServerConfig.cs`.

## LogService Features

The LogService provides:
- **Log Levels**: Debug, Info, Warning, Error, Fatal
- **Output Options**: Console (with colors) and file output
- **Thread Safety**: Background thread processing with synchronized queue
- **Exception Logging**: Automatic stack trace inclusion
- **Configuration**: Minimum log level, file path, output options
- **Log Retrieval**: Read recent log entries
- **File Management**: Clear log files

### Usage Example:
```csharp
// Configure and start
LogService.Instance.LogFilePath = "app.log";
LogService.Instance.MinimumLevel = LogLevel.Info;
LogService.Instance.Start();

// Log messages
LogService.Instance.Info("Server started");
LogService.Instance.Warning("High memory usage detected");
LogService.Instance.Error("Database connection failed", exception);

// Stop when done
LogService.Instance.Stop();
``` 