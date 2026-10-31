using ProntPet.Common;
using ProntPet.Models;

namespace ProntPet.dtos;

public record class TutorResponse(
    int id,
    string Name,
    string Cpf,
    string Phone,
    string Email,
    string Address,
    List<LinkDto> Links
)
{
    public static TutorResponse FromEntity(Tutor tutor, List<LinkDto>? links = null)
    {
        return new TutorResponse(
            tutor.Id,
            tutor.Name,
            tutor.Cpf,
            tutor.Phone,
            tutor.Email,
            tutor.Address,
            links ?? new List<LinkDto>()
        );
    }
}
