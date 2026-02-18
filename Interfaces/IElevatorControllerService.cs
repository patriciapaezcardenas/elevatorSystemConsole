using ElevatorSystem.Domain;
using ElevatorSystem.Domain.Entities;

namespace ElevatorSystem.Interfaces
{
    /// <summary>
    /// Defines the contract for managing elevators and handling elevator requests within the system.
    /// </summary>
    public interface IElevatorControllerService
    {
        /// <summary>
        /// Adds a new elevator instance to the system.
        /// Typically used during initialization or dynamic scaling of elevators.
        /// </summary>
        void AddElevator();

        /// <summary>
        /// Assigns an elevator to the specified request based on availability, direction, and proximity.
        /// </summary>
        /// <param name="elevatorRequest">The request containing source and destination floor information.</param>
        /// <returns>The assigned <see cref="Elevator"/> instance, or <c>null</c> if no suitable elevator is available.</returns>
        Elevator AssignElevator(ElevatorRequest elevatorRequest);

        /// <summary>
        /// Determines if there are any pending elevator requests awaiting assignment.
        /// </summary>
        /// <returns><c>true</c> if there are pending requests; otherwise, <c>false</c>.</returns>
        bool HasPendingRequests();

        /// <summary>
        /// Initializes the elevator system by registering and preparing elevator instances.
        /// The number of elevators is typically determined by configuration.
        /// </summary>
        void InitializeElevators();

        /// <summary>
        /// Starts the asynchronous execution loop for processing elevator requests
        /// and updating elevator states. This method runs continuously until cancellation is requested.
        /// </summary>
        /// <param name="cancellationToken">A token used to cancel the operation gracefully.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous run loop.</returns>
        Task RunAsync(CancellationToken cancellationToken);
    }
}