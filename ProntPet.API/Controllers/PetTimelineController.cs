using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia a linha do tempo clínica dos pets (eventos e anotações armazenados no MongoDB).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PetTimelineController : ControllerBase
    {
        private readonly IPetTimelineService _service;

        public PetTimelineController(IPetTimelineService service)
        {
            _service = service;
        }

        /// <summary>Lista os registros da timeline de um pet (mais recentes primeiro).</summary>
        /// <param name="idPet">Identificador do pet.</param>
        /// <response code="200">Registros retornados (pode ser lista vazia).</response>
        [HttpGet("pet/{idPet:int}")]
        public async Task<IActionResult> GetByPet(int idPet)
        {
            var entries = await _service.GetByPetAsync(idPet);
            return Ok(entries);
        }

        /// <summary>Obtém um registro da timeline pelo identificador (ObjectId, 24 caracteres).</summary>
        /// <response code="200">Registro encontrado.</response>
        /// <response code="404">Registro não encontrado.</response>
        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(result.Value),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>Cria um registro na timeline de um pet.</summary>
        /// <response code="201">Registro criado.</response>
        /// <response code="404">Pet não encontrado.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PetTimelineRequest request)
        {
            var result = await _service.CreateAsync(request);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>Atualiza um registro da timeline.</summary>
        /// <response code="204">Registro atualizado.</response>
        /// <response code="404">Registro não encontrado.</response>
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody] PetTimelineUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>Remove um registro da timeline.</summary>
        /// <response code="204">Registro removido.</response>
        /// <response code="404">Registro não encontrado.</response>
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _service.DeleteAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }
    }
}