using ProntPet.Models;

namespace ProntPet.dtos;

public record class PetRequest(
    int IdTutor,
    string Name,
    string Species,
    string Breed,
    DateOnly BirthDate,
    decimal Weight,
    string Sex
)
{
    public Pet ToEntity()
    {
        return new Pet
        {
            IdTutor = this.IdTutor,
            Name = this.Name,
            Species = this.Species,
            Breed = this.Breed,
            BirthDate = this.BirthDate.ToDateTime(TimeOnly.MinValue),
            Weight = this.Weight,
            Sex = this.Sex

        };
       
    }
}
