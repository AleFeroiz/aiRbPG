using System;
using System.Collections.Generic;
using System.Linq;

namespace aiRbPG.Domain.Pricing
{

/// <summary>
/// Initial pricing proposal: hourly space rate plus an optional per-person hourly rate
/// and hourly accessory rates. Confirm this formula with the group before final delivery.
/// </summary>
public static class PricingCalculator
{
    public static PriceQuote Calculate(
        decimal hours,
        int people,
        decimal spaceHourlyRate,
        decimal perPersonHourlyRate,
        IEnumerable<AccessoryCharge> accessories,
        decimal discount = 0,
        decimal fines = 0)
    {
        if (accessories == null) throw new ArgumentNullException("accessories");
        if (hours <= 0 || people <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), "Informe duração e quantidade de pessoas positivas.");
        }

        if (spaceHourlyRate < 0 || perPersonHourlyRate < 0 || discount < 0 || fines < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(spaceHourlyRate), "Tarifas, descontos e multas não podem ser negativos.");
        }

        var charges = accessories.ToArray();
        if (charges.Any(item => item.Quantity <= 0 || item.HourlyRate < 0))
        {
            throw new ArgumentException("A quantidade de acessórios deve ser positiva e a tarifa não pode ser negativa.");
        }

        var spaceSubtotal = Round(hours * spaceHourlyRate);
        var peopleSubtotal = Round(hours * people * perPersonHourlyRate);
        var accessoriesSubtotal = charges.Sum(item => Round(hours * item.Quantity * item.HourlyRate));
        var rawTotal = spaceSubtotal + peopleSubtotal + accessoriesSubtotal + fines - discount;
        if (rawTotal < 0)
        {
            throw new ArgumentException("O desconto não pode exceder o subtotal acrescido das multas.");
        }

        return new PriceQuote(
            spaceSubtotal,
            peopleSubtotal,
            accessoriesSubtotal,
            Round(discount),
            Round(fines),
            Round(rawTotal));
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
}
