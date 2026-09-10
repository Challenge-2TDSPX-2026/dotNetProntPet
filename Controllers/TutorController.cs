using Microsoft.AspNetCore.Mvc;
using ProntPet.Common;
using ProntPet.Dtos;
using ProntPet.dtos;
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
        /// Lista todos os tutores cadastrados.
        /// </summary>
        /// <remarks>
        /// Retorna os tutores convertidos para <see cref="TutorResponse"/>, sem expor a senha.
        /// </remarks>
        /// <returns>Lista de tutores.</returns>
        /// <response code="200">Tutores retornados com sucesso.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tutors = await _tutorService.GetAllAsync();
            var response = tutors.Select(t => TutorResponse.FromEntity(t));
            return Ok(response);
        }

        /// <summary>
        /// Obtém um tutor pelo identificador.
        /// </summary>
        /// <remarks>
        /// Busca um único tutor pelo ID e retorna os dados públicos (sem senha).
        /// </remarks>
        /// <param name="id">Identificador do tutor.</param>
        /// <returns>Dados do tutor encontrado.</returns>
        /// <response code="200">Tutor encontrado.</response>
        /// <response code="404">Tutor não encontrado.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _tutorService.GetByIdAsync(id);

            return result.Status switch
            {
                ServiceStatus.Ok => Ok(TutorResponse.FromEntity(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Cadastra um novo tutor.
        /// </summary>
        /// <remarks>
        /// Cria um registro de tutor com os dados informados no corpo da requisição.
        /// </remarks>
        /// <param name="tutorRequest">Dados do tutor a ser criado.</param>
        /// <returns>Tutor criado com o identificador gerado.</returns>
        /// <response code="201">Tutor criado com sucesso.</response>
        /// <response code="400">Dados inválidos na requisição.</response>
        [HttpPost]
        public async Task<IActionResult> Create(TutorRequest tutorRequest)
        {
            var result = await _tutorService.CreateAsync(tutorRequest);

            return result.Status switch
            {
                ServiceStatus.Ok => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, TutorResponse.FromEntity(result.Value!)),
                ServiceStatus.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        /// <summary>
        /// Atualiza os dados de um tutor existente.
        /// </summary>
        /// <remarks>
        /// Atualiza nome, CPF, telefone, e-mail, senha e endereço do tutor identificado por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador do tutor.</param>
        /// <param name="updatedTutor">Novos dados do tutor.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Tutor atualizado com sucesso.</response>
        /// <response code="404">Tutor não encontrado.</response>
        [HttpPut("{id}")]
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
        /// <remarks>
        /// Exclui permanentemente o tutor identificado por <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">Identificador do tutor.</param>
        /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
        /// <response code="204">Tutor removido com sucesso.</response>
        /// <response code="404">Tutor não encontrado.</response>
        [HttpDelete("{id}")]
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

    }
}
