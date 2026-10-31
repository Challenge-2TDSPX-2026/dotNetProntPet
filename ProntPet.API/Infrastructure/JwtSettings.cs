namespace ProntPet.Infrastructure;

/// <summary>Configuração do JWT (seção "Jwt" do appsettings).</summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Chave simétrica de assinatura (HS256). Mínimo de 32 caracteres. Não versionar a chave real.</summary>
    public string Key { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public string Audience { get; set; } = null!;

    /// <summary>Validade do token em minutos (padrão 120).</summary>
    public int ExpirationMinutes { get; set; } = 120;
}
