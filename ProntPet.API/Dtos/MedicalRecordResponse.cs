using ProntPet.Common;
using ProntPet.Models;

namespace ProntPet.Dtos;

public record class MedicalRecordResponse(
    int Id,
    int IdPet,
    string BloodType,
    string? Allergies,
    string? ChronicDiseases,
    bool IsCastrated,
    string? MicrochipCode,
    DateTime LastUpdate,
    List<LinkDto> Links
)
{
    public static MedicalRecordResponse FromEntity(MedicalRecord r, List<LinkDto> links) => new(
        r.Id, r.IdPet, r.BloodType, r.Allergies, r.ChronicDiseases, r.IsCastrated,
        r.MicrochipCode, r.LastUpdate, links);
}
