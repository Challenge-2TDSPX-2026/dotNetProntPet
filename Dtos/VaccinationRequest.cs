using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ProntPet.Models;

namespace ProntPet.Dtos;

public record class VaccinationRequest(
    [Required] int IdPet,
    [Required] string VaccineName,
    [Required] DateOnly ApplicationDate,
    [Required] DateOnly ExpirationDate,
    [Required] string Lot
)
{
    public Vaccination ToEntity()
    {
        return new Vaccination
        {
            IdPet = this.IdPet,
            VaccineName = this.VaccineName,
            ApplicationDate = this.ApplicationDate,
            ExpirationDate = this.ExpirationDate,
            Lot = this.Lot
        };
    }
}
