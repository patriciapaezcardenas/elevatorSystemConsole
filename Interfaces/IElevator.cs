using ElevatorSystem.Domain.Enums;
using System;

namespace ElevatorSystem.Interfaces
{
    /// <summary>
    /// Defines the contract for elevator behavior within the elevator system.
    /// </summary>
    public interface IElevator
    {
        /// <summary>
        /// Adds a target floor to the elevator's queue of stops.
        /// The direction and stop logic will be determined based on the current floor and requested floor.
        /// </summary>
        /// <param name="floor">The floor number where the elevator should stop.</param>
        void AddStop(int floor, Direction direction);

        /// <summary>
        /// Gets a human-readable string representing the current status of the elevator,
        /// including floor, direction, queued stops, and status.
        /// </summary>
        /// <returns>A formatted status string.</returns>
        string GetStatus();

        /// <summary>
        /// Advances the elevator's internal state by one tick.
        /// This may simulate movement, stopping at a floor, or entering idle state depending on internal logic.
        /// </summary>
        void Tick();
    }
}