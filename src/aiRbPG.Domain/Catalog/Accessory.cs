namespace aiRbPG.Domain.Catalog
{
    public sealed class Accessory
    {
        public Accessory(int id, string name, string description, int quantityInStock, decimal hourlyRate, bool isActive = true)
        {
            Id = id; Name = name; Description = description; QuantityInStock = quantityInStock; HourlyRate = hourlyRate; IsActive = isActive;
        }
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int QuantityInStock { get; private set; }
        public decimal HourlyRate { get; private set; }
        public bool IsActive { get; private set; }
    }
}
