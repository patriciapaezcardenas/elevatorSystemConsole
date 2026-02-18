using ElevatorSystem.Domain;
using System;

namespace ElevatorSystem.Interfaces
{
    /// <summary>
    /// Defines the contract for services that manage elevator requests,
    /// including automatic and manual submission, as well as control over request generation.
    /// </summary>
    public interface IRequestService
    {
        /// <summary>
        /// Event triggered whenever a new elevator request is generated or submitted.
        /// Consumers can subscribe to this event to react to incoming requests.
        /// </summary>
        event Action<ElevatorRequest> OnNewRequest;

        /// <summary>
        /// Starts the request generation service.
        /// Typically begins automatic generation of elevator requests at fixed intervals.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the request generation service.
        /// Useful for pausing or terminating the request flow.
        /// </summary>
        void Stop();

        /// <summary>
        /// Submits a manually created elevator request to the system.
        /// This bypasses automatic generation and allows external input.
        /// </summary>
        /// <param name="request">The elevator request to submit.</param>
        void SubmitManualRequest(ElevatorRequest request);
    }
}