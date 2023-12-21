using System.ComponentModel.DataAnnotations;

namespace Flights.Dtos
{
    public record Book(
        [Required]
        Guid FlightId,
        [Required]
        [EmailAddress]
        [StringLength(100, MinimumLength = 3)]
        string PassengerEmail,
        [Required]
        byte NumberOfSeats);
}
