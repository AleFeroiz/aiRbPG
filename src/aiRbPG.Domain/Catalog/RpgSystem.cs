namespace aiRbPG.Domain.Catalog
{
    public sealed class RpgSystem
    {
        public RpgSystem(int id, string name, string description, bool isActive = true)
        { Id = id; Name = name; Description = description; IsActive = isActive; }
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsActive { get; private set; }
    }
}
