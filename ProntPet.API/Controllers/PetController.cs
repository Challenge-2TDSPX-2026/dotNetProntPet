using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.dtos;
using ProntPet.Services;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia o cadastro e as operações de pets vinculados a tutores.
    /// </summary>
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
        /// Lista os pets de um tutor.
        /// </summary>
        /// <remarks>
        /// Filtra os pets pelo identificador do tutor informado na query string.
        /// </remarks>
        /// <param name="idTutor">Identificador do tutor.</param>
        /// <returns>Lista de pets do tutor.</returns>
        /// <response code="200">Pets retornados com sucesso (pode ser lista vazia).</response>
        [HttpGet("tutor/{idTutor}")]
        public async Task<IActionResult> GetPetsByTutor(int idTutor)
        {
            var pets = await _petService.GetByTutorAsync(idTutor);
            return Ok(pets);
        }

        /// <summary>
        /// Obtém um pet pelo identificador.
        /// </summary>
        /// <remarks>
        /// Busca um único pet pelo seu ID. Retorna 404 se não existir.
        /// </remarks>
        /// <param name="id">Identificador do pet.</param>
        /// <returns>Dados do pet encontrado.</returns>
        /// <response code="200">Pet encontrado.</response>
        /// <response code="404">Pet não encontrado.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _petService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(result.Value),
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
        /// <returns>Pet criado com o identificador gerado.</returns>
        /// <response code="201">Pet criado com sucesso.</response>
        /// <response code="404">Tutor informado não encontrado.</response>
        /// <response code="400">Peso do pet menor que 0.</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PetRequest petRequest)
        {
            var result = await _petService.CreateAsync(petRequest);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value),
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
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Pet atualizado com sucesso.</response>
        /// <response code="404">Pet não encontrado.</response>
        [HttpPut("{id}")]
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
        /// <remarks>
        /// Exclui permanentemente o pet identificado por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador do pet.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Pet removido com sucesso.</response>
        /// <response code="404">Pet não encontrado.</response>
        [HttpDelete("{id}")]
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
    }
}
