using ProntPet.Models;

namespace ProntPet;

public record class ClinicRequest(
    string Name,
    string Cnpj,
    string Address
)
{

    public Clinic ToEntity()
    {
        return new Clinic
        {
            Name = this.Name,
            Cnpj = this.Cnpj,
            Address = this.Address
        };
    }

}
