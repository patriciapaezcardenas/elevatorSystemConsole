using ElevatorSystem.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ElevatorSystem
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = AppBuilder.Create();

            var elevatorController = ActivatorUtilities.CreateInstance<ElevatorControllerService>(host.Services);

            elevatorController.InitializeElevators();

            // Cancelation control.
            var cts = new CancellationTokenSource();

            // Executes the logic in a separate task.
            var task = elevatorController.RunAsync(cts.Token);

            // Waits for 'q' key to exit.
            Console.WriteLine("Press 'q' to stop the system.");
            while (Console.ReadKey(true).KeyChar != 'q') { }

            cts.Cancel(); // Stops the run async task.
            await task;   // Waits until it finishes.
        }
    }
}

