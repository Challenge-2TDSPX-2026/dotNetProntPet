using ProntPet.Common;

namespace ProntPet.Dtos;

/// <summary>Paginação, ordenação e filtros da listagem de clínicas.</summary>
public class ClinicQueryParameters : QueryParameters
{
    /// <summary>Nome contém o texto (sem diferenciar maiúsculas).</summary>
    public string? Name { get; set; }

    /// <summary>CNPJ contém o texto.</summary>
    public string? Cnpj { get; set; }

    /// <summary>Endereço contém o texto (sem diferenciar maiúsculas).</summary>
    public string? Address { get; set; }
}
