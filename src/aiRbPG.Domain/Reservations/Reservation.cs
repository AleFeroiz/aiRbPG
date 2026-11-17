using aiRbPG.Domain.Catalog;
using System;
using System.Collections.Generic;

namespace aiRbPG.Domain.Reservations
{
    public sealed class ReservationAccessory
    {
        public ReservationAccessory(int accessoryId, string name, int quantity, decimal hourlyRate)
        { AccessoryId = accessoryId; Name = name; Quantity = quantity; HourlyRate = hourlyRate; }
        public int AccessoryId { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public decimal HourlyRate { get; private set; }
    }

    public sealed class Reservation
    {
        public Reservation(int id, string clientName, string spaceName, DateTime start, DateTime end, int people, string status, decimal total)
        { Id = id; ClientName = clientName; SpaceName = spaceName; Start = start; End = end; People = people; Status = status; Total = total; }
        public int Id { get; private set; }
        public string ClientName { get; private set; }
        public string SpaceName { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public int People { get; private set; }
        public string Status { get; private set; }
        public decimal Total { get; private set; }
    }

    public sealed class NewReservation
    {
        public NewReservation(int clientId, int spaceId, int createdByUserId, DateTime start, DateTime end, int people, string notes, IList<ReservationAccessory> accessories)
        { ClientId = clientId; SpaceId = spaceId; CreatedByUserId = createdByUserId; Start = start; End = end; People = people; Notes = notes; Accessories = accessories; }
        public int ClientId { get; private set; }
        public int SpaceId { get; private set; }
        public int CreatedByUserId { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public int People { get; private set; }
        public string Notes { get; private set; }
        public IList<ReservationAccessory> Accessories { get; private set; }
    }

    public sealed class AccessoryAvailability
    {
        public AccessoryAvailability(Accessory accessory, int available) { Accessory = accessory; Available = available; }
        public Accessory Accessory { get; private set; }
        public int Available { get; private set; }
    }
}
