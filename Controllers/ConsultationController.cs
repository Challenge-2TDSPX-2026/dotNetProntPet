using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.Models;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia consultas veterinárias vinculadas a prontuários e clínicas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationController : ControllerBase
    {
        
        private readonly AppDbContext _context;

        public ConsultationController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista as consultas de um prontuário médico.
        /// </summary>
        /// <remarks>
        /// Retorna todas as consultas associadas ao prontuário identificado por <paramref name="idRecord"/>.
        /// </remarks>
        /// <param name="idRecord">Identificador do prontuário médico.</param>
        /// <returns>Lista de consultas do prontuário.</returns>
        /// <response code="200">Consultas retornadas com sucesso (pode ser lista vazia).</response>
        [HttpGet("medical-record/{idRecord}")]
        public async Task<IActionResult> GetConsultationsByMedicalRecord(int idRecord)
        {
            var consultations = await _context
                .Consultations
                .Where(c => c.IdMedicalRecord == idRecord)
                .ToListAsync();

            return Ok(consultations);

            
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null) return NotFound($"Consulta de id {id} não encontrada!");
            return Ok(consultation);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ConsultationRequest consultationRequest)
        {
            var consultation = consultationRequest.ToEntity();
            var recordExists = await _context
                .MedicalRecords
                .AnyAsync(mc => mc.Id == consultation.IdMedicalRecord);

            if (!recordExists)
            {
                return NotFound($"O protuário de id {consultation.IdMedicalRecord} não foi encontrado!");
            }

            var clinicExists = await _context
                .Clinics
                .AnyAsync(c => c.Id == consultation.IdClinic);

            if (!clinicExists)
            {
                return NotFound($"A clínica de id {consultation.IdClinic} não foi encontrada!");
            }

            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new {id = consultation.Id}, consultation);
        }

        /// <summary>
        /// Atualiza os dados de uma consulta existente.
        /// </summary>
        /// <remarks>
        /// Atualiza data, sintomas, diagnóstico e observações da consulta identificada por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador da consulta.</param>
        /// <param name="request">Novos dados da consulta.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Consulta atualizada com sucesso.</response>
        /// <response code="404">Consulta não encontrada.</response>
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

        /// <summary>
        /// Remove uma consulta do sistema.
        /// </summary>
        /// <remarks>
        /// Exclui permanentemente a consulta identificada por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador da consulta.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Consulta removida com sucesso.</response>
        /// <response code="404">Consulta não encontrada.</response>
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
