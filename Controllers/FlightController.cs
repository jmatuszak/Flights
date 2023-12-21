using Flights.ReadModel;
using Microsoft.AspNetCore.Mvc;
using System;
using Flights.Domain.Entities;
using Flights.Dtos;
using Flights.Domain.Errors;
using Flights.Data;

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
    public IEnumerable<FlightRm> Search()
    {
        var flightRmList = _entities.Flights.Select(flight => new FlightRm(
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

        _entities.SaveChanges();

        return CreatedAtAction(nameof(Find), new { id = dto.FlightId });
    }


}