using ProntPet.Common;

namespace ProntPet.Dtos;

/// <summary>Paginação, ordenação e filtros da listagem de tutores.</summary>
public class TutorQueryParameters : QueryParameters
{
    /// <summary>Nome contém o texto (sem diferenciar maiúsculas).</summary>
    public string? Name { get; set; }

    /// <summary>E-mail contém o texto (sem diferenciar maiúsculas).</summary>
    public string? Email { get; set; }

    /// <summary>CPF contém o texto.</summary>
    public string? Cpf { get; set; }
}
