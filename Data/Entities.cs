using Flights.Domain.Entities;
using System;

namespace Flights.Data
{
    public class Entities
    {
        public IList<Passenger> Passengers = new List<Passenger>();
        static Random random = new Random();
        public List<Flight> Flights = new List<Flight>();
    }
}
