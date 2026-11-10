namespace aiRbPG.Domain.Reservations;

public static class ReservationRules
{
    public static bool PeriodsOverlap(DateTime start, DateTime end, DateTime otherStart, DateTime otherEnd) =>
        start < otherEnd && end > otherStart;

    public static void Validate(DateTime start, DateTime end, int people, int spaceCapacity)
    {
        if (end <= start)
        {
            throw new ArgumentException("O horário de término deve ser posterior ao início.");
        }

        if (people <= 0)
        {
            throw new ArgumentException("A reserva deve ter pelo menos uma pessoa.");
        }

        if (spaceCapacity <= 0 || people > spaceCapacity)
        {
            throw new ArgumentException("A quantidade de pessoas excede a capacidade do espaço.");
        }
    }
}
