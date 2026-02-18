namespace ElevatorSystem.Domain
{
    /// <summary>
    /// Class to hold configuration settings for the elevator system, such as minimum and maximum floors.
    /// </summary>
    public sealed class ElevatorSettings
    {
        public int MinFloor { get; set; } = 1;
        public int MaxFloor { get; set; } = 10;
    }
}
