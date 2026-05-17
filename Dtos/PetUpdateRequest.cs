using System.ComponentModel.DataAnnotations;
using ProntPet.Models;

namespace ProntPet.dtos;

public record class PetUpdateRequest(
    [Required] string Name,
    [Required] string Species,
    string? Breed,
    DateOnly? BirthDate,
    decimal? Weight,
    [Required] string Sex
)
{

    public Pet ToEntity()
    {

        return new Pet
        {
            Name = this.Name,
            Species = this.Species,
            Breed = this.Breed,
            BirthDate = this.BirthDate?.ToDateTime(TimeOnly.MinValue),            
            Weight = this.Weight,
            Sex = this.Sex

        };
       
    }

}
