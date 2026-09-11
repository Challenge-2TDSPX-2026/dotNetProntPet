using ProntPet.Models;

namespace ProntPet;

public record class ConsultationRequest(
    int IdMedicalRecord,
    int IdClinic,
    DateOnly ConsultationDate,
    string? Symptoms,
    string? Diagnosis,
    string? Observations

)
{

    public Consultation ToEntity()
    {
        return new Consultation
        {
            IdMedicalRecord = this.IdMedicalRecord,
            IdClinic = this.IdClinic,
            ConsultationDate = this.ConsultationDate.ToDateTime(TimeOnly.MinValue),
            Symptoms = this.Symptoms,
            Diagnosis = this.Diagnosis,
            Observations = this.Observations 
        };
    }   

}
