using ProntPet.Common;

namespace ProntPet.Dtos;

/// <summary>Paginação, ordenação e filtros da listagem de pets de um tutor.</summary>
public class PetQueryParameters : QueryParameters
{
    /// <summary>Nome contém o texto (sem diferenciar maiúsculas).</summary>
    public string? Name { get; set; }

    /// <summary>Espécie igual ao texto (sem diferenciar maiúsculas).</summary>
    public string? Species { get; set; }

    /// <summary>Raça contém o texto (sem diferenciar maiúsculas).</summary>
    public string? Breed { get; set; }

    /// <summary>Sexo igual ao texto (sem diferenciar maiúsculas).</summary>
    public string? Sex { get; set; }
}
