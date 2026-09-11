using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ProntPet.Models;

[Table("DB_MEDICAL_RECORD")]
[Index(nameof(IdPet), IsUnique = true)]
[Index(nameof(MicrochipCode), IsUnique = true)]
public class MedicalRecord
{

    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("ID_PET")]
    public int IdPet { get; set; }

    [JsonIgnore]
    [ForeignKey("IdPet")]
    public Pet Pet { get; set; }

    [Required]
    [MaxLength(5)]
    [Column("BLOOD_TYPE")]
    public string BloodType { get; set; }

    [MaxLength(500)]
    [Column("ALLERGIES")]
    public string? Allergies { get; set; }

    [MaxLength(500)]
    [Column("CHRONIC_DISEASES")]
    public string? ChronicDiseases { get; set; }

    [Required]
    [Column("IS_CASTRATED")]
    public bool IsCastrated { get; set; }

    [MaxLength(50)]
    [Column("MICROCHIP_CODE")]
    public string? MicrochipCode { get; set; }

    [Required]
    [Column("LAST_UPDATE")]
    public DateTime LastUpdate { get; set; }

    public void Update(string bloodType, string? allergies, string? chronicDiseases, 
                        bool isCastrated, string? microchipCode, DateTime lastUpdate)
    {
        this.BloodType = bloodType;
        this.Allergies = allergies;
        this.ChronicDiseases = chronicDiseases;
        this.IsCastrated = isCastrated;
        this.MicrochipCode = microchipCode;
        this.LastUpdate = lastUpdate;
    }

}
