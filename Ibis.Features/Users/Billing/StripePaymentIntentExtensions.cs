using Stripe;
using System.Globalization;

namespace Ibis.Users;

public static class StripePaymentIntentExtensions
{
    public static long StripeAmount(decimal amount, string currency)
    {
        return (long)(amount * StripeCurrencyMultiplier(currency));
    }

    public static decimal LocalAmount(this PaymentIntent paymentIntent)
    {
        return paymentIntent.Amount / StripeCurrencyMultiplier(paymentIntent.Currency);
    }
    

    public static string FormattedAmount(this PaymentIntent paymentIntent)
    {
        TryGetCurrencySymbol(paymentIntent.Currency, out string? symbol);
        return $"{symbol ?? paymentIntent.Currency.ToUpper()}{paymentIntent.Amount / StripeCurrencyMultiplier(paymentIntent.Currency):0.##}";
    }

    public static List<string> ZeroDecimalCurrencies = new() { "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf" };

    static decimal StripeCurrencyMultiplier(string currency) => ZeroDecimalCurrencies.Contains(currency.ToLower()) ? 1 : 100;

    static bool TryGetCurrencySymbol(string ISOCurrencySymbol, out string? symbol)
    {
        ISOCurrencySymbol = ISOCurrencySymbol.ToUpper();

        symbol = CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Where(c => !c.IsNeutralCulture)
            .Select(culture =>
            {
                try
                {
                    return new RegionInfo(culture.Name);
                }
                catch
                {
                    return null;
                }
            })
            .Where(ri => ri != null && ri.ISOCurrencySymbol == ISOCurrencySymbol)
            .Select(ri => ri!.CurrencySymbol)
            .FirstOrDefault();
        return symbol != null;
    }
}

