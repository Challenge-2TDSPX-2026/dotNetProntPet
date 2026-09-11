using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia o cadastro e as operações de clínicas veterinárias.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClinicController : ControllerBase
    {
        private readonly IClinicService _clinicService;

        public ClinicController(IClinicService clinicService)
        {
            _clinicService = clinicService;
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
            var clinics = await _clinicService.GetAllAsync();
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
            var result = await _clinicService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(result.Value),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
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
            var result = await _clinicService.CreateAsync(clinicRequest);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
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
            var result = await _clinicService.UpdateAsync(id, updatedClinicRequest);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
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
            var result = await _clinicService.DeleteAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

    }
}
