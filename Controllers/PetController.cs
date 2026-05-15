using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.dtos;
using ProntPet.Models;

namespace ProntPet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : ControllerBase
    {
        
        private readonly AppDbContext _context;

        public PetController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPetsByTutor(int idTutor)
        {
            var pets = await _context
                .Pets.Where(r => r.IdTutor == idTutor).ToListAsync();

            return Ok(pets);
            
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PetRequest petRequest)
        {
            // AnyAsync() retorna true se encontrar pelo menos 1 registro e false se não encontrar
            var tutorExists = await _context
                .Tutors.AnyAsync(t => t.Id == petRequest.IdTutor);

            if (!tutorExists)
            {
                return NotFound($"O tutor de Id {petRequest.IdTutor} não foi encontrado.");
            }

            var pet = petRequest.ToEntity();
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return Ok(pet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PetUpdateRequest updatedPet)
        {
            var pet = await _context.Pets.FindAsync(id);

            if (pet == null) return NotFound($"Pet de id {id} não encontrado");

            pet.Update(updatedPet.Name, updatedPet.Species, updatedPet.Breed, updatedPet.BirthDate.ToDateTime(TimeOnly.MinValue), 
                        updatedPet.Weight, updatedPet.Sex);

            await _context.SaveChangesAsync();
            return NoContent();
        }

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
