using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProntPet.Models;

[Table("DB_VACCINATION")]
public class Vaccination
{

    [Key]
    [Column("ID")]
    public int Id {get; set; }

    [Required]
    [Column("ID_PET")]
    public int IdPet {get; set; }

    [JsonIgnore]
    [ForeignKey("IdPet")]
    public Pet Pet {get; set; }

    [MaxLength(50)]
    [Required]
    [Column("VACCINE_NAME")]
    public string VaccineName;

    [Required]
    [Column("APPLICATION_DATE")]
    public DateOnly ApplicationDate;

    [Required]
    [Column("EXPIRATION_DATE")]
    public DateOnly ExpirationDate;

    [MaxLength(10)]
    [Required]
    [Column("LOT")]
    public string Lot;

    public void Update(string vaccineName, DateOnly applicationDate, 
                        DateOnly expirationDate, string lot)
    {
        this.VaccineName = vaccineName;
        this.ApplicationDate = applicationDate;
        this.ExpirationDate = expirationDate;
        this.Lot = lot;
    }

}
