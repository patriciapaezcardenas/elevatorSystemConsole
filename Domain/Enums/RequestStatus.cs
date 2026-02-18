namespace ElevatorSystem.Domain.Enums
{
    /// <summary>
    /// Represents the status of a request to identify if it is pending or has been assigned.
    /// </summary>
    public enum RequestStatus
    {
        /// <summary>
        /// Pending request that has not yet been assigned to an elevator.
        /// </summary>
        PENDING,

        /// <summary>
        /// Assigned request that has been allocated to an elevator and is in the process of being fulfilled.
        /// </summary>
        ASSIGNED
    }
}
