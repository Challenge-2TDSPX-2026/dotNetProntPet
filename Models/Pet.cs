using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProntPet.Models;

[Table("DB_PET")]
public class Pet
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("ID_TUTOR")]
    public int IdTutor { get; set; }

    [JsonIgnore] // Evita referência circular durante a serialização
    [ForeignKey("IdTutor")]
    public Tutor Tutor { get; set; }

    [MaxLength(100)]
    [Column("NAME")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "A Espécie do pet é obrigatória")]
    [MaxLength(50)]
    [Column("SPECIES")]
    public string Species { get; set; }

    [MaxLength(50)]
    [Column("BREED")]
    public string? Breed { get; set; }

    [Column("BIRTH_DATE")]
    public DateTime? BirthDate { get; set; }

    [Column("WEIGHT", TypeName = "NUMBER(5,2)")]
    public decimal? Weight { get; set; }

    [Required(ErrorMessage = "O Sexo do pet é obrigatório")]
    [MaxLength(15)]
    [Column("SEX")]
    public string Sex { get; set; }


    public void Update(string? name, string species, string? breed, DateTime? birthDate,
                         decimal? weight, string sex)
    {
        this.Name = name;
        this.Species = species;
        this.Breed = breed;
        this.BirthDate = birthDate;
        this.Weight = weight;
        this.Sex = sex;
    }

}
