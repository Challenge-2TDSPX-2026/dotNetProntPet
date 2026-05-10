using ProntPet.Models;

namespace ProntPet.dtos;

public record class PetRequest(
    int IdTutor,
    string Name,
    string Species,
    string Breed,
    int Age,
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
            Age = this.Age,
            Weight = this.Weight,
            Sex = this.Sex

        };
       
    }
}
