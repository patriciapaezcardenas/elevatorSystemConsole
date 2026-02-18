using ElevatorSystem.Domain.Enums;

namespace ElevatorSystem.Domain
{
    public class ElevatorRequest
    {
        public string Id { get; }

        private int _sourceFloor;

        public int SourceFloor
        {
            get
            {
                return _sourceFloor;
            }
            set
            {
                if (value < 1 || value > 10)
                    throw new ArgumentOutOfRangeException(nameof(SourceFloor), $"{nameof(SourceFloor)} should be between 1 and 10.");
                _sourceFloor = value;
            }
        }

        private int _destinationFloor;
        public int DestinationFloor
        {
            get
            {
                return _destinationFloor;
            }
            set
            {
                if (value < 1 || value > 10)
                    throw new ArgumentOutOfRangeException(nameof(DestinationFloor), $"{nameof(DestinationFloor)} should be between 1 and 10.");
                _destinationFloor = value;
            }
        }
        public DateTime TimeStamp { get; set; }

        public ElevatorRequest(string id, int sourceFloor, int destinationFloor)
        {
            Id = id;
            SourceFloor = sourceFloor;
            DestinationFloor = destinationFloor;
            TimeStamp = DateTime.Now;
            RequestStatus = RequestStatus.PENDING;
        }

        public RequestStatus RequestStatus { get; set; }

        public Direction GetRequestDirection()
        {
            if (DestinationFloor > SourceFloor)
                return Direction.UP;
            else if (DestinationFloor < SourceFloor)
                return Direction.DOWN;
            else
                return Direction.NONE;
        }

        public override string ToString()
        {
            return $"[Request {Id}]-[From {SourceFloor} To {DestinationFloor.ToString()}]";
        }
    }
}