using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.Models;
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
        /// Lista as clínicas com paginação, ordenação, filtros e links HATEOAS.
        /// </summary>
        /// <remarks>
        /// Padrão: 10 itens por página (máximo 50), ordenado por nome. Ordenações aceitas (<c>sortBy</c>): name, cnpj, address.
        /// Filtros: name, cnpj e address (contém o texto, sem diferenciar maiúsculas).
        /// Exemplo: <c>/api/Clinic?name=feliz&amp;sortBy=cnpj&amp;pageSize=5</c>.
        /// </remarks>
        /// <param name="query">Paginação, ordenação e filtros.</param>
        /// <response code="200">Página de clínicas retornada (pode ser vazia).</response>
        [HttpGet(Name = "GetClinics")]
        [ProducesResponseType(typeof(PagedResponse<ClinicResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] ClinicQueryParameters query)
        {
            var result = await _clinicService.GetAllAsync(query);

            if (result.Status != ServiceStatus.Ok)
                return BadRequest(result.Message);

            var page = result.Value!.Map(ToResponse);

            var filters = new Dictionary<string, object?>
            {
                ["name"] = query.Name,
                ["cnpj"] = query.Cnpj,
                ["address"] = query.Address
            };

            var links = PaginationLinkBuilder.Build(
                v => Url.Link("GetClinics", v), query, page.TotalPages, filters);

            return Ok(PagedResponse<ClinicResponse>.From(page, links));
        }

        /// <summary>
        /// Obtém uma clínica pelo identificador.
        /// </summary>
        /// <param name="id">Identificador da clínica.</param>
        /// <response code="200">Clínica encontrada.</response>
        /// <response code="404">Clínica não encontrada.</response>
        [HttpGet("{id}", Name = "GetClinicById")]
        [ProducesResponseType(typeof(ClinicResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _clinicService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Cadastra uma nova clínica.
        /// </summary>
        /// <param name="clinicRequest">Dados da clínica a ser criada.</param>
        /// <response code="201">Clínica criada com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ClinicResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] ClinicRequest clinicRequest)
        {
            var result = await _clinicService.CreateAsync(clinicRequest);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Atualiza os dados de uma clínica existente.
        /// </summary>
        /// <param name="id">Identificador da clínica.</param>
        /// <param name="updatedClinicRequest">Novos dados da clínica.</param>
        /// <response code="204">Clínica atualizada com sucesso.</response>
        /// <response code="404">Clínica não encontrada.</response>
        [HttpPut("{id}", Name = "UpdateClinic")]
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
        /// <param name="id">Identificador da clínica.</param>
        /// <response code="204">Clínica removida com sucesso.</response>
        /// <response code="404">Clínica não encontrada.</response>
        [HttpDelete("{id}", Name = "DeleteClinic")]
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

        /// <summary>Converte a entidade em response, anexando os links HATEOAS do recurso.</summary>
        private ClinicResponse ToResponse(Clinic clinic)
        {
            var links = new List<LinkDto>
            {
                new(Url.Link("GetClinicById", new { id = clinic.Id })!, LinkRelations.Self, "GET"),
                new(Url.Link("UpdateClinic", new { id = clinic.Id })!, "update_clinic", "PUT"),
                new(Url.Link("DeleteClinic", new { id = clinic.Id })!, "delete_clinic", "DELETE")
            };

            return ClinicResponse.FromEntity(clinic, links);
        }
    }
}
