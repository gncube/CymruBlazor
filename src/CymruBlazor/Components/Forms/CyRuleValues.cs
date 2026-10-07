using System.Globalization;

namespace CymruBlazor.Components.Forms;

/// <summary>Reads and compares the loosely typed values stored on a <see cref="CyRuleCondition"/>.</summary>
internal static class CyRuleValues
{
    internal static bool IsBlank(object? value) =>
        value is null || (value is string s && string.IsNullOrWhiteSpace(s));

    internal static bool TryNumber(object? value, out decimal number)
    {
        number = 0;

        switch (value)
        {
            case decimal d:
                number = d;
                return true;
            case int i:
                number = i;
                return true;
            case long l:
                number = l;
                return true;
            case double dbl when double.IsFinite(dbl):
                number = (decimal)dbl;
                return true;
            case float f when float.IsFinite(f):
                number = (decimal)f;
                return true;
            case string s:
                return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out number);
            default:
                return false;
        }
    }

    internal static bool TryDate(object? value, out DateOnly date)
    {
        date = default;

        switch (value)
        {
            case DateOnly d:
                date = d;
                return true;
            case DateTime dt:
                date = DateOnly.FromDateTime(dt);
                return true;
            case string s:
                return DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
            default:
                return false;
        }
    }
}
