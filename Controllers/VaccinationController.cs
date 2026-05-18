using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia o registro de vacinações vinculadas a pets.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class VaccinationController : ControllerBase
    {
        
        private readonly AppDbContext _context;

        public VaccinationController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista as vacinações de um pet.
        /// </summary>
        /// <remarks>
        /// Filtra as vacinações pelo identificador do pet informado.
        /// </remarks>
        /// <param name="idPet">Identificador do pet.</param>
        /// <returns>Lista de vacinações do pet.</returns>
        /// <response code="200">Vacinações retornadas com sucesso (pode ser lista vazia).</response>
        [HttpGet("pet/{idPet}")]
        public async Task<IActionResult> GetVaccinationsByPet(int idPet)
        {
            var vaccinations = await _context
                .Vaccinations.Where(v => v.IdPet == idPet).ToListAsync();

            return Ok(vaccinations);
        }

        /// <summary>
        /// Obtém uma vacinação pelo identificador.
        /// </summary>
        /// <remarks>
        /// Busca um único registro de vacinação pelo seu ID. Retorna 404 se não existir.
        /// </remarks>
        /// <param name="id">Identificador da vacinação.</param>
        /// <returns>Dados da vacinação encontrada.</returns>
        /// <response code="200">Vacinação encontrada.</response>
        /// <response code="404">Vacinação não encontrada.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vaccination = await _context.Vaccinations.FindAsync(id);
            if (vaccination == null) return NotFound($"Vacinação de id {id} não encontrada!");
            return Ok(vaccination);
        }

        /// <summary>
        /// Registra uma nova vacinação.
        /// </summary>
        /// <remarks>
        /// Cria um registro de vacinação vinculado ao pet informado em <c>IdPet</c>.
        /// Retorna 404 se o pet não existir.
        /// </remarks>
        /// <param name="request">Dados da vacinação a ser registrada.</param>
        /// <returns>Vacinação criada com o identificador gerado.</returns>
        /// <response code="201">Vacinação registrada com sucesso.</response>
        /// <response code="404">Pet informado não encontrado.</response>
        /// <response code="400">Data de expiração inferior ou igual a da aplicação</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VaccinationRequest request)
        {
            var petExists = await _context
                .Pets.AnyAsync(p => p.Id == request.IdPet);

            if (!petExists)
            {
                return NotFound($"O pet de id {request.IdPet} não foi encontrado!");
            }

            if (request.ExpirationDate <= request.ApplicationDate )
            {
                return BadRequest("A data de expiração da vacina não pode ser anterior ou igual a data de aplicação");
            }

            var vaccination = request.ToEntity();
            _context.Vaccinations.Add(vaccination);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new {id = vaccination.Id}, vaccination);

        }

        /// <summary>
        /// Atualiza os dados de uma vacinação existente.
        /// </summary>
        /// <remarks>
        /// Atualiza nome da vacina, data de aplicação, validade e lote da vacinação identificada por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador da vacinação.</param>
        /// <param name="request">Novos dados da vacinação.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Vacinação atualizada com sucesso.</response>
        /// <response code="404">Vacinação não encontrada.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VaccinationRequest request)
        {
            var vaccination = await _context.Vaccinations.FindAsync(id);

            if (vaccination == null) return NotFound($"Vacinação de id {id} não encontrada!");

            var updatedVaccination = request.ToEntity();

            vaccination.Update(updatedVaccination.VaccineName, updatedVaccination.ApplicationDate, 
                                updatedVaccination.ExpirationDate, updatedVaccination.Lot);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Remove um registro de vacinação do sistema.
        /// </summary>
        /// <remarks>
        /// Exclui permanentemente a vacinação identificada por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador da vacinação.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Vacinação removida com sucesso.</response>
        /// <response code="404">Vacinação não encontrada.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vaccination = await _context.Vaccinations.FindAsync(id);

            if (vaccination == null) return NotFound($"Vacinação de id {id} não encontrada!");

            _context.Vaccinations.Remove(vaccination);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
