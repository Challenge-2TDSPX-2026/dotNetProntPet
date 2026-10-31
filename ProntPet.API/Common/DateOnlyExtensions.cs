using System.Globalization;

namespace ProntPet.Common;

public static class DateOnlyExtensions
{
    /// <summary>Formata como yyyy-MM-dd (independente de cultura) para uso nos links HATEOAS.</summary>
    public static string? ToIsoDate(this DateOnly? value) =>
        value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
