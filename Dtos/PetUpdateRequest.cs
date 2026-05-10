using System.ComponentModel.DataAnnotations;

namespace ProntPet.dtos;

public record class PetUpdateRequest(
    [Required] string Name,
    [Required] string Species,
    string Breed,
    int Age,
    decimal Weight,
    [Required] string Sex
)
{

}
