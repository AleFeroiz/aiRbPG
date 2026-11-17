namespace aiRbPG.Domain.Catalog
{
    public sealed class RpgSpace
    {
        public RpgSpace(int id, string name, string description, string address, int capacity, decimal hourlyRate, decimal perPersonHourlyRate, bool isActive = true)
        {
            Id = id; Name = name; Description = description; Address = address; Capacity = capacity;
            HourlyRate = hourlyRate; PerPersonHourlyRate = perPersonHourlyRate; IsActive = isActive;
        }
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Address { get; private set; }
        public int Capacity { get; private set; }
        public decimal HourlyRate { get; private set; }
        public decimal PerPersonHourlyRate { get; private set; }
        public bool IsActive { get; private set; }
    }
}
