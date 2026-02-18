# ElevatorSystem

Console simulation of an elevator controller that generates requests and moves elevators using a simple scheduling loop. Uses Microsoft.Extensions.Hosting, IConfiguration and Serilog for logging.

## Prerequisites
- .NET 8 SDK (install from https://dotnet.microsoft.com)
- Visual Studio 2022 (optional) or any editor/terminal that can build .NET projects

## Build
From the repository root (where `ElevatorSystem.csproj` lives):

dotnet build

## Run
From the repository root:

dotnet run --project ElevatorSystem.csproj

Or open the project in Visual Studio 2022, set `ElevatorSystem` as the startup project and run (F5 or Ctrl+F5).

When running the app it starts the simulation and prints lamp/status logs to console. To stop the simulation press `q`.

## Configuration
The application reads `appsettings.json` (copied to output). Key configuration:

- `elevatorsQuantity` — number of elevators to initialize (int).
- Serilog configuration (logging level, sinks) is read from the same configuration file.

Example minimal `appsettings.json`:

{
  "elevatorsQuantity": 4,
  "maxFloor": 10,
  "minFloor":  1,
  "randomRequestService": {
    "intervalMilliseconds":6000
  }
}

Adjust `elevatorsQuantity` to change how many Elevator instances are created.

## How it works (short)
- `AppBuilder.Create()` builds configuration, initializes Serilog, registers DI services:
  - `ElevatorControllerService` (singleton)
  - `IRequestService` -> `RandomRequestService` (singleton factory)
  - `IElevator` -> `Elevator` (transient)
- `Program.Main` creates the host, resolves `ElevatorControllerService` with DI and runs `RunAsync` in a task.
- `RandomRequestService` periodically raises `OnNewRequest`; controller assigns elevators and advances elevator state every tick.

## Common issues & troubleshooting
- Missing `appsettings.json` or incorrect keys: ensure the file is present and copied to output (project already sets CopyToOutputDirectory).
- DI errors (Unable to resolve service for type ...): confirm constructor parameter signatures match registered services:
  - Use `ILogger<T>` (generic) in service constructors so the logging provider resolves automatically.
  - `IRequestService` is registered in `AppBuilder.Create()`. If you add more constructor dependencies, register them in `ConfigureServices`.
- If you change `RandomRequestService` construction (for example to inject `ILogger`), update service registration in `AppBuilder`.

## Customizing behavior
- Elevator count: change `elevatorsQuantity` in `appsettings.json`.
- Request frequency: change `intervalMilliseconds` in `appsettings.json` when registering `RandomRequestService` in `AppBuilder.Create()`.

- Logging sinks & levels: update Serilog section in `appsettings.json`.

## Stopping the app
Press `q` in the console. The program cancels the run task and exits gracefully.

## Notes
- The repository already wires Serilog via `.UseSerilog()` and reads configuration from `appsettings.json`.
- If adding runtime services or changing constructor signatures, update DI registrations accordingly."# elevatorSystemConsole" 
