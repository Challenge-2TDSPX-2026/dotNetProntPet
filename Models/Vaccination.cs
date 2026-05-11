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
    public string VaccineName {get; set; }

    [Required]
    [Column("APPLICATION_DATE")]
    public DateOnly ApplicationDate {get; set; }

    [Required]
    [Column("EXPIRATION_DATE")]
    public DateOnly ExpirationDate {get; set; }

    [MaxLength(10)]
    [Required]
    [Column("LOT")]
    public string Lot {get; set; }

    public void Update(string vaccineName, DateOnly applicationDate, 
                        DateOnly expirationDate, string lot)
    {
        this.VaccineName = vaccineName;
        this.ApplicationDate = applicationDate;
        this.ExpirationDate = expirationDate;
        this.Lot = lot;
    }

}
