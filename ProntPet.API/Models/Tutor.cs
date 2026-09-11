using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ProntPet.Models
{
    [Table("DB_TUTOR")]
    [Index(nameof(Cpf), IsUnique = true)]
    [Index(nameof(Phone), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class Tutor
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do Tutor é obrigatório")]
        [MaxLength(100)]
        [Column("NAME")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório")]
        [MaxLength(15)]
        [Column("CPF")]
        public string Cpf { get; set; }

        [Required(ErrorMessage = "O Telefone é obrigatório")]
        [MaxLength(15)]
        [Column("PHONE")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "O Email é obrigatório")]
        [MaxLength(150)]
        [Column("EMAIL")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A Senha é obrigatória")]
        [MaxLength(300)]
        [Column("PASSWORD")]
        public string Password { get; set; }

        [Required(ErrorMessage = "O Endereço é obrigatório")]
        [MaxLength(400)]
        [Column("ADDRESS")]
        public string Address { get; set; }

        // Um Tutor tem muitos Pets
        //public ICollection<Pet> Pets { get; set; } = new List<Pet>();


        public void Update(string name, string cpf, string phone, string email,
                            string password, string address)
        {
            this.Name = name;
            this.Cpf = cpf;
            this.Phone = phone;
            this.Email = email;
            this.Password = password;
            this.Address = address;
        }


    }
}
