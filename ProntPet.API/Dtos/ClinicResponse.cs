using ProntPet.Common;
using ProntPet.Models;

namespace ProntPet.Dtos;

public record class ClinicResponse(
    int Id,
    string Name,
    string Cnpj,
    string Address,
    List<LinkDto> Links
)
{
    public static ClinicResponse FromEntity(Clinic clinic, List<LinkDto> links) => new(
        clinic.Id, clinic.Name, clinic.Cnpj, clinic.Address, links);
}
