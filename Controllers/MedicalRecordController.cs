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
    public class MedicalRecordController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MedicalRecordController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMedicalRecordsByPet(int idPet)
        {
            var records = await _context
                .MedicalRecords.Where(mc => mc.IdPet == idPet).ToListAsync();

            return Ok(records);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MedicalRecordRequest recordRequest)
        {
            var petExists = await _context
                .Pets.AnyAsync(p => p.Id == recordRequest.IdPet);

            if (!petExists)
            {
                return NotFound($"O pet de id {recordRequest.IdPet} não foi encontrado!");
            }

            var record = recordRequest.ToEntity();
            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(record);

        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MedicalRecordRequest updatedRecordRequest)
        {

            var updatedRecord = updatedRecordRequest.ToEntity(); 

            var record = await _context.MedicalRecords.FindAsync(id);

            if (record == null) return NotFound($"O Protuário de id {id} não encontrado!");

            record.Update(updatedRecord.BloodType, updatedRecord.Allergies, 
                            updatedRecord.ChronicDiseases, updatedRecord.IsCastrated, 
                            updatedRecord.MicrochipCode, updatedRecord.LastUpdate);

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);

            if (record == null) return NotFound($"Protuário de id {id} não encontrado!");

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
