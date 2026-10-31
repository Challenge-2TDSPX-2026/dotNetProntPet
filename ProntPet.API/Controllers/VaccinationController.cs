using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Models;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia o histórico de vacinação dos pets.
    /// </summary>
    [Authorize]
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
        /// Lista as vacinações de um pet com paginação, ordenação, filtros e links HATEOAS.
        /// </summary>
        /// <remarks>
        /// Padrão: 10 itens por página (máximo 50), aplicadas mais recentemente primeiro
        /// (<c>sortBy=applicationDate&amp;ascending=false</c>). Ordenações aceitas: applicationDate, expirationDate, vaccineName, lot.
        /// Filtros: vaccineName e lot (contém o texto), appliedFrom e appliedTo (yyyy-MM-dd, inclusivos)
        /// e expired (true = vencidas, false = dentro da validade).
        /// Exemplo: <c>/api/Vaccination/pet/1?expired=false&amp;appliedFrom=2026-01-01&amp;pageSize=5</c>.
        /// </remarks>
        /// <param name="idPet">Identificador do pet.</param>
        /// <param name="query">Paginação, ordenação e filtros.</param>
        /// <response code="200">Página de vacinações retornada (pode ser vazia).</response>
        /// <response code="400">Parâmetros inválidos (ex.: appliedFrom maior que appliedTo).</response>
        [HttpGet("pet/{idPet}", Name = "GetVaccinationsByPet")]
        [ProducesResponseType(typeof(PagedResponse<VaccinationResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVaccinationsByPet(int idPet, [FromQuery] VaccinationQueryParameters query)
        {
            var result = await _vaccinationService.GetByPetAsync(idPet, query);

            if (result.Status != ServiceStatus.Ok)
                return BadRequest(result.Message);

            var page = result.Value!.Map(ToResponse);

            var filters = new Dictionary<string, object?>
            {
                ["idPet"] = idPet,
                ["vaccineName"] = query.VaccineName,
                ["lot"] = query.Lot,
                ["appliedFrom"] = query.AppliedFrom.ToIsoDate(),
                ["appliedTo"] = query.AppliedTo.ToIsoDate(),
                ["expired"] = query.Expired
            };

            var links = PaginationLinkBuilder.Build(
                v => Url.Link("GetVaccinationsByPet", v), query, page.TotalPages, filters);

            return Ok(PagedResponse<VaccinationResponse>.From(page, links));
        }

        /// <summary>
        /// Obtém uma vacinação pelo identificador.
        /// </summary>
        /// <param name="id">Identificador da vacinação.</param>
        /// <response code="200">Vacinação encontrada.</response>
        /// <response code="404">Vacinação não encontrada.</response>
        [HttpGet("{id}", Name = "GetVaccinationById")]
        [ProducesResponseType(typeof(VaccinationResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _vaccinationService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Registra uma nova vacinação para um pet.
        /// </summary>
        /// <param name="request">Dados da vacinação.</param>
        /// <response code="201">Vacinação registrada.</response>
        /// <response code="404">Pet não encontrado.</response>
        /// <response code="400">Datas inválidas (expiração anterior ou igual à aplicação).</response>
        [HttpPost]
        [ProducesResponseType(typeof(VaccinationResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] VaccinationRequest request)
        {
            var result = await _vaccinationService.CreateAsync(request);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Atualiza uma vacinação existente.
        /// </summary>
        /// <param name="id">Identificador da vacinação.</param>
        /// <param name="request">Novos dados da vacinação.</param>
        /// <response code="204">Vacinação atualizada.</response>
        /// <response code="404">Vacinação não encontrada.</response>
        [HttpPut("{id}", Name = "UpdateVaccination")]
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
        /// Remove uma vacinação.
        /// </summary>
        /// <param name="id">Identificador da vacinação.</param>
        /// <response code="204">Vacinação removida.</response>
        /// <response code="404">Vacinação não encontrada.</response>
        [HttpDelete("{id}", Name = "DeleteVaccination")]
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

        /// <summary>Converte a entidade em response, anexando os links HATEOAS do recurso.</summary>
        private VaccinationResponse ToResponse(Vaccination vaccination)
        {
            var links = new List<LinkDto>
            {
                new(Url.Link("GetVaccinationById", new { id = vaccination.Id })!, LinkRelations.Self, "GET"),
                new(Url.Link("UpdateVaccination", new { id = vaccination.Id })!, "update_vaccination", "PUT"),
                new(Url.Link("DeleteVaccination", new { id = vaccination.Id })!, "delete_vaccination", "DELETE"),
                new(Url.Link("GetPetById", new { id = vaccination.IdPet })!, "pet", "GET"),
                new(Url.Link("GetVaccinationsByPet", new { idPet = vaccination.IdPet })!, "pet_vaccinations", "GET")
            };

            return VaccinationResponse.FromEntity(vaccination, links);
        }
    }
}
