using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia consultas veterinárias vinculadas a prontuários e clínicas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationService _consultationService;

        public ConsultationController(IConsultationService consultationService)
        {
            _consultationService = consultationService;
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
            var consultations = await _consultationService.GetByMedicalRecordAsync(idRecord);
            return Ok(consultations);
        }

        /// <summary>
        /// Obtém uma consulta pelo identificador.
        /// </summary>
        /// <remarks>
        /// Busca uma única consulta veterinária pelo seu ID. Retorna 404 se não existir.
        /// </remarks>
        /// <param name="id">Identificador da consulta.</param>
        /// <returns>Dados da consulta encontrada.</returns>
        /// <response code="200">Consulta encontrada.</response>
        /// <response code="404">Consulta não encontrada.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _consultationService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(result.Value),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Registra uma nova consulta veterinária.
        /// </summary>
        /// <remarks>
        /// Cria uma consulta vinculada a um prontuário e a uma clínica.
        /// Retorna 404 se o prontuário ou a clínica não existirem.
        /// </remarks>
        /// <param name="consultationRequest">Dados da consulta a ser criada.</param>
        /// <returns>Consulta criada com o identificador gerado.</returns>
        /// <response code="201">Consulta registrada com sucesso.</response>
        /// <response code="404">Prontuário ou clínica informados não encontrados.</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ConsultationRequest consultationRequest)
        {
            var result = await _consultationService.CreateAsync(consultationRequest);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
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
            var result = await _consultationService.UpdateAsync(id, request);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
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
            var result = await _consultationService.DeleteAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

    }
}
