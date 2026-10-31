using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.dtos;
using ProntPet.Dtos;
using ProntPet.Models;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia o cadastro e as operações de pets vinculados a tutores.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : ControllerBase
    {
        private readonly IPetService _petService;

        public PetController(IPetService petService)
        {
            _petService = petService;
        }

        /// <summary>
        /// Lista os pets de um tutor com paginação, ordenação, filtros e links HATEOAS.
        /// </summary>
        /// <remarks>
        /// Padrão: 10 itens por página (máximo 50), ordenado por nome. Ordenações aceitas (<c>sortBy</c>): name, species, breed, birthDate, weight.
        /// Filtros: name e breed (contém o texto), species e sex (igual ao texto), todos sem diferenciar maiúsculas.
        /// Exemplo: <c>/api/Pet/tutor/1?species=gato&amp;sortBy=weight&amp;ascending=false</c>.
        /// </remarks>
        /// <param name="idTutor">Identificador do tutor.</param>
        /// <param name="query">Paginação, ordenação e filtros.</param>
        /// <response code="200">Página de pets retornada (pode ser vazia).</response>
        [HttpGet("tutor/{idTutor}", Name = "GetPetsByTutor")]
        [ProducesResponseType(typeof(PagedResponse<PetResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPetsByTutor(int idTutor, [FromQuery] PetQueryParameters query)
        {
            var result = await _petService.GetByTutorAsync(idTutor, query);

            if (result.Status != ServiceStatus.Ok)
                return BadRequest(result.Message);

            var page = result.Value!.Map(ToResponse);

            var filters = new Dictionary<string, object?>
            {
                ["idTutor"] = idTutor,
                ["name"] = query.Name,
                ["species"] = query.Species,
                ["breed"] = query.Breed,
                ["sex"] = query.Sex
            };

            var links = PaginationLinkBuilder.Build(
                v => Url.Link("GetPetsByTutor", v), query, page.TotalPages, filters);

            return Ok(PagedResponse<PetResponse>.From(page, links));
        }

        /// <summary>
        /// Obtém um pet pelo identificador.
        /// </summary>
        /// <remarks>
        /// Busca um único pet pelo seu ID, com links para o tutor, vacinações, prontuários e timeline.
        /// </remarks>
        /// <param name="id">Identificador do pet.</param>
        /// <response code="200">Pet encontrado.</response>
        /// <response code="404">Pet não encontrado.</response>
        [HttpGet("{id}", Name = "GetPetById")]
        [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _petService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Cadastra um novo pet.
        /// </summary>
        /// <remarks>
        /// Cria um pet vinculado ao tutor informado em <c>IdTutor</c>.
        /// Retorna 404 se o tutor não existir.
        /// </remarks>
        /// <param name="petRequest">Dados do pet a ser criado.</param>
        /// <response code="201">Pet criado com sucesso.</response>
        /// <response code="404">Tutor informado não encontrado.</response>
        /// <response code="400">Peso do pet menor que 0.</response>
        [HttpPost]
        [ProducesResponseType(typeof(PetResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] PetRequest petRequest)
        {
            var result = await _petService.CreateAsync(petRequest);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ToResponse(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Atualiza os dados de um pet existente.
        /// </summary>
        /// <remarks>
        /// Atualiza nome, espécie, raça, data de nascimento, peso e sexo do pet identificado por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador do pet.</param>
        /// <param name="request">Novos dados do pet.</param>
        /// <response code="204">Pet atualizado com sucesso.</response>
        /// <response code="404">Pet não encontrado.</response>
        [HttpPut("{id}", Name = "UpdatePet")]
        public async Task<IActionResult> Update(int id, [FromBody] PetUpdateRequest request)
        {
            var result = await _petService.UpdateAsync(id, request);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Remove um pet do sistema.
        /// </summary>
        /// <param name="id">Identificador do pet.</param>
        /// <response code="204">Pet removido com sucesso.</response>
        /// <response code="404">Pet não encontrado.</response>
        [HttpDelete("{id}", Name = "DeletePet")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _petService.DeleteAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => NoContent(),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>Converte a entidade em response, anexando os links HATEOAS do recurso.</summary>
        private PetResponse ToResponse(Pet pet)
        {
            var links = new List<LinkDto>
            {
                new(Url.Link("GetPetById", new { id = pet.Id })!, LinkRelations.Self, "GET"),
                new(Url.Link("UpdatePet", new { id = pet.Id })!, "update_pet", "PUT"),
                new(Url.Link("DeletePet", new { id = pet.Id })!, "delete_pet", "DELETE"),
                new(Url.Link("GetTutorById", new { id = pet.IdTutor })!, "tutor", "GET"),
                new(Url.Link("GetVaccinationsByPet", new { idPet = pet.Id })!, "vaccinations", "GET"),
                new(Url.Link("GetMedicalRecordsByPet", new { idPet = pet.Id })!, "medical_records", "GET"),
                new(Url.Link("GetPetTimelineByPet", new { idPet = pet.Id })!, "timeline", "GET")
            };

            return PetResponse.FromEntity(pet, links);
        }
    }
}
