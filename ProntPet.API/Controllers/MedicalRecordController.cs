using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Models;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia os prontuários médicos dos pets.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        /// <summary>
        /// Lista os prontuários de um pet com paginação, ordenação, filtros e links HATEOAS.
        /// </summary>
        /// <remarks>
        /// Cada pet possui no máximo um prontuário (índice único), então a página tem 0 ou 1 item;
        /// a resposta segue o mesmo formato das demais listagens. Padrão: <c>sortBy=lastUpdate&amp;ascending=false</c>.
        /// Ordenações aceitas: lastUpdate, bloodType. Filtros: bloodType (igual), isCastrated (true/false)
        /// e search (contém o texto em alergias e doenças crônicas).
        /// </remarks>
        /// <param name="idPet">Identificador do pet.</param>
        /// <param name="query">Paginação, ordenação e filtros.</param>
        /// <response code="200">Página de prontuários retornada (pode ser vazia).</response>
        [HttpGet("pet/{idPet}", Name = "GetMedicalRecordsByPet")]
        [ProducesResponseType(typeof(PagedResponse<MedicalRecordResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMedicalRecordsByPet(int idPet, [FromQuery] MedicalRecordQueryParameters query)
        {
            var result = await _medicalRecordService.GetByPetAsync(idPet, query);

            if (result.Status != ServiceStatus.Ok)
                return BadRequest(result.Message);

            var page = result.Value!.Map(ToResponse);

            var filters = new Dictionary<string, object?>
            {
                ["idPet"] = idPet,
                ["bloodType"] = query.BloodType,
                ["isCastrated"] = query.IsCastrated,
                ["search"] = query.Search
            };

            var links = PaginationLinkBuilder.Build(
                v => Url.Link("GetMedicalRecordsByPet", v), query, page.TotalPages, filters);

            return Ok(PagedResponse<MedicalRecordResponse>.From(page, links));
        }

        /// <summary>
        /// Obtém um prontuário pelo identificador.
        /// </summary>
        /// <param name="id">Identificador do prontuário.</param>
        /// <response code="200">Prontuário encontrado.</response>
        /// <response code="404">Prontuário não encontrado.</response>
        [HttpGet("{id}", Name = "GetMedicalRecordById")]
        [ProducesResponseType(typeof(MedicalRecordResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _medicalRecordService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Cria o prontuário de um pet.
        /// </summary>
        /// <param name="recordRequest">Dados do prontuário.</param>
        /// <response code="201">Prontuário criado.</response>
        /// <response code="404">Pet não encontrado.</response>
        [HttpPost]
        [ProducesResponseType(typeof(MedicalRecordResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] MedicalRecordRequest recordRequest)
        {
            var result = await _medicalRecordService.CreateAsync(recordRequest);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Atualiza um prontuário existente.
        /// </summary>
        /// <param name="id">Identificador do prontuário.</param>
        /// <param name="updatedRecord">Novos dados do prontuário.</param>
        /// <response code="204">Prontuário atualizado.</response>
        /// <response code="404">Prontuário não encontrado.</response>
        [HttpPut("{id}", Name = "UpdateMedicalRecord")]
        public async Task<IActionResult> Update(int id, [FromBody] MedicalRecordRequest updatedRecord)
        {
            var result = await _medicalRecordService.UpdateAsync(id, updatedRecord);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Remove um prontuário.
        /// </summary>
        /// <param name="id">Identificador do prontuário.</param>
        /// <response code="204">Prontuário removido.</response>
        /// <response code="404">Prontuário não encontrado.</response>
        [HttpDelete("{id}", Name = "DeleteMedicalRecord")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _medicalRecordService.DeleteAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>Converte a entidade em response, anexando os links HATEOAS do recurso.</summary>
        private MedicalRecordResponse ToResponse(MedicalRecord record)
        {
            var links = new List<LinkDto>
            {
                new(Url.Link("GetMedicalRecordById", new { id = record.Id })!, LinkRelations.Self, "GET"),
                new(Url.Link("UpdateMedicalRecord", new { id = record.Id })!, "update_medical_record", "PUT"),
                new(Url.Link("DeleteMedicalRecord", new { id = record.Id })!, "delete_medical_record", "DELETE"),
                new(Url.Link("GetPetById", new { id = record.IdPet })!, "pet", "GET"),
                new(Url.Link("GetConsultationsByMedicalRecord", new { idRecord = record.Id })!, "consultations", "GET")
            };

            return MedicalRecordResponse.FromEntity(record, links);
        }
    }
}
