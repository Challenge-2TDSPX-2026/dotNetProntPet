using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.Models;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia o cadastro e as operações de clínicas veterinárias.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClinicController : ControllerBase
    {

        private readonly AppDbContext _context;

        public ClinicController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todas as clínicas cadastradas.
        /// </summary>
        /// <remarks>
        /// Retorna a coleção completa de clínicas sem filtros ou paginação.
        /// </remarks>
        /// <returns>Lista de clínicas.</returns>
        /// <response code="200">Clínicas retornadas com sucesso.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clinics = await _context.Clinics.ToListAsync();
            return Ok(clinics);
        }

        /// <summary>
        /// Obtém uma clínica pelo identificador.
        /// </summary>
        /// <remarks>
        /// Busca uma única clínica pelo seu ID. Retorna 404 se não existir.
        /// </remarks>
        /// <param name="id">Identificador da clínica.</param>
        /// <returns>Dados da clínica encontrada.</returns>
        /// <response code="200">Clínica encontrada.</response>
        /// <response code="404">Clínica não encontrada.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound($"Clínica de id {id} não encontrada!");
            return Ok(clinic);
        }

        /// <summary>
        /// Cadastra uma nova clínica.
        /// </summary>
        /// <remarks>
        /// Cria um registro de clínica com nome e endereço informados no corpo da requisição.
        /// </remarks>
        /// <param name="clinicRequest">Dados da clínica a ser criada.</param>
        /// <returns>Clínica criada com o identificador gerado.</returns>
        /// <response code="201">Clínica criada com sucesso.</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClinicRequest clinicRequest)
        {
            var clinic = clinicRequest.ToEntity();
            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = clinic.Id }, clinic);
        }

        /// <summary>
        /// Atualiza os dados de uma clínica existente.
        /// </summary>
        /// <remarks>
        /// Atualiza nome e endereço da clínica identificada por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador da clínica.</param>
        /// <param name="updatedClinicRequest">Novos dados da clínica.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Clínica atualizada com sucesso.</response>
        /// <response code="404">Clínica não encontrada.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClinicRequest updatedClinicRequest)
        {
            var updatedClinic = updatedClinicRequest.ToEntity();
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();
            clinic.Update(updatedClinic.Name, updatedClinic.Address);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Remove uma clínica do sistema.
        /// </summary>
        /// <remarks>
        /// Exclui permanentemente a clínica identificada por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador da clínica.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Clínica removida com sucesso.</response>
        /// <response code="404">Clínica não encontrada.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();

            _context.Clinics.Remove(clinic);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
