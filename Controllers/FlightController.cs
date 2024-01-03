using Flights.ReadModels;
using Microsoft.AspNetCore.Mvc;
using System;
using Flights.Domain.Entities;
using Flights.Dtos;
using Flights.Domain.Errors;
using Flights.Data;
using Microsoft.EntityFrameworkCore;

namespace Flights.Controllers;

[ApiController]
[Route("[controller]")]
public class FlightController : ControllerBase
{
    private readonly ILogger<FlightController> _logger;
    private readonly Entities _entities;

    public FlightController(ILogger<FlightController> logger, Entities entities)
    {
        _logger = logger;
        _entities = entities;
    }

    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ProducesResponseType(typeof(IEnumerable<FlightRm>),200)]
    [HttpGet]
    public IEnumerable<FlightRm> Search([FromQuery]FlightSearchParameters @params)
    {
        _logger.LogInformation("Searching for a flight for: {Destination}", @params.Destination);

        IQueryable<Flight> flights = _entities.Flights;

        if (!string.IsNullOrWhiteSpace(@params.Destination))
            flights = flights.Where(f => f.Arrival.Place.Contains(@params.Destination));

        if (!string.IsNullOrWhiteSpace(@params.From))
            flights = flights.Where(f => f.Departure.Place.Contains(@params.From));

        if (@params.FromDate != null)
            flights = flights.Where(f => f.Departure.Time >= @params.FromDate.Value.Date);

        if(@params.ToDate != null)
            flights = flights.Where(f=>f.Arrival.Time <= @params.ToDate.Value.Date.AddDays(1).AddTicks(-1));

        if (@params.NumberOfPassengers != null && @params.NumberOfPassengers != 0)
            flights = flights.Where(f => f.RemainingNumberOfSeats >= @params.NumberOfPassengers);
        else
            flights = flights.Where(f => f.RemainingNumberOfSeats > 0);



        var flightRmList = flights
            .Select(flight => new FlightRm(
            flight.Id,
            flight.Airline,
            flight.Price,
            new TimePlaceRm(flight.Departure.Place.ToString(), flight.Departure.Time),
            new TimePlaceRm(flight.Arrival.Place.ToString(), flight.Arrival.Time),
            flight.RemainingNumberOfSeats
            ));

        return flightRmList;
    }

    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ProducesResponseType(typeof(FlightRm), 200)]
    [HttpGet("{id}")]

    public ActionResult<FlightRm> Find(Guid id)
    {
        var flight = _entities.Flights.SingleOrDefault(f => f.Id == id);

        if(flight == null)
            return NotFound();

        var readModel = new FlightRm(
            flight.Id,
            flight.Airline,
            flight.Price,
            new TimePlaceRm(flight.Departure.Place.ToString(), flight.Departure.Time),
            new TimePlaceRm(flight.Arrival.Place.ToString(), flight.Arrival.Time),
            flight.RemainingNumberOfSeats
            );

        return Ok(readModel);
    }


    [HttpPost]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(200)]
    public IActionResult Book(BookDto dto)
    {
        System.Diagnostics.Debug.WriteLine($"Booking new flight {dto.FlightId}");


        var flight = _entities.Flights.SingleOrDefault(f => f.Id == dto.FlightId );

        if(flight == null) return NotFound();

        var error = flight.MakeBooking(dto.PassengerEmail, dto.NumberOfSeats);


        if (error is OverbookError)
            return Conflict(new { message = "Not enough seats." });

        try
        {
            _entities.SaveChanges();
        }catch (DbUpdateConcurrencyException e)
        {
            return Conflict(new { message = "An error occured while booking. Please try again." });
        }

        return CreatedAtAction(nameof(Find), new { id = dto.FlightId });
    }


}