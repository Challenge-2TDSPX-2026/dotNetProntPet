using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProntPet.Models;

public class Clinic
{

    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("NAME")]
    public string Name { get; set; }

    [Required]
    [MaxLength(18)]
    [Column("CNPJ")]
    public string Cnpj { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("ADDRESS")]
    public string Address { get; set; }


    public void Update(string name, string address)
    {
        this.Name = name;
        this.Address = address;
    }

}
