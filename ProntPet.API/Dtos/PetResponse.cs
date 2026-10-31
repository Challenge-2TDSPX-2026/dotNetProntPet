using ProntPet.Common;
using ProntPet.Models;

namespace ProntPet.Dtos;

public record class PetResponse(
    int Id,
    int IdTutor,
    string? Name,
    string Species,
    string? Breed,
    DateTime? BirthDate,
    decimal? Weight,
    string Sex,
    List<LinkDto> Links
)
{
    public static PetResponse FromEntity(Pet pet, List<LinkDto> links) => new(
        pet.Id, pet.IdTutor, pet.Name, pet.Species, pet.Breed, pet.BirthDate, pet.Weight, pet.Sex, links);
}
