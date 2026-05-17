using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.dtos;
using ProntPet.Models;

namespace ProntPet.Controllers
{
    /// <summary>
    /// Gerencia o cadastro e as operações de pets vinculados a tutores.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : ControllerBase
    {
        
        private readonly AppDbContext _context;

        public PetController(AppDbContext context)
        {
            _context = context;
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
        [HttpGet]
        public async Task<IActionResult> GetPetsByTutor(int idTutor)
        {
            var pets = await _context
                .Pets.Where(r => r.IdTutor == idTutor).ToListAsync();

            return Ok(pets);
            
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound($"Pet de id {id} não encontrado!");
            return Ok(pet);
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
        /// <response code="200">Pet criado com sucesso.</response>
        /// <response code="404">Tutor informado não encontrado.</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PetRequest petRequest)
        {
            var tutorExists = await _context
                .Tutors.AnyAsync(t => t.Id == petRequest.IdTutor);

            if (!tutorExists)
            {
                return NotFound($"O tutor de Id {petRequest.IdTutor} não foi encontrado.");
            }

            var pet = petRequest.ToEntity();
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new {id = pet.Id}, pet);
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

            var updatedPet = request.ToEntity();

            var pet = await _context.Pets.FindAsync(id);

            if (pet == null) return NotFound($"Pet de id {id} não encontrado");

            pet.Update(updatedPet.Name, updatedPet.Species, updatedPet.Breed, updatedPet.BirthDate, 
                        updatedPet.Weight, updatedPet.Sex);

            await _context.SaveChangesAsync();
            return NoContent();
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
            var pet = await _context.Pets.FindAsync(id);

            if (pet == null) return NotFound($"Pet de id {id} não encontrado");

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
