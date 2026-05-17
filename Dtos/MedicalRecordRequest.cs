using System.Security.Cryptography.X509Certificates;
using ProntPet.Models;

namespace ProntPet.Dtos;

public record class MedicalRecordRequest(
    int IdPet,
    string BloodType,
    string? Allergies,
    string? ChronicDiseases,
    bool? IsCastrated,
    string? MicrochipCode,
    DateOnly LastUpdate
)
{

    public MedicalRecord ToEntity()
    {
        return new MedicalRecord
        {
            IdPet = this.IdPet,
            BloodType = this.BloodType,
            Allergies = this.Allergies,
            ChronicDiseases = this.ChronicDiseases,
            IsCastrated = this.IsCastrated,
            MicrochipCode = this.MicrochipCode,
            LastUpdate = this.LastUpdate.ToDateTime(TimeOnly.MinValue)
        };
        
    }

}
