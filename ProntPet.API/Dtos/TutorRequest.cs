using ProntPet.Models;

namespace ProntPet.Dtos;

public record class TutorRequest(
    string Name,
    string Cpf,
    string Phone,
    string Email,
    string Password,
    string Address
)
{
    public Tutor ToEntity()
    {
        return new Tutor
        {
            Name = this.Name,
            Cpf = this.Cpf,
            Phone = this.Phone,
            Email = this.Email,
            Password = this.Password,
            Address = this.Address
        };
    }
}
