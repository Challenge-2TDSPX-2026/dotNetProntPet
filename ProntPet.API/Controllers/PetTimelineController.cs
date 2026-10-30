using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.API.Models;
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

        /// <summary>Lista a timeline de um pet com paginação, ordenação, filtros e links HATEOAS.</summary>
        /// <remarks>
        /// Padrão: <c>sortBy=occurredAt&amp;ascending=false</c> (mais recentes primeiro), 10 itens por página (máximo 50).
        /// Ordenações aceitas: occurredAt, title, type, createdAt. Informe <c>ascending</c> junto com <c>sortBy</c>.
        /// </remarks>
        /// <param name="idPet">Identificador do pet.</param>
        /// <param name="query">Paginação, ordenação e filtros.</param>
        /// <response code="200">Página de registros retornada (pode ser vazia).</response>
        /// <response code="400">Parâmetros inválidos (ex.: from maior que to).</response>
        [HttpGet("pet/{idPet:int}", Name = "GetPetTimelineByPet")]
        [ProducesResponseType(typeof(PagedResponse<PetTimelineResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPet(int idPet, [FromQuery] PetTimelineQueryParameters query)
        {
            var result = await _service.GetByPetAsync(idPet, query);

            if (result.Status != ServiceStatus.Ok)
                return BadRequest(result.Message);

            var page = result.Value!.Map(ToResponse);

            // Filtros e valores de rota que precisam ser repetidos nos links de navegação.
            var filters = new Dictionary<string, object?>
            {
                ["idPet"] = idPet,
                ["type"] = query.Type,
                ["tag"] = query.Tag,
                ["search"] = query.Search,
                ["from"] = ToIso(query.From),
                ["to"] = ToIso(query.To)
            };

            var links = PaginationLinkBuilder.Build(
                v => Url.Link("GetPetTimelineByPet", v), query, page.TotalPages, filters);

            return Ok(PagedResponse<PetTimelineResponse>.From(page, links));
        }

        /// <summary>Obtém um registro da timeline pelo identificador (ObjectId, 24 caracteres).</summary>
        /// <response code="200">Registro encontrado.</response>
        /// <response code="404">Registro não encontrado.</response>
        [HttpGet("{id:length(24)}", Name = "GetPetTimelineById")]
        [ProducesResponseType(typeof(PetTimelineResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>Cria um registro na timeline de um pet.</summary>
        /// <response code="201">Registro criado.</response>
        /// <response code="404">Pet não encontrado.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [ProducesResponseType(typeof(PetTimelineResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] PetTimelineRequest request)
        {
            var result = await _service.CreateAsync(request);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ToResponse(result.Value)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>Atualiza um registro da timeline.</summary>
        /// <response code="204">Registro atualizado.</response>
        /// <response code="404">Registro não encontrado.</response>
        [HttpPut("{id:length(24)}", Name = "UpdatePetTimelineEntry")]
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
        [HttpDelete("{id:length(24)}", Name = "DeletePetTimelineEntry")]
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

        /// <summary>Converte a entidade em response, anexando os links HATEOAS do recurso.</summary>
        private PetTimelineResponse ToResponse(PetTimelineEntry entry)
        {
            var links = new List<LinkDto>
            {
                new(Url.Link("GetPetTimelineById", new { id = entry.Id })!, "self", "GET"),
                new(Url.Link("UpdatePetTimelineEntry", new { id = entry.Id })!, "update_entry", "PUT"),
                new(Url.Link("DeletePetTimelineEntry", new { id = entry.Id })!, "delete_entry", "DELETE"),
                new(Url.Link("GetPetTimelineByPet", new { idPet = entry.IdPet })!, "pet_timeline", "GET")
            };

            return PetTimelineResponse.FromEntity(entry, links);
        }

        // Datas nos links em ISO-8601 UTC: independe de cultura e é lida de volta sem ambiguidade.
        private static string? ToIso(DateTime? value) =>
            value?.ToUtcAssumingUtc().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
    }
}