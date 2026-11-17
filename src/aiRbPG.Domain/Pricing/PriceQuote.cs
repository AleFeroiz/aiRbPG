namespace aiRbPG.Domain.Pricing
{
    public sealed class AccessoryCharge
    {
        public AccessoryCharge(int quantity, decimal hourlyRate) { Quantity = quantity; HourlyRate = hourlyRate; }
        public int Quantity { get; private set; }
        public decimal HourlyRate { get; private set; }
    }

    public sealed class PriceQuote
    {
        public PriceQuote(decimal spaceSubtotal, decimal peopleSubtotal, decimal accessoriesSubtotal, decimal discount, decimal fines, decimal total)
        {
            SpaceSubtotal = spaceSubtotal; PeopleSubtotal = peopleSubtotal; AccessoriesSubtotal = accessoriesSubtotal;
            Discount = discount; Fines = fines; Total = total;
        }
        public decimal SpaceSubtotal { get; private set; }
        public decimal PeopleSubtotal { get; private set; }
        public decimal AccessoriesSubtotal { get; private set; }
        public decimal Discount { get; private set; }
        public decimal Fines { get; private set; }
        public decimal Total { get; private set; }
    }
}
