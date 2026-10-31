using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.dtos;
using ProntPet.Models;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia o cadastro e as operações de tutores (donos de pets).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TutorController : ControllerBase
    {
        private readonly ITutorService _tutorService;

        public TutorController(ITutorService tutorService)
        {
            _tutorService = tutorService;
        }

        /// <summary>
        /// Lista os tutores com paginação, ordenação, filtros e links HATEOAS.
        /// </summary>
        /// <remarks>
        /// Padrão: 10 itens por página (máximo 50), ordenado por nome. Ordenações aceitas (<c>sortBy</c>): name, email, cpf.
        /// Filtros: name, email e cpf (contém o texto, sem diferenciar maiúsculas).
        /// Exemplo: <c>/api/Tutor?name=maria&amp;sortBy=email&amp;ascending=false&amp;pageSize=5</c>.
        /// A senha nunca é retornada.
        /// </remarks>
        /// <param name="query">Paginação, ordenação e filtros.</param>
        /// <response code="200">Página de tutores retornada (pode ser vazia).</response>
        [HttpGet(Name = "GetTutors")]
        [ProducesResponseType(typeof(PagedResponse<TutorResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] TutorQueryParameters query)
        {
            var result = await _tutorService.GetAllAsync(query);

            if (result.Status != ServiceStatus.Ok)
                return BadRequest(result.Message);

            var page = result.Value!.Map(ToResponse);

            var filters = new Dictionary<string, object?>
            {
                ["name"] = query.Name,
                ["email"] = query.Email,
                ["cpf"] = query.Cpf
            };

            var links = PaginationLinkBuilder.Build(
                v => Url.Link("GetTutors", v), query, page.TotalPages, filters);

            return Ok(PagedResponse<TutorResponse>.From(page, links));
        }

        /// <summary>
        /// Obtém um tutor pelo identificador.
        /// </summary>
        /// <remarks>
        /// Busca um único tutor pelo ID e retorna os dados públicos (sem senha), com links relacionados.
        /// </remarks>
        /// <param name="id">Identificador do tutor.</param>
        /// <response code="200">Tutor encontrado.</response>
        /// <response code="404">Tutor não encontrado.</response>
        [HttpGet("{id}", Name = "GetTutorById")]
        [ProducesResponseType(typeof(TutorResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _tutorService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Cadastra um novo tutor.
        /// </summary>
        /// <param name="tutorRequest">Dados do tutor a ser criado.</param>
        /// <response code="201">Tutor criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [ProducesResponseType(typeof(TutorResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(TutorRequest tutorRequest)
        {
            var result = await _tutorService.CreateAsync(tutorRequest);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Atualiza os dados de um tutor existente.
        /// </summary>
        /// <param name="id">Identificador do tutor.</param>
        /// <param name="updatedTutor">Novos dados do tutor.</param>
        /// <response code="204">Tutor atualizado com sucesso.</response>
        /// <response code="404">Tutor não encontrado.</response>
        [HttpPut("{id}", Name = "UpdateTutor")]
        public async Task<IActionResult> Update(int id, [FromBody] TutorRequest updatedTutor)
        {
            var result = await _tutorService.UpdateAsync(id, updatedTutor);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Remove um tutor do sistema.
        /// </summary>
        /// <param name="id">Identificador do tutor.</param>
        /// <response code="204">Tutor removido com sucesso.</response>
        /// <response code="404">Tutor não encontrado.</response>
        [HttpDelete("{id}", Name = "DeleteTutor")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _tutorService.DeleteAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>Converte a entidade em response, anexando os links HATEOAS do recurso.</summary>
        private TutorResponse ToResponse(Tutor tutor)
        {
            var links = new List<LinkDto>
            {
                new(Url.Link("GetTutorById", new { id = tutor.Id })!, LinkRelations.Self, "GET"),
                new(Url.Link("UpdateTutor", new { id = tutor.Id })!, "update_tutor", "PUT"),
                new(Url.Link("DeleteTutor", new { id = tutor.Id })!, "delete_tutor", "DELETE"),
                new(Url.Link("GetPetsByTutor", new { idTutor = tutor.Id })!, "tutor_pets", "GET")
            };

            return TutorResponse.FromEntity(tutor, links);
        }
    }
}
