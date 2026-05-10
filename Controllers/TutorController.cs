using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.dtos;
using ProntPet.Dtos;

namespace ProntPet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TutorController : ControllerBase
    {
        
        private readonly AppDbContext _context;

        public TutorController(AppDbContext context)
        {
            _context = context;
        }

        // async indica que o método suporta await
        /* 
            'await' Busca os tutores no banco. Enquanto o banco responde, não 
            bloqueie a thread da API. Quando terminar, continue daqui com os dados 
            carregados.
        */
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tutors = await _context.Tutors.ToListAsync();

            var response = tutors.Select(t => TutorResponse.FromEntity(t));

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tutor = await _context.Tutors.FindAsync(id);
            if (tutor == null) return NotFound();
            return Ok(TutorResponse.FromEntity(tutor));
        }

        [HttpPost]
        public async Task<IActionResult> Create(TutorRequest tutorRequest)
        {
            var tutor = tutorRequest.ToEntity();
            _context.Tutors.Add(tutor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tutor.Id }, tutor);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TutorRequest updatedTutor)
        {
            var tutor = await _context.Tutors.FindAsync(id);

            if (tutor == null) return NotFound();

            tutor.Update(updatedTutor.Name, updatedTutor.Cpf, updatedTutor.Phone, updatedTutor.Email, updatedTutor.Password, updatedTutor.Address);
            await _context.SaveChangesAsync();
            return NoContent();
        }

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
