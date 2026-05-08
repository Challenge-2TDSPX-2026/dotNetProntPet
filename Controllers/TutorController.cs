using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProntPet.Data;
using ProntPet.Models;

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
            return Ok(tutors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tutor = await _context.Tutors.FindAsync(id);
            if (tutor == null) return NotFound();
            return Ok(tutor);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tutor tutor)
        {
            _context.Tutors.Add(tutor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tutor.Id }, tutor);
        }

    }
}
