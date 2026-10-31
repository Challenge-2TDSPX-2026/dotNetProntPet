using ProntPet.Common;
using ProntPet.Models;

namespace ProntPet.Dtos;

public record class ConsultationResponse(
    int Id,
    int IdMedicalRecord,
    int IdClinic,
    DateTime ConsultationDate,
    string? Symptoms,
    string? Diagnosis,
    string? Observations,
    List<LinkDto> Links
)
{
    public static ConsultationResponse FromEntity(Consultation c, List<LinkDto> links) => new(
        c.Id, c.IdMedicalRecord, c.IdClinic, c.ConsultationDate, c.Symptoms, c.Diagnosis, c.Observations, links);
}
