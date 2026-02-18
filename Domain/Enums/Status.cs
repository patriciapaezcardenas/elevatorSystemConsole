namespace ElevatorSystem.Domain.Enums
{
    /// <summary>
    /// Represents the various states that an elevator can be in during its operation. This enumeration is used to track.
    /// </summary>
    public enum Status
    {       
        /// <summary>
        /// Indicates that the entity is currently in motion.
        /// </summary>
        MOVING = 1,

        /// <summary>
        /// 
        /// </summary>
        IDLE = 3,

        /// <summary>
        /// Indicates that the elevator is currently in the process of boarding passengers.
        /// </summary>
        STOPPED
    }
}
