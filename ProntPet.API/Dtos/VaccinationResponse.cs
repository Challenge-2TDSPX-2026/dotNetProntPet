using ProntPet.Common;
using ProntPet.Models;

namespace ProntPet.Dtos;

public record class VaccinationResponse(
    int Id,
    int IdPet,
    string VaccineName,
    DateTime ApplicationDate,
    DateTime ExpirationDate,
    string Lot,
    List<LinkDto> Links
)
{
    public static VaccinationResponse FromEntity(Vaccination v, List<LinkDto> links) => new(
        v.Id, v.IdPet, v.VaccineName, v.ApplicationDate, v.ExpirationDate, v.Lot, links);
}
