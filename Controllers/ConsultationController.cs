using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.Models;

namespace ProntPet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationController : ControllerBase
    {
        
        private readonly AppDbContext _context;

        public ConsultationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("medical-record/{idRecord}")]
        public async Task<IActionResult> GetConsultationsByMedicalRecord(int idRecord)
        {
            var consultations = await _context
                .Consultations
                .Where(c => c.IdMedicalRecord == idRecord)
                .ToListAsync();

            return Ok(consultations);

            
        }

        [HttpGet("clinic/{idClinic}")]
        public async Task<IActionResult> GetConsultationsByClinic(int idClinic)
        {
            var consultations = await _context
                .Consultations
                .Where(c => c.IdClinic == idClinic)
                .ToListAsync();

            return Ok(consultations);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ConsultationRequest consultationRequest)
        {
            // Converte para entidade
            var consultation = consultationRequest.ToEntity();
            // verifica se o prontuário existe
            var recordExists = await _context
                .MedicalRecords
                .AnyAsync(mc => mc.Id == consultation.IdMedicalRecord);

            if (!recordExists)
            {
                return NotFound($"O protuário de id {consultation.IdMedicalRecord} não foi encontrado!");
            }

            // Verifica se a clínica existe
            var clinicExists = await _context
                .Clinics
                .AnyAsync(c => c.Id == consultation.IdClinic);

            if (!clinicExists)
            {
                return NotFound($"A clínica de id {consultation.IdClinic} não foi encontrada!");
            }

            // add
            _context.Consultations.Add(consultation);
            // savechanges
            await _context.SaveChangesAsync();
            // return ok
            return Ok(consultation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ConsultationRequest request)
        {

            var updatedConsultation = request.ToEntity();

            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null) return NotFound();
            consultation.Update(updatedConsultation.ConsultationDate, updatedConsultation.Symptoms, updatedConsultation.Diagnosis, updatedConsultation.Observations);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null) return NotFound();
            _context.Consultations.Remove(consultation);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
