namespace ProntPet.Common;

public static class DateTimeExtensions
{
    /// <summary>
    /// Normaliza para UTC. Datas sem fuso (ex.: "2026-10-20" na query string)
    /// são interpretadas como UTC, e não como horário local do servidor.
    /// </summary>
    public static DateTime ToUtcAssumingUtc(this DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
}