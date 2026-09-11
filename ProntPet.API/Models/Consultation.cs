using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProntPet.Models;

[Table("DB_CONSULTATION")]
public class Consultation
{

    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("ID_MEDICAL_RECORD")]
    public int IdMedicalRecord { get; set; }

    [JsonIgnore]
    [ForeignKey("IdMedicalRecord")]
    public MedicalRecord MedicalRecord { get; set; }

    [Required]
    [Column("ID_CLINIC")]
    public int IdClinic { get; set; } 

    [JsonIgnore]
    [ForeignKey("IdClinic")]
    public Clinic Clinic { get; set; }

    [Required]
    [Column("CONSULTATION_DATE")]
    public DateTime ConsultationDate { get; set; }

    [MaxLength(100)]
    [Column("SYMPTOMS")]
    public string? Symptoms { get; set; }

    [MaxLength(200)]
    [Column("DIAGNOSIS")]
    public string? Diagnosis { get; set; }

    [MaxLength(500)]
    [Column("OBSERVATIONS")]
    public string? Observations { get; set; }


    public void Update(DateTime consultationDate, string? symptoms, string? diagnosis, string? observations)
    {
        this.ConsultationDate = consultationDate;
        this.Symptoms =  symptoms;
        this.Diagnosis = diagnosis;
        this.Observations = observations;
    }
    

}
