using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Models;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia as consultas veterinárias vinculadas a prontuários e clínicas.
    /// </summary>
    [Authorize]
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
        /// Lista as consultas de um prontuário com paginação, ordenação, filtros e links HATEOAS.
        /// </summary>
        /// <remarks>
        /// Padrão: 10 itens por página (máximo 50), mais recentes primeiro
        /// (<c>sortBy=consultationDate&amp;ascending=false</c>). Ordenações aceitas: consultationDate, idClinic, diagnosis.
        /// Filtros: idClinic, from e to (yyyy-MM-dd, inclusivos) e search (contém o texto em sintomas, diagnóstico e observações).
        /// Exemplo: <c>/api/Consultation/medical-record/1?idClinic=2&amp;from=2026-01-01&amp;search=vômito</c>.
        /// </remarks>
        /// <param name="idRecord">Identificador do prontuário.</param>
        /// <param name="query">Paginação, ordenação e filtros.</param>
        /// <response code="200">Página de consultas retornada (pode ser vazia).</response>
        /// <response code="400">Parâmetros inválidos (ex.: from maior que to).</response>
        [HttpGet("medical-record/{idRecord}", Name = "GetConsultationsByMedicalRecord")]
        [ProducesResponseType(typeof(PagedResponse<ConsultationResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConsultationsByMedicalRecord(
            int idRecord, [FromQuery] ConsultationQueryParameters query)
        {
            var result = await _consultationService.GetByMedicalRecordAsync(idRecord, query);

            if (result.Status != ServiceStatus.Ok)
                return BadRequest(result.Message);

            var page = result.Value!.Map(ToResponse);

            var filters = new Dictionary<string, object?>
            {
                ["idRecord"] = idRecord,
                ["idClinic"] = query.IdClinic,
                ["from"] = query.From.ToIsoDate(),
                ["to"] = query.To.ToIsoDate(),
                ["search"] = query.Search
            };

            var links = PaginationLinkBuilder.Build(
                v => Url.Link("GetConsultationsByMedicalRecord", v), query, page.TotalPages, filters);

            return Ok(PagedResponse<ConsultationResponse>.From(page, links));
        }

        /// <summary>
        /// Obtém uma consulta pelo identificador.
        /// </summary>
        /// <param name="id">Identificador da consulta.</param>
        /// <response code="200">Consulta encontrada.</response>
        /// <response code="404">Consulta não encontrada.</response>
        [HttpGet("{id}", Name = "GetConsultationById")]
        [ProducesResponseType(typeof(ConsultationResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _consultationService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Registra uma nova consulta.
        /// </summary>
        /// <param name="consultationRequest">Dados da consulta.</param>
        /// <response code="201">Consulta registrada.</response>
        /// <response code="404">Prontuário ou clínica não encontrados.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ConsultationResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] ConsultationRequest consultationRequest)
        {
            var result = await _consultationService.CreateAsync(consultationRequest);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Atualiza uma consulta existente.
        /// </summary>
        /// <param name="id">Identificador da consulta.</param>
        /// <param name="request">Novos dados da consulta.</param>
        /// <response code="204">Consulta atualizada.</response>
        /// <response code="404">Consulta não encontrada.</response>
        [HttpPut("{id}", Name = "UpdateConsultation")]
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
        /// Remove uma consulta.
        /// </summary>
        /// <param name="id">Identificador da consulta.</param>
        /// <response code="204">Consulta removida.</response>
        /// <response code="404">Consulta não encontrada.</response>
        [HttpDelete("{id}", Name = "DeleteConsultation")]
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

        /// <summary>Converte a entidade em response, anexando os links HATEOAS do recurso.</summary>
        private ConsultationResponse ToResponse(Consultation consultation)
        {
            var links = new List<LinkDto>
            {
                new(Url.Link("GetConsultationById", new { id = consultation.Id })!, LinkRelations.Self, "GET"),
                new(Url.Link("UpdateConsultation", new { id = consultation.Id })!, "update_consultation", "PUT"),
                new(Url.Link("DeleteConsultation", new { id = consultation.Id })!, "delete_consultation", "DELETE"),
                new(Url.Link("GetMedicalRecordById", new { id = consultation.IdMedicalRecord })!, "medical_record", "GET"),
                new(Url.Link("GetClinicById", new { id = consultation.IdClinic })!, "clinic", "GET"),
                new(Url.Link("GetConsultationsByMedicalRecord", new { idRecord = consultation.IdMedicalRecord })!,
                    "medical_record_consultations", "GET")
            };

            return ConsultationResponse.FromEntity(consultation, links);
        }
    }
}
