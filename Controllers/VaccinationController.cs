using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.Dtos;
using ProntPet.Models;

namespace ProntPet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccinationController : ControllerBase
    {
        
        private readonly AppDbContext _context;

        public VaccinationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetVaccinationsByPet(int idPet)
        {
            var vaccinations = await _context
                .Vaccinations.Where(v => v.IdPet == idPet).ToListAsync();

            return Ok(vaccinations);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VaccinationRequest request)
        {
            var petExists = await _context
                .Pets.AnyAsync(p => p.Id == request.IdPet);

            if (!petExists)
            {
                return NotFound($"O pet de id {request.IdPet} não foi encontrado!");
            }

            var vaccination = request.ToEntity();
            _context.Vaccinations.Add(vaccination);
            await _context.SaveChangesAsync();

            return Ok(vaccination);

        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VaccinationRequest updatedVaccination)
        {
            var vaccination = await _context.Vaccinations.FindAsync(id);

            if (vaccination == null) return NotFound($"Vacinação de id {id} não encontrada!");

            vaccination.Update(updatedVaccination.VaccineName, updatedVaccination.ApplicationDate, 
                                updatedVaccination.ExpirationDate, updatedVaccination.Lot);

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vaccination = await _context.Vaccinations.FindAsync(id);

            if (vaccination == null) return NotFound($"Vacinação de id {id} não encontrada!");

            _context.Vaccinations.Remove(vaccination);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
