using System;
namespace aiRbPG.Domain.Reservations
{
public static class ReservationRules
{
    public static bool PeriodsOverlap(DateTime start, DateTime end, DateTime otherStart, DateTime otherEnd) =>
        start < otherEnd && end > otherStart;

    public static string Validate(DateTime start, DateTime end, int people, int spaceCapacity)
    {
        if (end <= start)
        {
            return "O horário de término deve ser posterior ao início.";
        }

        if (people <= 0)
        {
            return "A reserva deve ter pelo menos uma pessoa.";
        }

        if (spaceCapacity <= 0 || people > spaceCapacity)
        {
            return "A quantidade de pessoas excede a capacidade do espaço.";
        }

        return null;
    }
}
}
