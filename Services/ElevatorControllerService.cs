using ElevatorSystem.Domain;
using ElevatorSystem.Domain.Entities;
using ElevatorSystem.Domain.Enums;
using ElevatorSystem.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ElevatorSystem.Services
{
    /// <summary>
    /// Core service responsible for managing elevators, handling requests,
    /// and orchestrating elevator assignment and movement logic.
    /// </summary>
    public class ElevatorControllerService : IElevatorControllerService
    {
        private readonly ILogger<ElevatorControllerService> _logger;
        private readonly IRequestService _requestService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The number of elevators to initialize. Read from configuration.
        /// Defaults to <c>1</c> until configuration is loaded.
        /// </summary>
        private int _elevatorsQuantity = 1;

        /// <summary>
        /// Configuration key used to read the elevators quantity from <c>appsettings.json</c>.
        /// </summary>
        private const string ELEVATOR_QUANTITY_CONFIG = "elevatorsQuantity";

        /// <summary>
        /// Gets the list of all elevator requests received.
        /// </summary>
        public List<ElevatorRequest> ElevatorRequests { get; } = new();

        /// <summary>
        /// Gets the list of all elevators registered in the system.
        /// </summary>
        public List<Elevator> Elevators { get; } = new();

        /// <summary>
        /// Factory delegate used to create elevators while keeping the controller
        /// independent from the <see cref="Elevator"/> construction details (DI-friendly).
        /// </summary>
        private readonly Func<int, Elevator> _elevatorFactory;

        /// <summary>
        /// Next elevator identifier to assign when creating new elevators.
        /// </summary>
        private int _nextElevatorId = 1;

        /// <summary>
        /// Creates a new instance of <see cref="ElevatorControllerService"/>.
        /// Subscribes to <see cref="IRequestService.OnNewRequest"/> to receive elevator requests.
        /// </summary>
        /// <param name="logger">Logger for outputting system information.</param>
        /// <param name="requestService">Service responsible for receiving elevator requests.</param>
        /// <param name="configuration">Application configuration source.</param>
        /// <param name="elevatorFactory">Factory used to create elevators with an assigned id.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="elevatorFactory"/> is <c>null</c>.
        /// </exception>
        public ElevatorControllerService(
            ILogger<ElevatorControllerService> logger,
            IRequestService requestService,
            IConfiguration configuration,
            Func<int, Elevator> elevatorFactory)
        {
            _logger = logger;
            _requestService = requestService;
            _configuration = configuration;
            _elevatorFactory = elevatorFactory ?? throw new ArgumentNullException(nameof(elevatorFactory));

            _elevatorsQuantity = configuration.GetSection(ELEVATOR_QUANTITY_CONFIG).Get<int>();

            if (_elevatorsQuantity == 0)
            {
                _elevatorsQuantity = 4; // Default to 4 elevators if not configured
                _logger.LogWarning(
                    "Verify the configuration key {0} value in appSettings.config file. Default value is used.",
                    ELEVATOR_QUANTITY_CONFIG);
            }

            // Subscribe to incoming requests
            _requestService.OnNewRequest += HandleNewRequest;
        }

        /// <summary>
        /// Handles a new incoming elevator request by logging and storing it.
        /// </summary>
        /// <param name="request">The new elevator request.</param>
        private void HandleNewRequest(ElevatorRequest request)
        {
            _logger.LogInformation(
                "New request received. Id={Id} Source={Source} Destination={Destination}",
                request.Id, request.SourceFloor, request.DestinationFloor);

            AddRequest(request);
        }

        /// <summary>
        /// Creates and registers a new elevator instance in the system.
        /// Uses the configured factory to keep elevator creation centralized and testable.
        /// </summary>
        public void AddElevator()
        {
            var id = _nextElevatorId++;
            var elevator = _elevatorFactory(id);

            Elevators.Add(elevator);

            _logger.LogInformation("Elevator created. ElevatorId={ElevatorId}", id);
        }

        /// <summary>
        /// Submits a manually created elevator request to the request service.
        /// Intended for quick local testing / demo scenarios.
        /// </summary>
        public void SendManualRequest()
        {
            var manualRequest = new ElevatorRequest("manual", 1, 5);
            _requestService.SubmitManualRequest(manualRequest);
        }

        /// <summary>
        /// Validates and adds a new elevator request to the list of tracked requests.
        /// Requests with invalid floor ranges are ignored.
        /// </summary>
        /// <param name="request">The request to be added.</param>
        /// <remarks>
        /// Currently validates floors using a fixed range (1..10). Consider reading min/max
        /// floors from configuration to avoid hardcoded values.
        /// </remarks>
        public void AddRequest(ElevatorRequest request)
        {
            if (request.SourceFloor < 1 || request.SourceFloor > 10)
                return;

            if (request.DestinationFloor < 1 || request.DestinationFloor > 10)
                return;

            ElevatorRequests.Add(request);
        }

        /// <summary>
        /// Determines whether there are any pending elevator requests.
        /// </summary>
        /// <returns><c>true</c> if there are pending requests; otherwise, <c>false</c>.</returns>
        public bool HasPendingRequests()
        {
            return ElevatorRequests.Any(e => e.RequestStatus == RequestStatus.PENDING);
        }

        /// <summary>
        /// Assigns the most suitable elevator to a given request based on availability and direction.
        /// </summary>
        /// <param name="elevatorRequest">The elevator request to fulfill.</param>
        /// <returns>
        /// An <see cref="Elevator"/> instance assigned to the request, or <c>null</c> if no suitable elevator is found.
        /// </returns>
        /// <remarks>
        /// Selection strategy:
        /// <list type="number">
        /// <item><description>Prefer an <see cref="Status.IDLE"/> elevator.</description></item>
        /// <item><description>Otherwise, prefer the closest elevator moving in the same direction as the request.</description></item>
        /// <item><description>If none match the direction, select the closest elevator regardless of direction.</description></item>
        /// </list>
        /// </remarks>
        public Elevator AssignElevator(ElevatorRequest elevatorRequest)
        {
            // 1. Try to assign an IDLE elevator
            var assignedElevator = Elevators.FirstOrDefault(e => e.Status == Status.IDLE);
            if (assignedElevator != null)
                return assignedElevator;

            // 2. Assign the closest elevator moving in the same direction
            var requestDirection = elevatorRequest.GetRequestDirection();
            assignedElevator = Elevators
                .Where(e => e.Direction == requestDirection)
                .OrderBy(e => Math.Abs(e.CurrentFloor - elevatorRequest.SourceFloor))
                .FirstOrDefault();

            // 3. Fallback: closest elevator overall
            if (assignedElevator == null)
            {
                assignedElevator = Elevators
                    .OrderBy(e => Math.Abs(e.CurrentFloor - elevatorRequest.SourceFloor))
                    .FirstOrDefault();
            }

            return assignedElevator;
        }

        /// <summary>
        /// Main asynchronous loop that continuously processes requests and updates elevator states.
        /// </summary>
        /// <param name="cancellationToken">A token used to cancel the simulation loop gracefully.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// The loop performs the following steps:
        /// <list type="bullet">
        /// <item><description>Collects pending requests.</description></item>
        /// <item><description>Assigns elevators and converts requests to <see cref="RequestStatus.ASSIGNED"/>.</description></item>
        /// <item><description>Updates each elevator by calling <see cref="Elevator.Tick"/>.</description></item>
        /// <item><description>Waits a fixed delay between iterations.</description></item>
        /// </list>
        /// Any unhandled exception inside the loop is logged and the loop continues.
        /// </remarks>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚀 Elevator system is running...");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var pendingRequests = ElevatorRequests
                        .Where(r => r.RequestStatus == RequestStatus.PENDING)
                        .ToList();

                    foreach (var request in pendingRequests)
                    {
                        var elevator = AssignElevator(request);
                        if (elevator == null) continue;

                        request.RequestStatus = RequestStatus.ASSIGNED;

                        if (request.SourceFloor == request.DestinationFloor)
                        {
                            _logger.LogInformation("Request completed immediately (same floor). Id={Id} Floor={Floor}", request.Id, request.SourceFloor);
                            continue;
                        }

                        elevator.AddStop(request.SourceFloor);
                        elevator.AddStop(
                            request.DestinationFloor,
                            request.DestinationFloor < request.SourceFloor ? Direction.DOWN : Direction.UP);
                    }

                    foreach (var elevator in Elevators)
                    {
                        elevator.Tick();
                        _logger.LogInformation(elevator.GetStatus());
                    }

                    await Task.Delay(4000, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error while running elevator simulation loop.");
                }
            }

            _logger.LogInformation("🛑 Elevator system has been stopped.");
        }

        /// <summary>
        /// Initializes and registers the configured number of elevator instances.
        /// The number of elevators is read from the configuration.
        /// </summary>
        /// <remarks>
        /// If the configuration resolves to a value less than or equal to zero, no elevators are created.
        /// </remarks>
        public void InitializeElevators()
        {
            if (_elevatorsQuantity <= 0)
            {
                return;
            }

            for (int i = 1; i <= _elevatorsQuantity; i++)
            {
                AddElevator();
            }
        }
    }
}