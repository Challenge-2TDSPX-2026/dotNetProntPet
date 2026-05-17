using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.Models;

namespace ProntPet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClinicController : ControllerBase
    {

        private readonly AppDbContext _context;

        public ClinicController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clinics = await _context.Clinics.ToListAsync();
            return Ok(clinics);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();
            return Ok(clinic);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Clinic clinic)
        {
            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = clinic.Id }, clinic);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Clinic updatedClinic)
        {
            // verifica se existe
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();
            // chama o update
            clinic.Update(updatedClinic.Name, updatedClinic.Address);
            // salva
            await _context.SaveChangesAsync();
            // retorna nocontent
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();

            _context.Clinics.Remove(clinic);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
