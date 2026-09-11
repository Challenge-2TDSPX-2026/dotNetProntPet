using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using ProntPet.Models;

namespace ProntPet.Dtos;

public record class MedicalRecordRequest(
    [Required] int IdPet,
    
    [Required]
    [MaxLength(5, ErrorMessage = "O tipo sanguíneo do pet não pode exceder 5 caracteres")] 
    string BloodType,
    
    [MaxLength(500)] string? Allergies,
    [MaxLength(500)] string? ChronicDiseases,
    bool IsCastrated,
    [MaxLength(50)] string? MicrochipCode
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
        };
        
    }

}
