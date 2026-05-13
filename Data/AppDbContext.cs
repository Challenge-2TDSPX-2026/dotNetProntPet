using Microsoft.EntityFrameworkCore;
using ProntPet.Models;

namespace ProntPet.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Tutor> Tutors { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Vaccination> Vaccinations { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }

        
    }
}
