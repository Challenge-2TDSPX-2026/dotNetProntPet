using System;

namespace ProntPet.Models;

public class Consultation
{

    public int Id { get; set; }
    public int IdMedicalRecord { get; set; }
    public MedicalRecord MedicalRecord { get; set; }
    public int IdClinic { get; set; } 
    public Clinic Clinic { get; set; }
    public DateTime ConsultationDate { get; set; }
    public string Symptoms { get; set; }
    public string Diagnosis { get; set; }
    public string Observations { get; set; }
    

}
