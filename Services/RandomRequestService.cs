using ElevatorSystem.Domain;
using ElevatorSystem.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Timers;

namespace ElevatorSystem.Services
{
    /// <summary>
    /// Service responsible for generating elevator requests at random intervals,
    /// as well as accepting manual requests. It triggers events that notify
    /// listeners of newly generated or submitted requests.
    /// </summary>
    public class RandomRequestService : IRequestService
    {
        private readonly System.Timers.Timer _timer;
        private readonly Random _random;
        private readonly int _millisecondsInterval;

        /// <summary>
        /// Event that is triggered whenever a new elevator request is generated
        /// or submitted manually.
        /// </summary>
        public event Action<ElevatorRequest>? OnNewRequest;

        /// <summary>
        /// Indicates whether the request generation service is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomRequestService"/> class.
        /// </summary>
        /// <param name="intervalMilliseconds">Interval in milliseconds between automatic request generations.</param>
        public RandomRequestService(int intervalMilliseconds)
        {
            _millisecondsInterval = intervalMilliseconds;
            _random = new Random();
            _timer = new System.Timers.Timer(intervalMilliseconds);
            _timer.Elapsed += GenerateRandomRequest;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        /// <summary>
        /// Starts the automatic request generation timer.
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;
            _timer.Start();
        }

        /// <summary>
        /// Stops the automatic request generation timer.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            IsRunning = false;
            _timer.Stop();
        }

        /// <summary>
        /// Submits a manually created elevator request. Triggers the <see cref="OnNewRequest"/> event.
        /// </summary>
        /// <param name="request">The manually created elevator request.</param>
        public void SubmitManualRequest(ElevatorRequest request)
        {
            OnNewRequest?.Invoke(request);
        }

        /// <summary>
        /// Generates a random elevator request and invokes the <see cref="OnNewRequest"/> event.
        /// Triggered automatically by the internal timer.
        /// </summary>
        /// <param name="sender">The timer object triggering the event.</param>
        /// <param name="e">Timer event arguments.</param>
        private void GenerateRandomRequest(object? sender, ElapsedEventArgs e)
        {
            var sourceFloor = _random.Next(1, 11); // Floors 1 to 10
            var destinationFloor = _random.Next(1, 11); // Floors 1 to 10
            var id = _random.Next(1000, 9999).ToString(); // 4-digit random ID

            var request = new ElevatorRequest(id, sourceFloor, destinationFloor);
            OnNewRequest?.Invoke(request);
        }
    }
}