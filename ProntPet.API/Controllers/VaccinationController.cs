using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia o registro de vacinações vinculadas a pets.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class VaccinationController : ControllerBase
    {
        private readonly IVaccinationService _vaccinationService;

        public VaccinationController(IVaccinationService vaccinationService)
        {
            _vaccinationService = vaccinationService;
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
            var vaccinations = await _vaccinationService.GetByPetAsync(idPet);
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
            var result = await _vaccinationService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(result.Value),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
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
            var result = await _vaccinationService.CreateAsync(request);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
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
            var result = await _vaccinationService.UpdateAsync(id, request);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
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
            var result = await _vaccinationService.DeleteAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

    }
}
