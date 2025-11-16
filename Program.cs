using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.CompilerServices;

// Foundations of the Web API
WebApplicationBuilder trainReservationBuilder = WebApplication.CreateBuilder(args);
trainReservationBuilder.Services.AddEndpointsApiExplorer();
trainReservationBuilder.Services.AddSwaggerGen();
WebApplication trainReservationApi = trainReservationBuilder.Build();

// Swagger Testing in Development Environment
if (trainReservationApi.Environment.IsDevelopment())
{
    trainReservationApi.UseSwagger();
    trainReservationApi.UseSwaggerUI();
    trainReservationApi.UseHttpsRedirection();
}

// Creating the API Endpoints
trainReservationApi.MapPost("/api/reservations", (ReservationRequest reservationRequest) =>
{
    // Input validation is added to provide input consistency.
    if (reservationRequest == null)
        return Results.BadRequest("Request body cannot be empty.");

    if(reservationRequest.Train==null)
        return Results.BadRequest("Train information must be provided.");

    if (reservationRequest.ReservationCount < 0)
        return Results.BadRequest("ReservationCount must be greater than 0.");

    if (reservationRequest.Train.Wagons == null || !reservationRequest.Train.Wagons.Any())
        return Results.BadRequest("The train must have at least one wagon.");

    foreach(var Wagon in reservationRequest.Train.Wagons)
    {
        if (Wagon.Capacity < 0 || Wagon.FullSeats < 0)
        {
            return Results.BadRequest($"Error with {Wagon.Name}: Capacity and FullSeats values must be greater than 0.");
        }
        if (Wagon.FullSeats > Wagon.Capacity)
        {
            return Results.BadRequest($"Error with {Wagon.Name}: The value of FullSeats cannot be greater than a wagon's capacity.");
        }
    }

    // Core logic is in the CalculateReservations function. Return HTTP 200 if request is successful.
    ReservationResponse reservationResponse = ReservationService.CalculateReservations(reservationRequest);
    return Results.Ok(reservationResponse);
})
.WithName("MakeReservation"); // For checking via Swagger

trainReservationApi.Run();

// Core Reservation Logic

// Defining Data Models via "record" type
// Record type is introduced in C# 9.0. It is used in this project due to its immutability which makes it impossible to alter.

public record Wagon(string Name, int Capacity, int FullSeats);

public record Train(string Name, List<Wagon> Wagons);

public record ReservationRequest(Train Train, int ReservationCount, bool DifferentVagonsAllowed);

public record PlacementDetails(string WagonName, int ReservedCustomerCount);

public record ReservationResponse(bool ReservationAvailable, List<PlacementDetails> PlacementDetails);

// Function created separately from the MapPost due to "Separation of Concerns" and unit testing purposes.
public static class ReservationService 
{
    // Adhere to the 70% rule mentioned in the project document. Only consider the wagons with available seats.
    public static ReservationResponse CalculateReservations(ReservationRequest reservationRequest)
    {
        const double maxFullSeatsThreshold = 0.7;

        List<(string wagonName, int availableSeats)> availableWagons = new List<(string wagonName, int availableSeats)>();

        foreach (Wagon wagon in reservationRequest.Train.Wagons)
        {
            int maxFullSeats = (int)Math.Floor(wagon.Capacity * maxFullSeatsThreshold);

            int availableSeats = maxFullSeats - wagon.FullSeats;

            if (availableSeats > 0)
            {
                availableWagons.Add((wagon.Name, availableSeats));
            }
        }

        // Train empty seats check. If no seats are available, return empty list.
        int maxAvailableSeats = availableWagons.Sum(v => v.availableSeats);
        if (maxAvailableSeats < reservationRequest.ReservationCount)
        {
            return new ReservationResponse(false, new List<PlacementDetails>());
        }

        // Placement logic - Everyone is either in the same wagon, or customers are divided into different wagons.
        List<PlacementDetails> placementDetailsList = new List<PlacementDetails>();
        if (!reservationRequest.DifferentVagonsAllowed) // same wagon scenario
        {
            var availableWagon = availableWagons
                .FirstOrDefault(v => v.availableSeats >= reservationRequest.ReservationCount);

            if (availableWagon.wagonName != null) // Success Condition: Requested space available in one wagon.
            {
                placementDetailsList.Add(new PlacementDetails(availableWagon.wagonName, reservationRequest.ReservationCount));
                return new ReservationResponse(true, placementDetailsList);
            }
            else // Fail Condition: Requested space available in the train, but not enough space in one wagon.
            {
                return new ReservationResponse(false, new List<PlacementDetails>());
            }
        }
        else // different wagons scenario, using Greedy algorithm for placing customers.
        {
            int remainingCustomerCount = reservationRequest.ReservationCount;

            foreach (var wagon in availableWagons)
            {
                if (remainingCustomerCount == 0) // Success Condition: Everyone is seated.
                    break;

                // Space check and placement for unseated customers
                int spaceForCustomers = Math.Min(remainingCustomerCount, wagon.availableSeats);

                if (spaceForCustomers > 0)
                {
                    placementDetailsList.Add(new PlacementDetails(wagon.wagonName, spaceForCustomers));
                    remainingCustomerCount -= spaceForCustomers;
                }
            }

            // Should always be "true" due to the capacity check in the beginning.
            return new ReservationResponse(true, placementDetailsList);
        }
    }
}
