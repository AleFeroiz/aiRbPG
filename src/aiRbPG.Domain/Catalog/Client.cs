namespace aiRbPG.Domain.Catalog
{
    public sealed class Client
    {
        public Client(int id, string name, string phone, string email, bool isActive = true)
        { Id = id; Name = name; Phone = phone; Email = email; IsActive = isActive; }
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Phone { get; private set; }
        public string Email { get; private set; }
        public bool IsActive { get; private set; }
    }
}
