namespace aiRbPG.Domain.Pricing;

public sealed record AccessoryCharge(int Quantity, decimal HourlyRate);

public sealed record PriceQuote(
    decimal SpaceSubtotal,
    decimal PeopleSubtotal,
    decimal AccessoriesSubtotal,
    decimal Discount,
    decimal Fines,
    decimal Total);
