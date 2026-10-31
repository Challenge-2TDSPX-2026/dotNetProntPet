using System.ComponentModel.DataAnnotations;

namespace ProntPet.Dtos;

public record class LoginRequest(
    [Required] [EmailAddress] string Email,
    [Required] string Password
);
