using ElevatorSystem.Domain.Enums;
using ElevatorSystem.Interfaces;

namespace ElevatorSystem.Domain.Entities
{
    /// <summary>
    /// Represents an elevator in the system, responsible for handling movement,
    /// direction, floor stops, and status.
    /// </summary>
    public class Elevator : IElevator
    {
        private readonly ElevatorSettings _settings;

        public Elevator(int id, ElevatorSettings settings)
        {
            Id = id;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            CurrentFloor = _settings.MinFloor;
        }

        /// <summary>
        /// Unique identifier of the elevator.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Current floor where the elevator is located.
        /// </summary>
        public int CurrentFloor { get; set; } = 1;

        /// <summary>
        /// Current direction in which the elevator is moving (UP, DOWN, or NONE).
        /// </summary>
        public Direction Direction { get; set; } = Direction.NONE;

        /// <summary>
        /// Current status of the elevator (IDLE, MOVING, or STOPPED).
        /// </summary>
        public Status Status { get; set; } = Status.IDLE;

        /// <summary>
        /// Set of floors to stop at while going up (sorted in ascending order).
        /// </summary>
        private readonly SortedSet<int> _stopsUp = new();

        /// <summary>
        /// Set of floors to stop at while going down (sorted in descending order).
        /// </summary>
        private readonly SortedSet<int> _stopsDown = new(Comparer<int>.Create((a, b) => b.CompareTo(a)));

        /// <summary>
        /// Simulates a cooldown period (ticks) between movements or stops.
        /// 1 tick = 10 seconds.
        /// </summary>
        private int _cooldownTicks = 0;

        /// <summary>
        /// Adds a floor to the elevator's stop list based on the relative position.
        /// Also sets the initial direction if the elevator is currently idle.
        /// </summary>
        /// <param name="floor">The floor to add as a stop.</param>
        public void AddStop(int floor, Direction direction = Direction.NONE)
        {
            if (floor == CurrentFloor)
            {
                return;
            }
            //if(floor == _settings.MinFloor  && CurrentFloor  == _settings.MinFloor )
            //    Direction = Direction.UP;

            //if (floor == _settings.MaxFloor && CurrentFloor == _settings.MaxFloor)
            //    Direction = Direction.DOWN;

            if (direction == Direction.NONE)
            {
                if (floor > CurrentFloor)
                    _stopsUp.Add(floor);
                else
                    _stopsDown.Add(floor);
            }

            if (direction == Direction.UP)
            {
                _stopsUp.Add(floor);
            }

            if (direction == Direction.DOWN)
            {
                _stopsDown.Add(floor);
            }

            if (Direction == Direction.NONE)
                Direction = floor > CurrentFloor ? Direction.UP : Direction.DOWN;
        }

        /// <summary>
        /// Advances the elevator's state by one tick (simulating time).
        /// Handles movement, stopping at floors, direction switching, and cooldowns.
        /// </summary>
        public void Tick()
        {
            // If in cooldown, wait
            if (_cooldownTicks > 0)
            {
                _cooldownTicks--;
                return;
            }

            // If idle, do nothing
            if (Direction == Direction.NONE)
            {
                Status = Status.IDLE;
                return;
            }

            // Move elevator
            Status = Status.MOVING;

            if (Direction == Direction.UP)
                CurrentFloor++;
            else if (Direction == Direction.DOWN)
                CurrentFloor--;

                 _cooldownTicks = 1; // simulate 10s of movement

            // Check if current floor is a stop
            if (_stopsUp.Contains(CurrentFloor) && Direction == Direction.UP)
            {
                _stopsUp.Remove(CurrentFloor);
                Status = Status.STOPPED;
                _cooldownTicks = 1; // simulate 10s of stop
            }
            else if (_stopsDown.Contains(CurrentFloor) && Direction == Direction.DOWN)
            {
                //Console.WriteLine($"Stopping at floor {CurrentFloor} (DOWN)");
                _stopsDown.Remove(CurrentFloor);
                Status = Status.STOPPED;
                _cooldownTicks = 1; // simulate 10s of stop
            }

            // Update direction and status
            if (!_stopsUp.Any() && !_stopsDown.Any())
            {
                Direction = Direction.NONE;
                Status = Status.IDLE;
            }
            else if (Direction == Direction.UP && (!_stopsUp.Any() || CurrentFloor == _settings.MaxFloor))
                {
                Direction = Direction.DOWN;
            }
            else if (Direction == Direction.DOWN && (!_stopsDown.Any() ||CurrentFloor == _settings.MinFloor))
            {
                Direction = Direction.UP;
            }   
        }

        /// <summary>
        /// Returns a string describing the elevator’s current status, including floor, direction, stops, and movement status.
        /// </summary>
        /// <returns>A formatted string representing the elevator's state.</returns>
        public string GetStatus()
        {
            return $"E{Id}: Floor {CurrentFloor} {Direction} | Up:[{string.Join(",", _stopsUp)}] Down:[{string.Join(",", _stopsDown)}] Status:{Status}";
        }
    }
}
