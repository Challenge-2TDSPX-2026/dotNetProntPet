using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.dtos;
using ProntPet.Dtos;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia o cadastro e as operações de tutores (donos de pets).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TutorController : ControllerBase
    {
        
        private readonly AppDbContext _context;

        public TutorController(AppDbContext context)
        {
            _context = context;
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
            var tutors = await _context.Tutors.ToListAsync();

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
            var tutor = await _context.Tutors.FindAsync(id);
            if (tutor == null) return NotFound();
            return Ok(TutorResponse.FromEntity(tutor));
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
            var tutor = tutorRequest.ToEntity();
            _context.Tutors.Add(tutor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tutor.Id }, tutor);
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
            var tutor = await _context.Tutors.FindAsync(id);

            if (tutor == null) return NotFound();

            tutor.Update(updatedTutor.Name, updatedTutor.Cpf, updatedTutor.Phone, updatedTutor.Email, updatedTutor.Password, updatedTutor.Address);
            await _context.SaveChangesAsync();
            return NoContent();
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
            var tutor = await _context.Tutors.FindAsync(id);
            if (tutor == null) return  NotFound();
            _context.Tutors.Remove(tutor);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
