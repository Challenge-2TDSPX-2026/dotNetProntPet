using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia prontuários médicos vinculados a pets.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MedicalRecordController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Busca o prontuário de um pet.
        /// </summary>
        /// <remarks>
        /// Filtra o prontuário médico pelo identificador do pet informado.
        /// </remarks>
        /// <param name="idPet">Identificador do pet.</param>
        /// <returns>Prontuário do pet.</returns>
        /// <response code="200">Prontuário retornado com sucesso (pode ser lista vazia).</response>
        [HttpGet("pet/{idPet}")]
        public async Task<IActionResult> GetMedicalRecordsByPet(int idPet)
        {
            var records = await _context
                .MedicalRecords.Where(mc => mc.IdPet == idPet).ToListAsync();

            return Ok(records);
        }

        /// <summary>
        /// Obtém um prontuário médico pelo identificador.
        /// </summary>
        /// <remarks>
        /// Busca um único prontuário pelo seu ID. Retorna 404 se não existir.
        /// </remarks>
        /// <param name="id">Identificador do prontuário.</param>
        /// <returns>Dados do prontuário encontrado.</returns>
        /// <response code="200">Prontuário encontrado.</response>
        /// <response code="404">Prontuário não encontrado.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null) return NotFound($"Prontuário de id {id} não encontrado!");
            return Ok(record);
        }

        /// <summary>
        /// Cadastra um novo prontuário médico.
        /// </summary>
        /// <remarks>
        /// Cria um prontuário vinculado ao pet informado em <c>IdPet</c>.
        /// Retorna 404 se o pet não existir.
        /// </remarks>
        /// <param name="recordRequest">Dados do prontuário a ser criado.</param>
        /// <returns>Prontuário criado com o identificador gerado.</returns>
        /// <response code="201">Prontuário criado com sucesso.</response>
        /// <response code="404">Pet informado não encontrado.</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MedicalRecordRequest recordRequest)
        {
            var petExists = await _context
                .Pets.AnyAsync(p => p.Id == recordRequest.IdPet);

            if (!petExists)
            {
                return NotFound($"O pet de id {recordRequest.IdPet} não foi encontrado!");
            }

            var record = recordRequest.ToEntity();
            
            // data da última atualização é setado no momento da criação
            record.LastUpdate = DateTime.UtcNow;

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = record.Id}, record);

        }

        /// <summary>
        /// Atualiza os dados de um prontuário existente.
        /// </summary>
        /// <remarks>
        /// Atualiza tipo sanguíneo, alergias, doenças crônicas, castração, microchip e data da última atualização.
        /// </remarks>
        /// <param name="id">Identificador do prontuário.</param>
        /// <param name="updatedRecord">Novos dados do prontuário.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Prontuário atualizado com sucesso.</response>
        /// <response code="404">Prontuário não encontrado.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MedicalRecordRequest updatedRecord)
        {

            //var updatedRecord = updatedRecordRequest.ToEntity(); 

            var record = await _context.MedicalRecords.FindAsync(id);

            if (record == null) return NotFound($"O Protuário de id {id} não encontrado!");

            record.Update(updatedRecord.BloodType, updatedRecord.Allergies, 
                            updatedRecord.ChronicDiseases, updatedRecord.IsCastrated, 
                            updatedRecord.MicrochipCode, DateTime.UtcNow);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Remove um prontuário do sistema.
        /// </summary>
        /// <remarks>
        /// Exclui permanentemente o prontuário identificado por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador do prontuário.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Prontuário removido com sucesso.</response>
        /// <response code="404">Prontuário não encontrado.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);

            if (record == null) return NotFound($"Protuário de id {id} não encontrado!");

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
