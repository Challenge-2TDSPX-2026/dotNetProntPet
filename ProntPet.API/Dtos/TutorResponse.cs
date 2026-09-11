using ProntPet.Models;

namespace ProntPet.dtos;

public record class TutorResponse(
    int id,
    string Name,
    string Cpf,
    string Phone,
    string Email,
    string Address
)
{
    public static TutorResponse FromEntity(Tutor tutor)
    {
        return new TutorResponse(
            tutor.Id,
            tutor.Name,
            tutor.Cpf,
            tutor.Phone,
            tutor.Email,
            tutor.Address
        );
    }
}
